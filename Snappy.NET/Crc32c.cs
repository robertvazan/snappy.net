using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy
{
    class Crc32C
    {
        const uint POLY = 0x82f63b78;
        static readonly uint[][] crc32c_table = Enumerable.Range(0, 8).Select(i => new uint[256]).ToArray();

        static Crc32C()
        {
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

        public static uint Compute(byte[] data)
        {
            uint crc = ~0u;
            for (int i = 0; i < data.Length; ++i)
                crc = crc32c_table[0][(crc ^ data[i]) & 0xff] ^ (crc >> 8);
            return ~crc;
        }

        public static uint ComputeMasked(byte[] data)
        {
            var checksum = Compute(data);
            return ((checksum >> 15) | (checksum << 17)) + 0xa282ead8;
        }
    }
}
