using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.Tests
{
    [TestFixture]
    public class SnappyStreamTest
    {
        Random Random = new Random(0);

        [Test]
        public void Twister()
        {
            var testdata = Directory.GetFiles(Benchmark.DataPath).Select(f => File.ReadAllBytes(f)).ToArray();
            long totalData = 0;
            for (int i = 0; i < 10; ++i)
            {
                int count = Random.Next(1, 21);
                var sequence = Enumerable.Range(0, count).Select(n => testdata[Random.Next(testdata.Length)]).ToArray();
                totalData += sequence.Sum(f => f.Length);
                var stream = new MemoryStream();
                using (var compressor = new SnappyStream(stream, CompressionMode.Compress))
                {
                    foreach (var file in sequence)
                        compressor.Write(file, 0, file.Length);
                    compressor.Flush();
                }
                stream = new MemoryStream(stream.ToArray());
                using (var decompressor = new SnappyStream(stream, CompressionMode.Decompress))
                {
                    foreach (var file in sequence)
                    {
                        var decompressed = new byte[file.Length];
                        int decompressedLength = decompressor.Read(decompressed, 0, decompressed.Length);
                        Assert.AreEqual(file.Length, decompressedLength);
                        CollectionAssert.AreEqual(file, decompressed);
                    }
                    Assert.AreEqual(-1, decompressor.ReadByte());
                }
            }
            Console.WriteLine("Ran {0} MB through the stream", totalData / 1024 / 1024);
        }
    }
}
