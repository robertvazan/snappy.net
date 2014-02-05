using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy
{
    class FrameCompressor
    {
        readonly FrameSerializer Serializer;

        public FrameCompressor(Stream stream)
        {
            Serializer = new FrameSerializer(stream);
        }

        public void Write(byte[] data)
        {
            Serializer.Write(0, Crc32C.Compute(data), SnappyCodec.Compress(data));
        }
    }
}
