using System.Text;

namespace NsqClient.Frames
{
    class ResponseFrame : Frame
    {
        public string Message { get; }

        public ResponseType Type { get; set; }

        public ResponseFrame(byte[] payload)
        {
            this.Message = Encoding.ASCII.GetString(payload);

            ParseResponse();
        }

        private void ParseResponse()
        {
            if (this.Message.StartsWith("{"))
            {
                this.Type = ResponseType.Identify;
            }
            else switch (this.Message)
            {
                case "OK":
                    this.Type = ResponseType.Ok;
                    break;
                case "_heartbeat_":
                    this.Type = ResponseType.Heartbeat;
                    break;
                default:
                    this.Type = ResponseType.Unknown;
                    break;
            }
        }
    }
}
