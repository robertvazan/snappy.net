using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
#if SNAPPY_ASYNC
using System.Threading.Tasks;
#endif

namespace Snappy.Tests
{
    [TestFixture]
    public class SnappyStreamTest
    {
        Random Random = new Random();
        Random ReadRandom = new Random(1);
        Random WriteRandom = new Random(2);

        [Test]
        public void Twister()
        {
            var testdata = Directory.GetFiles(Benchmark.DataPath).Select(f => File.ReadAllBytes(f)).ToArray();
            long totalData = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.Elapsed < TimeSpan.FromSeconds(3))
            {
                int count = Random.Next(1, 21);
                var sequence = Enumerable.Range(0, count).Select(n => testdata[Random.Next(testdata.Length)]).ToArray();
                totalData += sequence.Sum(f => f.Length);
                var stream = new TestStream();
                ManualResetEvent doneReading = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(ctx =>
                {
                    try
                    {
                        using (var decompressor = new SnappyStream(stream, CompressionMode.Decompress))
                        {
                            foreach (var file in sequence)
                            {
                                var decompressed = new byte[file.Length];
                                if (decompressed.Length < 500)
                                {
                                    for (int i = 0; i < decompressed.Length; ++i)
                                        decompressed[i] = checked((byte)decompressor.ReadByte());
                                }
                                else
                                {
#if SNAPPY_ASYNC
                                    if (ReadRandom.Next(2) == 0)
                                        ReadAllAsync(decompressor, decompressed, 0, decompressed.Length).Wait();
                                    else
#endif
                                        ReadAll(decompressor, decompressed, 0, decompressed.Length);
                                }
                                CheckBuffers(file, decompressed);
                            }
                            Assert.AreEqual(-1, decompressor.ReadByte());
                        }
                        doneReading.Set();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Read thread failed: {0}", e);
                        throw;
                    }
                });
                using (var compressor = new SnappyStream(stream, CompressionMode.Compress))
                {
                    foreach (var file in sequence)
                    {
                        if (file.Length < 500)
                        {
                            for (int i = 0; i < file.Length; ++i)
                                compressor.WriteByte(file[i]);
                        }
                        else
                        {
#if SNAPPY_ASYNC
                            if (WriteRandom.Next(2) == 0)
                                compressor.WriteAsync(file, 0, file.Length).Wait();
                            else
#endif
                                compressor.Write(file, 0, file.Length);
                        }
                        if (WriteRandom.Next(10) == 0)
                            compressor.Flush();
                    }
                    compressor.Flush();
                }
                doneReading.WaitOne();
            }
            stopwatch.Stop();
            Console.WriteLine("Ran {0} MB through the stream, that's {1:0.0} MB/s", totalData / 1024 / 1024, totalData / stopwatch.Elapsed.TotalSeconds / 1024 / 1024);
        }

        void ReadAll(Stream stream, byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int read = stream.Read(buffer, offset, count);
                if (read <= 0)
                    throw new EndOfStreamException();
                offset += read;
                count -= read;
            }
        }

#if SNAPPY_ASYNC
        async Task ReadAllAsync(Stream stream, byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int read = await stream.ReadAsync(buffer, offset, count);
                if (read <= 0)
                    throw new EndOfStreamException();
                offset += read;
                count -= read;
            }
        }
#endif

        void CheckBuffers(byte[] expected, byte[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i)
                if (expected[i] != actual[i])
                    Assert.Fail();
        }
    }
}
