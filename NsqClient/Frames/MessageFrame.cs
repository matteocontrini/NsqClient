using System;
using System.Text;

namespace NsqClient.Frames
{
    class MessageFrame : Frame
    {
        private const int TIMESTAMP_LENGTH = 8;
        private const int ATTEMPTS_LENGTH = 2;
        private const int MESSAGEID_LENGTH = 16;

        public long Timestamp { get; set; }

        public ushort Attempts { get; set; }

        public string MessageId { get; set; }

        public byte[] MessageBody { get; set; }

        public MessageFrame(byte[] payload)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(payload, 0, TIMESTAMP_LENGTH);
                Array.Reverse(payload, TIMESTAMP_LENGTH, ATTEMPTS_LENGTH);
            }

            int offset = 0;
            
            this.Timestamp = BitConverter.ToInt64(payload, offset);
            offset += TIMESTAMP_LENGTH;
            
            this.Attempts = BitConverter.ToUInt16(payload, offset);
            offset += ATTEMPTS_LENGTH;
            
            this.MessageId = Encoding.ASCII.GetString(payload, offset, MESSAGEID_LENGTH);
            offset += MESSAGEID_LENGTH;

            byte[] body = new byte[payload.Length - offset];
            Array.ConstrainedCopy(payload, offset, body, 0, body.Length);
            
            this.MessageBody = body;
        }
    }
}
