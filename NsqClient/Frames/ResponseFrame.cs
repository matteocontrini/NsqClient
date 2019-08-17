namespace NsqClient.Frames
{
    class ResponseFrame : Frame
    {
        public byte[] Payload { get; }

        public ResponseFrame(byte[] payload)
        {
            this.Payload = payload;
        }
    }
}
