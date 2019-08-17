using System.Text;

namespace NsqClient.Commands
{
    public class ProtocolVersion : IToBytes
    {
        private string payload = "  V2"; // [space][space][V][2]


        public byte[] ToBytes()
        {
            return Encoding.ASCII.GetBytes(this.payload);
        }
    }
}
