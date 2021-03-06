using System.Text;

namespace NsqClient.Frames
{
    internal class ErrorFrame : Frame
    {
        public string Message { get; }

        public ErrorFrame(byte[] payload)
        {
            this.Message = Encoding.ASCII.GetString(payload);
        }
    }
}
