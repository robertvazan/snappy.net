using System;
using System.Collections.Generic;
using System.Text;

namespace Snappy
{
    class Crc32C
    {
        const uint POLY = 0x82f63b78;
        static readonly uint[][] crc32c_table = new uint[8][];

        static Crc32C()
        {
            for (int i = 0; i < 8; ++i)
                crc32c_table[i] = new uint[256];
            for (uint n = 0; n < 256; n++)
            {
                uint crc = n;
                crc = (crc & 1) != 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc = (crc & 1) != 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc = (crc & 1) != 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc = (crc & 1) != 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc = (crc & 1) != 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc = (crc & 1) != 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc = (crc & 1) != 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc = (crc & 1) != 0 ? (crc >> 1) ^ POLY : crc >> 1;
                crc32c_table[0][n] = crc;
            }
            for (uint n = 0; n < 256; n++)
            {
                uint crc = crc32c_table[0][n];
                for (uint k = 1; k < 8; k++)
                {
                    crc = crc32c_table[0][crc & 0xff] ^ (crc >> 8);
                    crc32c_table[k][n] = crc;
                }
            }
        }

        public static uint Compute(byte[] data, int offset, int count)
        {
            uint crc = ~0u;
            for (int i = 0; i < count; ++i)
                crc = crc32c_table[0][(crc ^ data[offset + i]) & 0xff] ^ (crc >> 8);
            return ~crc;
        }

        public static uint ComputeMasked(byte[] data, int offset, int count)
        {
            var checksum = Compute(data, offset, count);
            return ((checksum >> 15) | (checksum << 17)) + 0xa282ead8;
        }
    }
}
