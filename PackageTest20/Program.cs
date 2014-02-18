using Snappy;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PackageTest20
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = Encoding.ASCII.GetBytes("Hello World!");
            
            Console.WriteLine("SnappyCodec roundtrip: {0}", Encoding.ASCII.GetString(SnappyCodec.Uncompress(SnappyCodec.Compress(data))));

            var buffer = new MemoryStream();
            var stream = new SnappyStream(buffer, CompressionMode.Compress);
            stream.Write(data, 0, data.Length);
            stream.Close();
            buffer = new MemoryStream(buffer.ToArray());
            stream = new SnappyStream(buffer, CompressionMode.Decompress);
            var roundtrip = new byte[data.Length];
            int read = stream.Read(roundtrip, 0, data.Length);
            if (read != data.Length)
                throw new ApplicationException();
            if (0 != stream.Read(roundtrip, 0, data.Length))
                throw new ApplicationException();
            Console.WriteLine("SnappyStream roundtrip: {0}", Encoding.ASCII.GetString(roundtrip));
        }
    }
}
