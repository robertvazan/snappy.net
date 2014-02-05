using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy
{
    class FrameSerializer
    {
        readonly Stream Stream;
        static readonly byte[] Identification = Encoding.ASCII.GetBytes("sNaPpY");

        public FrameSerializer(Stream stream)
        {
            Stream = stream;
            Write(0xff, null, Identification);
        }

        public void Write(byte type, uint? checksum, byte[] data)
        {
            Stream.WriteByte(type);
            int length = data.Length;
            if (checksum != null)
                length += 4;
            Stream.WriteByte((byte)length);
            Stream.WriteByte((byte)(length >> 8));
            Stream.WriteByte((byte)(length >> 16));
            if (checksum != null)
            {
                Stream.WriteByte((byte)checksum.Value);
                Stream.WriteByte((byte)(checksum.Value >> 8));
                Stream.WriteByte((byte)(checksum.Value >> 16));
                Stream.WriteByte((byte)(checksum.Value >> 24));
            }
            Stream.Write(data, 0, data.Length);
        }
    }
}
