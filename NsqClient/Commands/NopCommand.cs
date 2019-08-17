using System.Text;

namespace NsqClient.Commands
{
    public class NopCommand : IToBytes
    {
        private readonly byte[] payload = Encoding.ASCII.GetBytes("NOP\n");


        public byte[] ToBytes()
        {
            return this.payload;
        }
    }
}
