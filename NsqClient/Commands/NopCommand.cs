using System.Text;

namespace NsqClient.Commands
{
    internal class NopCommand : ICommand
    {
        private readonly byte[] payload = Encoding.ASCII.GetBytes("NOP\n");


        public byte[] ToBytes()
        {
            return this.payload;
        }
    }
}
