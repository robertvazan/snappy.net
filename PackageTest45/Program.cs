// Part of Snappy for Windows: https://snappy.machinezoo.com/
﻿﻿using Snappy;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageTest45
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = Encoding.ASCII.GetBytes("Hello World!");

            Console.WriteLine("SnappyCodec roundtrip: {0}", Encoding.ASCII.GetString(SnappyCodec.Uncompress(SnappyCodec.Compress(data))));

            var buffer = new MemoryStream();
            var stream = new SnappyStream(buffer, CompressionMode.Compress);
            stream.WriteAsync(data, 0, data.Length).Wait();
            stream.Close();
            buffer = new MemoryStream(buffer.ToArray());
            stream = new SnappyStream(buffer, CompressionMode.Decompress);
            var roundtrip = new byte[data.Length];
            int read = stream.ReadAsync(roundtrip, 0, data.Length).Result;
            if (read != data.Length)
                throw new ApplicationException();
            if (0 != stream.ReadAsync(roundtrip, 0, data.Length).Result)
                throw new ApplicationException();
            Console.WriteLine("SnappyStream async roundtrip: {0}", Encoding.ASCII.GetString(roundtrip));
        }
    }
}
