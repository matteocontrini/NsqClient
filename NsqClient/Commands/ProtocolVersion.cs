using System.Text;

namespace NsqClient.Commands
{
    public class ProtocolVersion : ICommand
    {
        // [space][space][V][2]
        private readonly byte[] payload = Encoding.ASCII.GetBytes("  V2"); 


        public byte[] ToBytes()
        {
            return this.payload;
        }
    }
}
