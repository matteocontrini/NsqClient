using System;

namespace NsqClient.Frames
{
    internal abstract class Frame
    {
        public static Frame Create(int frameType, byte[] payload)
        {
            switch (frameType)
            {
                case 0:
                    return new ResponseFrame(payload);
                case 1:
                    return new ErrorFrame(payload);
                case 2:
                    return new MessageFrame(payload);
                default:
                    throw new Exception("Invalid frame type received: " + frameType);
            }
        }
    }
}
