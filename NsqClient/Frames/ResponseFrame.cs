using System.Linq;
using System.Text;

namespace NsqClient.Frames
{
    class ResponseFrame : Frame
    {
        private static readonly byte[] HEARTBEAT_BYTES = Encoding.ASCII.GetBytes("_heartbeat_");
        
        public byte[] Payload { get; }

        public ResponseFrame(byte[] payload)
        {
            this.Payload = payload;
        }

        public bool IsHeartbeat()
        {
            return this.Payload.SequenceEqual(HEARTBEAT_BYTES);
        }
    }
}
