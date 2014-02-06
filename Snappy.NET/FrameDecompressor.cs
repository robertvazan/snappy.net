using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy
{
    class FrameDecompressor
    {
        readonly FrameDeserializer Deserializer;

        public FrameDecompressor(Stream stream)
        {
            Deserializer = new FrameDeserializer(stream);
        }

        public byte[] Read()
        {
            byte type;
            uint checksum;
            var data = Deserializer.Read(out type, out checksum);
            if (data == null)
                return null;
            else
            {
                var uncompressed = type == 1 ? data : SnappyCodec.Uncompress(data);
                if (Crc32C.ComputeMasked(uncompressed) != checksum)
                    throw new InvalidDataException("Incorrect checksum");
                return uncompressed;
            }
        }
    }
}
