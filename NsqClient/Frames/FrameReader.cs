using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NsqClient.Frames
{
    class FrameReader
    {
        private const int FRAME_SIZE_LENGTH = 4;
        private const int FRAME_TYPE_LENGTH = 4;
        
        private readonly NetworkStream stream;

        public FrameReader(NetworkStream stream)
        {
            this.stream = stream;
        }
        
        public async Task<Frame> ReadNext()
        {
            int frameLength = await ReadAsInt32(FRAME_SIZE_LENGTH);
            int frameType = await ReadAsInt32(FRAME_TYPE_LENGTH);
            
            byte[] payload = new byte[frameLength - FRAME_TYPE_LENGTH];
            this.stream.Read(payload, 0, payload.Length);

            return Frame.Create(frameType, payload);
        }

        private async Task<int> ReadAsInt32(int length)
        {
            byte[] readBytes = new byte[length];
            await this.stream.ReadAsync(readBytes, 0, length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(readBytes);
            }

            return BitConverter.ToInt32(readBytes, 0);
        }
    }
}
