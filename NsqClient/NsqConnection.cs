using System;
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

        public NsqConnection(NsqConnectionOptions options)
        {
            this.options = options;
        }

        public async Task Connect()
        {
            this.client = new TcpClient();

            await this.client.ConnectAsync(this.options.Hostname, this.options.Port);

            this.stream = this.client.GetStream();
            this.reader = new FrameReader(this.stream);

            await PerformHandshake();
            await Subscribe();
            
            this.loopTask = Task.Run(ReadLoop);
        }

        private Task WriteProtocolCommand(IToBytes command)
        {
            byte[] bytes = command.ToBytes();
            return this.stream.WriteAsync(bytes, 0, bytes.Length);
        }

        private async Task PerformHandshake()
        {
            await WriteProtocolCommand(new ProtocolVersion());
            await WriteProtocolCommand(new IdentifyCommand());
            
            ResponseFrame frame = await this.reader.ReadNext() as ResponseFrame;

            // TODO: might be error
            
            if (frame is null)
            {
                throw new NsqException("Unexpected response during handshake");
            }

            this.identify = IdentifyResponse.ParseWithFrame(frame);

            if (this.identify.AuthRequired)
            {
                throw new NsqException("Authentication is not supported");
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

        private async Task ReadLoop()
        {
            while (true)
            {
                Frame frame = await this.reader.ReadNext();

                if (frame is ResponseFrame resp)
                {
                    if (resp.IsHeartbeat())
                    {
                        await WriteProtocolCommand(new NopCommand());
                    }
                }
            }
        }

        public void Dispose()
        {
            this.client.Close();
            // TODO: stop loop
        }
    }
}
