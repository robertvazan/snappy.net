// Part of Snappy for Windows: https://snappy.machinezoo.com/
﻿﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
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
    public class StreamThroughputTest
    {
        [Test, TestCaseSource(typeof(Benchmark), "GetTestFiles")]
        public void Compression(string name)
        {
            Benchmark.Run("Compressing", name, benchmark =>
            {
                var stream = new NullStream();
                var compressor = new SnappyStream(stream, CompressionMode.Compress);
                benchmark.Stopwatch.Start();
                for (int i = 0; i < benchmark.Iterations; ++i)
                {
                    compressor.Write(benchmark.Input, 0, benchmark.Input.Length);
                    compressor.Flush();
                }
                benchmark.Stopwatch.Stop();
                benchmark.Note = String.Format(" ({0:0.00 %})", stream.Written / (double)benchmark.Input.Length / benchmark.Iterations);
            });
        }

#if SNAPPY_ASYNC
        [Test, TestCaseSource(typeof(Benchmark), "GetTestFiles")]
        public void AsyncCompression(string name)
        {
            Benchmark.Run("Async-compressing", name, benchmark =>
            {
                var stream = new NullStream();
                var compressor = new SnappyStream(stream, CompressionMode.Compress);
                benchmark.Stopwatch.Start();
                for (int i = 0; i < benchmark.Iterations; ++i)
                {
                    compressor.WriteAsync(benchmark.Input, 0, benchmark.Input.Length).Wait();
                    compressor.FlushAsync().Wait();
                }
                benchmark.Stopwatch.Stop();
                benchmark.Note = String.Format(" ({0:0.00 %})", stream.Written / (double)benchmark.Input.Length / benchmark.Iterations);
            });
        }
#endif

        [Test, TestCaseSource(typeof(Benchmark), "GetTestFiles")]
        public void Decompression(string name)
        {
            Benchmark.Run("Decompressing", name, benchmark =>
            {
                var stream = new RepeaterStream(GetCompressedFile(benchmark.Input));
                var decompressor = new SnappyStream(stream, CompressionMode.Decompress);
                var decompressed = new byte[benchmark.Input.Length];
                benchmark.Stopwatch.Start();
                for (int i = 0; i < benchmark.Iterations; ++i)
                    ReadFully(decompressor, decompressed, 0, decompressed.Length);
                benchmark.Stopwatch.Stop();
            });
        }

#if SNAPPY_ASYNC
        [Test, TestCaseSource(typeof(Benchmark), "GetTestFiles")]
        public void AsyncDecompression(string name)
        {
            Benchmark.Run("Async-decompressing", name, benchmark =>
            {
                var stream = new RepeaterStream(GetCompressedFile(benchmark.Input));
                var decompressor = new SnappyStream(stream, CompressionMode.Decompress);
                var decompressed = new byte[benchmark.Input.Length];
                benchmark.Stopwatch.Start();
                for (int i = 0; i < benchmark.Iterations; ++i)
                    ReadFullyAsync(decompressor, decompressed, 0, decompressed.Length).Wait();
                benchmark.Stopwatch.Stop();
            });
        }
#endif

        void ReadFully(Stream stream, byte[] buffer, int offset, int count)
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
        async Task ReadFullyAsync(Stream stream, byte[] buffer, int offset, int count)
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

        byte[] GetCompressedFile(byte[] uncompressed)
        {
            var compressed = new MemoryStream();
            using (var compressor = new SnappyStream(compressed, CompressionMode.Compress, true))
                compressor.Write(uncompressed, 0, uncompressed.Length);
            compressed.Close();
            return compressed.ToArray();
        }
    }
}
