using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NsqClient.Commands;
using NsqClient.Exceptions;
using NsqClient.Frames;
using NsqClient.Responses;

namespace NsqClient
{
    public class NsqConnection : IDisposable
    {
        private readonly NsqConnectionOptions options;
        private TcpClient client;
        private Task loopTask;
        private NetworkStream stream;
        private FrameReader reader;
        private IdentifyResponse identify;
        private bool isConnected;
        private bool isExiting;
        private TraceSource tracer = new TraceSource(nameof(NsqConnection), SourceLevels.All);
        
        public event EventHandler<NsqMessageEventArgs> OnMessage;
        public event EventHandler<NsqErrorEventArgs> OnError;
        public event EventHandler<NsqDisconnectionEventArgs> OnDisconnected;
        public event EventHandler<NsqReconnectionEventArgs> OnReconnected;

        public NsqConnection(NsqConnectionOptions options)
        {
            this.options = options;
        }

        public async Task Connect()
        {
            this.client = new TcpClient()
            {
                SendTimeout = 3000,
                ReceiveTimeout = 3000
            };

            await this.client.ConnectAsync(this.options.Hostname, this.options.Port);

            this.stream = this.client.GetStream();
            this.reader = new FrameReader(this.stream);

            await PerformHandshake();
            await Subscribe();
            await SendReady();

            if (this.loopTask == null)
            {
                this.loopTask = Task.Run(ReadLoop);
            }
        }

        public Task Publish(string topicName, string body)
        {
            return Publish(topicName, Encoding.UTF8.GetBytes(body));
        }

        public async Task Publish(string topicName, byte[] body)
        {
            await WriteProtocolCommand(new PublishCommand(topicName, body));
        }

        internal Task WriteProtocolCommand(IToBytes command)
        {
            byte[] bytes = command.ToBytes();
            return this.stream.WriteAsync(bytes, 0, bytes.Length);
        }

        private async Task PerformHandshake()
        {
            await WriteProtocolCommand(new ProtocolVersion());
            await WriteProtocolCommand(new IdentifyCommand());
            
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

        private async Task Subscribe()
        {
            await WriteProtocolCommand(new SubscribeCommand(this.options.Topic, this.options.Channel));

            Frame frame = await this.reader.ReadNext();

            if (frame is ErrorFrame error)
            {
                throw new NsqException("Unexpected response while subscribing: " + error.Message);
            }
        }

        private async Task SendReady()
        {
            await WriteProtocolCommand(new ReadyCommand(this.options.MaxInFlight));
        }

        private async Task ReadLoop()
        {
            while (!this.isExiting)
            {
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
                            this.tracer.TraceInformation("Not attempting reconnection because exiting");
                            break;
                        }
                        
                        await Reconnect();
                    }
                    else
                    {
                        this.OnError?.Invoke(this, new NsqErrorEventArgs(ex));
                    }
                }
            }
        }

        private async Task Reconnect()
        {
            this.client.Dispose();
            this.isConnected = false;
            
            this.OnDisconnected?.Invoke(this, new NsqDisconnectionEventArgs(true));

            int attempt = 0;
            DateTimeOffset start = DateTimeOffset.UtcNow;

            while (!this.isConnected)
            {
                attempt++;
                this.tracer.TraceInformation("Reconnection attempt {0}", attempt);
                
                try
                {
                    await Connect();
                    this.isConnected = true;
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
                            this.tracer.TraceInformation("Interrupting reconnection for exiting");
                            return;
                        }
                        
                        // keep trying
                    }
                    else
                    {
                        this.OnError?.Invoke(this, new NsqErrorEventArgs(ex));
                    }
                }
            }

            TimeSpan interval = DateTimeOffset.UtcNow - start;
            
            this.OnReconnected?.Invoke(this, new NsqReconnectionEventArgs(attempt, interval));
        }

        private async Task ProcessNextFrame()
        {
            Frame frame = await this.reader.ReadNext();

            if (frame is ResponseFrame responseFrame)
            {
                if (responseFrame.Type == ResponseType.Heartbeat)
                {
                    await WriteProtocolCommand(new NopCommand());
                }

                // Otherwise, this is OK or CLOSE_WAIT, which can be ignored
            }
            else if (frame is MessageFrame messageFrame)
            {
                await RaiseMessageEvent(messageFrame);
            }
            else if (frame is ErrorFrame errorFrame)
            {
                RaiseErrorEvent(errorFrame);
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

        private void RaiseErrorEvent(ErrorFrame frame)
        {
            NsqException exception = new NsqException(frame.Message);
            this.OnError?.Invoke(this, new NsqErrorEventArgs(exception));
        }

        public void Dispose()
        {
            this.isExiting = true;
            this.client.Close();
            this.OnDisconnected?.Invoke(this, new NsqDisconnectionEventArgs(false));
        }
    }
}
