using System.Text;

namespace NsqClient.Commands
{
    public class RequeueCommand : IToBytes
    {
        private string template = "REQ {0} {1}\n";
        
        private readonly string messageId;
        private readonly int delay;

        public RequeueCommand(string messageId, int delay)
        {
            this.messageId = messageId;
            this.delay = delay;
        }

        public byte[] ToBytes()
        {
            string payload = string.Format(this.template, this.messageId, this.delay);
            return Encoding.ASCII.GetBytes(payload);
        }
    }
}
