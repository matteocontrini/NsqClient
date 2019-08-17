using System.Text;

namespace NsqClient.Frames
{
    class ErrorFrame : Frame
    {
        public string Message { get; }
        
        public ErrorType Type { get; private set; }

        public ErrorFrame(byte[] payload)
        {
            this.Message = Encoding.ASCII.GetString(payload);
            
            ParseError();
        }

        private void ParseError()
        {
            switch (this.Message)
            {
                case "E_INVALID":
                    this.Type = ErrorType.Invalid;
                    break;
                case "E_BAD_BODY":
                    this.Type = ErrorType.BadBody;
                    break;
                case "E_BAD_TOPIC":
                    this.Type = ErrorType.BadTopic;
                    break;
                case "E_BAD_CHANNEL":
                    this.Type = ErrorType.BadChannel;
                    break;
                case "E_BAD_MESSAGE":
                    this.Type = ErrorType.BadMessage;
                    break;
                case "E_PUB_FAILED":
                    this.Type = ErrorType.PubFailed;
                    break;
                case "E_MPUB_FAILED":
                    this.Type = ErrorType.MPubFailed;
                    break;
                case "E_DPUB_FAILED":
                    this.Type = ErrorType.DPubFailed;
                    break;
                case "E_FIN_FAILED":
                    this.Type = ErrorType.FinFailed;
                    break;
                case "E_REQ_FAILED":
                    this.Type = ErrorType.ReqFailed;
                    break;
                case "E_TOUCH_FAILED":
                    this.Type = ErrorType.TouchFailed;
                    break;
                default:
                    this.Type = ErrorType.Unknown;
                    break;
            }
        }
    }
}
