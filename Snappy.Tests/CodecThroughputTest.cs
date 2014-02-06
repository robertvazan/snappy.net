using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Snappy.Tests
{
    [TestFixture]
    public class CodecThroughputTest
    {
        [Test, TestCaseSource(typeof(Benchmark), "GetTestFiles")]
        public void Compression(string name)
        {
            Benchmark.Run("Compressing", name, benchmark =>
            {
                var output = new byte[SnappyCodec.GetMaxCompressedLength(benchmark.Input.Length)];
                int length = 0;
                benchmark.Stopwatch.Start();
                for (int i = 0; i < benchmark.Iterations; ++i)
                    length = SnappyCodec.Compress(benchmark.Input, 0, benchmark.Input.Length, output, 0);
                benchmark.Stopwatch.Stop();
                var roundtrip = new byte[benchmark.Input.Length];
                var roundtripLength = SnappyCodec.Uncompress(output, 0, length, roundtrip, 0);
                CollectionAssert.AreEqual(benchmark.Input, roundtrip.Take(roundtripLength));
                benchmark.Note = String.Format(" ({0:0.00 %})", length / (double)benchmark.Input.Length);
            });
        }

        [Test, TestCaseSource(typeof(Benchmark), "GetTestFiles")]
        public void Uncompression(string name)
        {
            Benchmark.Run("Uncompressing", name, benchmark =>
            {
                var compressed = SnappyCodec.Compress(benchmark.Input);
                var roundtrip = new byte[benchmark.Input.Length];
                int length = 0;
                benchmark.Stopwatch.Start();
                for (int i = 0; i < benchmark.Iterations; ++i)
                    length = SnappyCodec.Uncompress(compressed, 0, compressed.Length, roundtrip, 0);
                benchmark.Stopwatch.Stop();
                CollectionAssert.AreEqual(benchmark.Input, roundtrip);
            });
        }

        [Test, TestCaseSource(typeof(Benchmark), "GetTestFiles")]
        public void Validation(string name)
        {
            Benchmark.Run("Validating", name, benchmark =>
            {
                var compressed = SnappyCodec.Compress(benchmark.Input);
                bool ok = false;
                benchmark.Stopwatch.Start();
                for (int i = 0; i < benchmark.Iterations; ++i)
                    ok = SnappyCodec.Validate(compressed);
                benchmark.Stopwatch.Stop();
                Assert.IsTrue(ok);
            });
        }
    }
}
