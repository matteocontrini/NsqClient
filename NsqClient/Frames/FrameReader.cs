using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NsqClient.Frames
{
    internal class FrameReader
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
            int frameLength = await ReadInt32(FRAME_SIZE_LENGTH);
            int frameType = await ReadInt32(FRAME_TYPE_LENGTH);
            
            byte[] payload = new byte[frameLength - FRAME_TYPE_LENGTH];
            await ReadBytesAsync(payload, 0, payload.Length).ConfigureAwait(false);

            return Frame.Create(frameType, payload);
        }

        private async Task ReadBytesAsync(byte[] buffer, int offset, int count)
        {
            int bytesLeft = count;
            int bytesRead;

            while (bytesLeft > 0 &&
                (bytesRead = await this.stream.ReadAsync(buffer, offset, bytesLeft).ConfigureAwait(false)) > 0)
            {
                offset += bytesRead;
                bytesLeft -= bytesRead;
            }

            if (bytesLeft > 0)
            {
                throw new EndOfStreamException();
            }
        }

        private async Task<int> ReadInt32(int length)
        {
            byte[] bytes = new byte[length];
            await ReadBytesAsync(bytes, 0, bytes.Length).ConfigureAwait(false);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
