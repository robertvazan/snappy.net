using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
#if SNAPPY_ASYNC
using System.Threading.Tasks;
#endif

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
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < 50; ++i)
            {
                int count = Random.Next(1, 6);
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
            stopwatch.Stop();
            Console.WriteLine("Ran {0} MB through the stream, that's {1:0.0} MB/s", totalData / 1024 / 1024, totalData / stopwatch.Elapsed.Seconds / 1024 / 1024);
        }
    }
}
