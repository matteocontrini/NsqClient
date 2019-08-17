using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using NsqClient.Commands;
using NsqClient.Exceptions;
using NsqClient.Frames;
using NsqClient.Responses;

namespace NsqClient
{
    public class NsqConnection
    {
        private readonly string nsqHostname;
        private readonly int nsqPort;
        private TcpClient client;
        private Task readLoop;
        private NetworkStream stream;
        private FrameReader reader;
        private IdentifyResponse identify;

        public NsqConnection(string nsqHostname, int nsqPort)
        {
            this.nsqHostname = nsqHostname;
            this.nsqPort = nsqPort;
        }

        public async Task Connect()
        {
            this.client = new TcpClient();

            await this.client.ConnectAsync(this.nsqHostname, this.nsqPort);

            this.stream = this.client.GetStream();
            this.reader = new FrameReader(this.stream);

            await PerformHandshake();
            
            this.readLoop = Task.Run(ReadLoop);
        }

        private async Task PerformHandshake()
        {
            await WriteProtocolCommand(new ProtocolVersion());
            await WriteProtocolCommand(new IdentifyCommand());
            
            ResponseFrame frame = await this.reader.ReadNext() as ResponseFrame;

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

        private Task WriteProtocolCommand(IToBytes command)
        {
            byte[] bytes = command.ToBytes();
            return this.stream.WriteAsync(bytes, 0, bytes.Length);
        }

        private async Task ReadLoop()
        {
//            while (true)
//            {
//                Frame frame = await this.reader.ReadNext();
//
//                if (frame is ResponseFrame f)
//                {
//                }
//            }
        }
    }
}
