using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy
{
    class StreamChunker
    {
        const int MaxFrameSize = 1 << 16;
        readonly FrameCompressor Compressor;
        readonly byte[] Frame = new byte[MaxFrameSize];
        int Buffered;

        public StreamChunker(Stream stream)
        {
            Compressor = new FrameCompressor(stream);
        }

        public void Write(byte[] data)
        {
            int offset = 0;
            while (Buffered + (data.Length - offset) >= MaxFrameSize)
            {
                Array.Copy(data, offset, Frame, Buffered, MaxFrameSize - Buffered);
                Compressor.Write(Frame);
                offset += MaxFrameSize - Buffered;
                Buffered = 0;
            }
            Array.Copy(data, offset, Frame, Buffered, data.Length - offset);
            Buffered += data.Length;
        }

        public void Flush()
        {
            if (Buffered > 0)
            {
                var remainder = new byte[Buffered];
                Array.Copy(Frame, remainder, Buffered);
                Buffered = 0;
                Compressor.Write(remainder);
            }
        }
    }
}
