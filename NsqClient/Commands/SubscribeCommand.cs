using System.Text;

namespace NsqClient.Commands
{
    public class SubscribeCommand : ICommand
    {
        private string template = "SUB {0} {1}\n";
        
        private readonly string topic;
        private readonly string channel;

        public SubscribeCommand(string topic, string channel)
        {
            this.topic = topic;
            this.channel = channel;
        }

        public byte[] ToBytes()
        {
            string payload = string.Format(this.template, this.topic, this.channel);
            return Encoding.ASCII.GetBytes(payload);
        }
    }
}
