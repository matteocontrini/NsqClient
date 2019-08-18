using System.Text;

namespace NsqClient.Commands
{
    public class ReadyCommand : ICommand
    {
        private string template = "RDY {0}\n";
        
        private readonly int count;

        public ReadyCommand(int count)
        {
            this.count = count;
        }

        public byte[] ToBytes()
        {
            string payload = string.Format(this.template, this.count);
            return Encoding.ASCII.GetBytes(payload);
        }
    }
}
