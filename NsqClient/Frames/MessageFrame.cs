namespace NsqClient.Frames
{
    class MessageFrame : Frame
    {
        public byte[] Payload { get; }

        public MessageFrame(byte[] payload)
        {
            this.Payload = payload;
        }
    }
}
