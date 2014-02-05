using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy
{
    class StreamDechunker
    {
        readonly FrameDecompressor Decompressor;
        byte[] Frame = new byte[0];
        int Consumed;

        public StreamDechunker(Stream stream)
        {
            Decompressor = new FrameDecompressor(stream);
        }

        public byte[] Read(int count)
        {
            if (Frame == null)
                return new byte[0];
            var result = new byte[count];
            int offset = 0;
            while (count - offset > Frame.Length - Consumed)
            {
                Array.Copy(Frame, Consumed, result, offset, Frame.Length - Consumed);
                offset += Frame.Length - Consumed;
                Consumed = 0;
                Frame = Decompressor.Read();
                if (Frame == null)
                {
                    var remainder = new byte[offset];
                    Array.Copy(result, 0, remainder, 0, offset);
                    return remainder;
                }
            }
            Array.Copy(Frame, Consumed, result, offset, count - offset);
            Consumed += count - offset;
            return result;
        }
    }
}
