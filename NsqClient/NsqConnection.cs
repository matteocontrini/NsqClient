using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NsqClient.Commands;
using NsqClient.Exceptions;
using NsqClient.Frames;
using NsqClient.Responses;

namespace NsqClient
{
    internal class NsqConnection
    {
        // TODO: make topic name/topic classes
        private readonly NsqClientOptions options;
        private TcpClient client;
        private Task loopTask;
        private NetworkStream stream;
        private FrameReader reader;
        private IdentifyResponse identify;
        private int isStarted;
        private bool isConnected;
        private bool isExiting;
        private readonly TraceSource tracer;
        private readonly ConcurrentQueue<TaskCompletionSource<bool>> callbacksQueue;

        public event EventHandler<NsqMessageEventArgs> OnMessage;
        public event EventHandler<NsqErrorEventArgs> OnError;
        public event EventHandler<NsqDisconnectionEventArgs> OnDisconnected;
        public event EventHandler<NsqReconnectionEventArgs> OnReconnected;

        internal NsqConnection(NsqClientOptions options)
        {
            this.options = options;
            this.callbacksQueue = new ConcurrentQueue<TaskCompletionSource<bool>>();
            this.tracer = new TraceSource(nameof(NsqConnection), SourceLevels.All);
        }

        internal Task FirstConnect()
        {
            int wasAlreadyStarted = Interlocked.CompareExchange(ref this.isStarted, 1, 0);
            
            if (wasAlreadyStarted == 1)
            {
                this.tracer.TraceEvent(TraceEventType.Warning, 0, "First connection was requested but it has already been made");
                return Task.CompletedTask;
            }
            else
            {
                return Connect();
            }
        }

        private async Task Connect()
        {
            this.tracer.TraceInformation("Starting TCP connection...");
            
            this.client = new TcpClient()
            {
                SendTimeout = 3000,
                ReceiveTimeout = 3000
            };

            await this.client.ConnectAsync(this.options.Hostname, this.options.Port);
            
            this.tracer.TraceEvent(TraceEventType.Verbose, 0, "Connected. Preparing connection...");

            this.stream = this.client.GetStream();
            this.reader = new FrameReader(this.stream);

            await PerformHandshake();

            if (this.options is NsqConsumerOptions consumerOptions)
            {
                await Subscribe(consumerOptions.Topic, consumerOptions.Channel);
                await SendReady(consumerOptions.MaxInFlight);
            }

            this.tracer.TraceInformation("Connection is ready");
            this.isConnected = true;

            if (this.loopTask == null)
            {
                this.tracer.TraceEvent(TraceEventType.Verbose, 0, "Starting IO loop");
                this.loopTask = Task.Run(ReadLoop);
            }
        }

        internal Task Publish(string topicName, string body)
        {
            return Publish(topicName, Encoding.UTF8.GetBytes(body));
        }

        internal async Task Publish(string topicName, byte[] body)
        {
            if (!this.isConnected || this.isExiting)
            {
                throw new NsqException("The connection is not ready");
            }
            
            this.tracer.TraceInformation("Publishing message to topic {0}", topicName);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.callbacksQueue.Enqueue(tcs);

            try
            {
                await WriteProtocolCommand(new PublishCommand(topicName, body));
            }
            catch (Exception ex)
            {
                this.tracer.TraceEvent(TraceEventType.Error, 0, "Publishing failed with exception: {0}", ex);
                throw new NsqException("Publishing failed. See inner exception", ex);
            }

            // Wait for OK or fail
            await tcs.Task;
            
            this.tracer.TraceInformation("Published message to topic {0}", topicName);
        }

        internal Task WriteProtocolCommand(ICommand command)
        {
            byte[] bytes = command.ToBytes();
            return this.stream.WriteAsync(bytes, 0, bytes.Length);
        }

        internal Task SetMaxInFlight(int value)
        {
            if (this.options is NsqConsumerOptions consumerOptions)
            {
                consumerOptions.MaxInFlight = value;
                return this.SendReady(value);
            }

            return Task.CompletedTask;
        }

        private async Task PerformHandshake()
        {
            await WriteProtocolCommand(new ProtocolVersion());

            if (this.options is NsqConsumerOptions consumerOptions)
            {
                await WriteProtocolCommand(new IdentifyCommand(consumerOptions.MsgTimeout));
            }
            else
            {
                await WriteProtocolCommand(new IdentifyCommand());
            }
            
            Frame frame = await this.reader.ReadNext();

            if (frame is ErrorFrame errorFrame)
            {
                throw new NsqException("Error during handshake: " + errorFrame.Message);   
            }
            else if (frame is ResponseFrame responseFrame)
            {
                this.identify = IdentifyResponse.ParseWithFrame(responseFrame);

                if (this.identify.AuthRequired)
                {
                    throw new NsqException("Authentication is not supported");
                }
            }
            else
            {
                throw new NsqException("Unexpected response during handshake");
            }
        }

        private async Task Subscribe(string topic, string channel)
        {
            await WriteProtocolCommand(new SubscribeCommand(topic, channel));

            Frame frame = await this.reader.ReadNext();

            if (frame is ErrorFrame error)
            {
                throw new NsqException("Unexpected response while subscribing: " + error.Message);
            }
        }

        private async Task SendReady(int maxInFlight)
        {
            if (maxInFlight > this.identify.MaxRdyCount)
            {
                maxInFlight = this.identify.MaxRdyCount;
            }
            
            await WriteProtocolCommand(new ReadyCommand(maxInFlight));
        }

        private async Task ReadLoop()
        {
            while (!this.isExiting)
            {
                this.tracer.TraceEvent(TraceEventType.Verbose, 0, "Reading next frame");

                try
                {
                    await ProcessNextFrame();
                }
                catch (Exception ex)
                {
                    this.tracer.TraceEvent(TraceEventType.Error, 0, "Exception in NsqConnection.ReadLoop: {0}", ex);
                    
                    if (ex is SocketException ||
                        ex is IOException ||
                        ex is ObjectDisposedException)
                    {
                        if (this.isExiting)
                        {
                            Disconnect(willReconnect: false);
                            break;
                        }
                     
                        Disconnect(willReconnect: true);
                        await Reconnect();
                    }
                    else
                    {
                        this.OnError?.Invoke(this, new NsqErrorEventArgs(ex));
                    }
                }
            }
        }

        private void Disconnect(bool willReconnect)
        {
            this.client.Dispose();
            this.isConnected = false;
            
            this.OnDisconnected?.Invoke(this, new NsqDisconnectionEventArgs(willReconnect));
            FailCallbacks();   
        }

        private async Task Reconnect()
        {
            int attempt = 0;
            DateTimeOffset start = DateTimeOffset.UtcNow;

            while (!this.isConnected)
            {
                attempt++;
                this.tracer.TraceEvent(TraceEventType.Verbose, 0, "Reconnection attempt {0}", attempt);
                
                try
                {
                    await Connect();
                }
                catch (Exception ex)
                {
                    this.tracer.TraceEvent(TraceEventType.Error, 0, "Exception while reconnecting: {0}", ex);
                    
                    if (ex is SocketException ||
                        ex is IOException ||
                        ex is ObjectDisposedException)
                    {
                        if (this.isExiting)
                        {
                            this.tracer.TraceEvent(TraceEventType.Verbose, 0, "Interrupting reconnection for exiting");
                            return;
                        }
                        
                        // keep trying
                    }
                    else
                    {
                        this.OnError?.Invoke(this, new NsqErrorEventArgs(ex));
                    }

                    // Wait before retrying to avoid getting in a "fast loop"
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            TimeSpan interval = DateTimeOffset.UtcNow - start;
            this.tracer.TraceInformation("Reconnected after {0} attempts and {1} seconds", attempt, interval.TotalSeconds);
            
            this.OnReconnected?.Invoke(this, new NsqReconnectionEventArgs(attempt, interval));
        }

        private async Task ProcessNextFrame()
        {
            Frame frame = await this.reader.ReadNext();

            if (frame is ResponseFrame responseFrame)
            {
                this.tracer.TraceEvent(TraceEventType.Verbose, 0, "Read ResponseFrame of type {0}", responseFrame.Type);
                
                if (responseFrame.Type == ResponseType.Heartbeat)
                {
                    await WriteProtocolCommand(new NopCommand());
                }
                else if (responseFrame.Type == ResponseType.Ok)
                {
                    // If publishing, unlock the Publish task that is awaiting
                    if (this.callbacksQueue.TryDequeue(out TaskCompletionSource<bool> tcs))
                    {
                        tcs.TrySetResult(true);
                    }
                }
            }
            else if (frame is MessageFrame messageFrame)
            {
                this.tracer.TraceEvent(TraceEventType.Verbose, 0, "Read MessageFrame with ID {0}", messageFrame.MessageId);
                
                await RaiseMessageEvent(messageFrame);
            }
            else if (frame is ErrorFrame errorFrame)
            {
                // Note: we don't always get here, because the connection is closed by nsqd when errors happen
                // https://nsq.io/clients/building_client_libraries.html#a-brief-interlude-on-errors
                
                this.tracer.TraceEvent(TraceEventType.Verbose, 0, "Read ErrorFrame with message {0}", errorFrame.Message);
                
                NsqException exception = new NsqException(errorFrame.Message);
                
                RaiseErrorEvent(exception);
                
                // If publishing, throw the exception
                if (this.callbacksQueue.TryDequeue(out TaskCompletionSource<bool> tcs))
                {
                    tcs.TrySetException(exception);
                }
            }
        }

        private async Task RaiseMessageEvent(MessageFrame frame)
        {
            EventHandler<NsqMessageEventArgs> handler = this.OnMessage;
            NsqMessageEventArgs args = new NsqMessageEventArgs(this, frame);

            if (handler == null)
            {
                await args.Requeue();
            }
            else
            {
                handler.Invoke(this, args);
            }
        }

        private void RaiseErrorEvent(NsqException exception)
        {
            this.OnError?.Invoke(this, new NsqErrorEventArgs(exception));
        }
        
        private void FailCallbacks()
        {
            while (this.callbacksQueue.TryDequeue(out TaskCompletionSource<bool> tcs))
            {
                tcs.TrySetException(new NsqException("Connection was lost"));
            }
        }

        internal void Close()
        {
            this.tracer.TraceInformation("Closing everything... Bye");

            this.isExiting = true;
            
            Disconnect(willReconnect: false);
        }
    }
}
