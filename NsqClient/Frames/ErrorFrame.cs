namespace NsqClient.Frames
{
    class ErrorFrame : Frame
    {
        public byte[] Payload { get; }

        public ErrorFrame(byte[] payload)
        {
            this.Payload = payload;
        }
    }
}
