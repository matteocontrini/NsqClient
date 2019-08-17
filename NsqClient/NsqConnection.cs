using System;
using System.IO;
using System.Net.Sockets;
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
            while (true)
            {
                try
                {
                    await ProcessNextFrame();
                }
                catch (Exception ex)
                {
                    if (ex is SocketException ||
                        ex is IOException)
                    {
                        await Reconnect();    
                    }
                    else
                    {
                        OnError?.Invoke(this, new NsqErrorEventArgs(ex));
                    }
                }
            }
        }

        private async Task Reconnect()
        {
            this.client.Dispose();
            this.isConnected = false;
            
            this.OnDisconnected?.Invoke(this, new NsqDisconnectionEventArgs(true));

            int attempts = 0;
            DateTimeOffset start = DateTimeOffset.UtcNow;

            while (!this.isConnected)
            {
                attempts++;
                
                try
                {
                    await Connect();
                    this.isConnected = true;
                }
                catch (Exception ex)
                {
                    if (ex is SocketException ||
                        ex is IOException)
                    {
                        // keep trying    
                    }
                    else
                    {
                        OnError?.Invoke(this, new NsqErrorEventArgs(ex));
                    }
                }
            }

            TimeSpan interval = DateTimeOffset.UtcNow - start;
            
            this.OnReconnected?.Invoke(this, new NsqReconnectionEventArgs(attempts, interval));
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
            OnError?.Invoke(this, new NsqErrorEventArgs(exception));
        }

        public void Dispose()
        {
            this.client.Close();
            // TODO: stop loop
        }
    }
}
