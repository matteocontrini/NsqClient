using System.Text;

namespace NsqClient.Commands
{
    internal class TouchCommand : ICommand
    {
        private string template = "TOUCH {0}\n";
        
        private readonly string messageId;

        public TouchCommand(string messageId)
        {
            this.messageId = messageId;
        }

        public byte[] ToBytes()
        {
            string payload = string.Format(this.template, this.messageId);
            return Encoding.ASCII.GetBytes(payload);
        }
    }
}
