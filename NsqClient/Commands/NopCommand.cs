using System.Text;

namespace NsqClient.Commands
{
    public class NopCommand : IToBytes
    {
        private string payload = "NOP\n";


        public byte[] ToBytes()
        {
            return Encoding.ASCII.GetBytes(this.payload);
        }
    }
}
