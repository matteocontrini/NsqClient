using System.Text;

namespace NsqClient.Commands
{
    public class FinishCommand : ICommand
    {
        private string template = "FIN {0}\n";
        
        private readonly string messageId;

        public FinishCommand(string messageId)
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
