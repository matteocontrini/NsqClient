using System;
using System.Linq;
using System.Text;

namespace NsqClient.Commands
{
    public class PublishCommand : ICommand
    {
        private readonly byte[] prefix = Encoding.ASCII.GetBytes("PUB ");
        private readonly byte[] lineFeed = Encoding.ASCII.GetBytes("\n");

        private readonly string topic;
        private readonly byte[] body;

        public PublishCommand(string topic, byte[] body)
        {
            this.topic = topic;
            this.body = body;
        }

        public byte[] ToBytes()
        {
            byte[] bodyLengthBytes = BitConverter.GetBytes(this.body.Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bodyLengthBytes);
            }

            return this.prefix
                .Concat(Encoding.ASCII.GetBytes(this.topic))
                .Concat(this.lineFeed)
                .Concat(bodyLengthBytes)
                .Concat(this.body)
                .ToArray();
        }
    }
}
