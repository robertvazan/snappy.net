using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy
{
    class FrameDeserializer
    {
        readonly Stream Stream;
        bool HasIdentificationFrame;
        bool IsEndOfStream;
        bool IsBadState;
        static readonly byte[] Identification = Encoding.ASCII.GetBytes("sNaPpY");

        public FrameDeserializer(Stream stream)
        {
            Stream = stream;
        }

        public byte[] Read(out byte type, out uint checksum)
        {
            try
            {
                type = 0xff;
                checksum = 0;
                if (IsBadState)
                    throw new InvalidOperationException("Snappy stream is broken");
                if (IsEndOfStream)
                    return null;
                var data = ReadInternal(out type, out checksum);
                if (data == null)
                    IsEndOfStream = true;
                return data;
            }
            catch (Exception)
            {
                IsBadState = true;
                throw;
            }
        }

        byte[] ReadInternal(out byte type, out uint checksum)
        {
            type = 0xff;
            checksum = 0;
            int length;
            byte[] data;
            do
            {
                var typeOrEof = Stream.ReadByte();
                if (typeOrEof == -1)
                    return null;
                type = (byte)typeOrEof;
                length = checked((byte)Stream.ReadByte() + ((int)(byte)Stream.ReadByte() << 8) + ((int)(byte)Stream.ReadByte() << 16));
                if (type == 0 || type == 1)
                {
                    if (length < 4)
                        throw new InvalidDataException("Snappy frame too small to contain checksum");
                    checksum = checked((uint)(byte)Stream.ReadByte() + ((uint)(byte)Stream.ReadByte() << 8) + ((uint)(byte)Stream.ReadByte() << 16) + ((uint)(byte)Stream.ReadByte() << 24));
                    length -= 4;
                }
                data = new byte[length];
                Read(data, 0, length);
                if (type == 0xff)
                {
                    if (length != 6 || !data.SequenceEqual(Identification))
                        throw new InvalidDataException("Invalid Snappy stream identification frame");
                    HasIdentificationFrame = true;
                }
                else
                {
                    if (!HasIdentificationFrame)
                        throw new InvalidDataException("Missing Snappy stream identification");
                    if (type >= 0x02 && type <= 0x7f)
                        throw new InvalidDataException("Encountered unskippable Snappy frame");
                }
            } while (type >= 0x80 && type <= 0xff);
            return data;
        }

        void Read(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int read = Stream.Read(buffer, offset, count);
                if (read <= 0)
                    throw new InvalidDataException("Unexpected end of Snappy stream");
                offset += read;
                count -= read;
            }
        }
    }
}
