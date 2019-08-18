using System.Text;

namespace NsqClient.Commands
{
    public class NopCommand : ICommand
    {
        private readonly byte[] payload = Encoding.ASCII.GetBytes("NOP\n");


        public byte[] ToBytes()
        {
            return this.payload;
        }
    }
}
