using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.Tests
{
    [TestFixture]
    public class SnappyCodecTest
    {
        [Test]
        public void CompressRange()
        {
            var input = Encoding.ASCII.GetBytes("ByeHelloBye");
            var output = new byte[100];
            var length = SnappyCodec.Compress(input, 3, 5, output, 10);
            Assert.AreEqual("Hello", Encoding.ASCII.GetString(SnappyCodec.Uncompress(output.Skip(10).Take(length).ToArray())));
        }

        [Test]
        public void CompressUncompressSimple()
        {
            Assert.AreEqual("Hello", Encoding.ASCII.GetString(SnappyCodec.Uncompress(SnappyCodec.Compress(Encoding.ASCII.GetBytes("Hello")))));
        }

        [Test]
        public void CompressUncompressEmpty()
        {
            var compressed = SnappyCodec.Compress(new byte[0]);
            Assert.That(compressed.Length, Is.GreaterThan(0));
            Assert.AreEqual(0, SnappyCodec.Uncompress(compressed).Length);
        }

        [Test]
        public void CompressExceptions()
        {
            var input = new byte[100];
            var output = new byte[100];
            Assert.Throws<ArgumentNullException>(() => SnappyCodec.Compress(null));
            Assert.Throws<ArgumentNullException>(() => SnappyCodec.Compress(null, 0, 3, output, 0));
            Assert.Throws<ArgumentNullException>(() => SnappyCodec.Compress(input, 0, 3, null, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Compress(input, -1, 3, output, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Compress(input, 0, -1, output, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Compress(input, 90, 20, output, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Compress(input, 0, 3, output, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Compress(input, 0, 3, output, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Compress(input, 0, 3, output, 101));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Compress(input, 0, 100, new byte[3], 0));
        }

        [Test]
        public void UncompressRange()
        {
            var howdy = Encoding.ASCII.GetBytes("Howdy");
            var padded = howdy.Take(3).Concat(SnappyCodec.Compress(Encoding.ASCII.GetBytes("Hello"))).Concat(howdy.Skip(3)).ToArray();
            var output = new byte[100];
            var length = SnappyCodec.Uncompress(padded, 3, padded.Length - 5, output, 10);
            Assert.AreEqual(5, length);
            Assert.AreEqual("Hello", Encoding.ASCII.GetString(output.Skip(10).Take(5).ToArray()));
        }

        [Test]
        public void UncompressExceptions()
        {
            var uncompressed = Encoding.ASCII.GetBytes("Hello, hello, howdy?");
            var compressed = SnappyCodec.Compress(uncompressed);
            var buffer = new byte[100];
            Assert.Throws<ArgumentNullException>(() => SnappyCodec.Uncompress(null));
            Assert.Throws<ArgumentNullException>(() => SnappyCodec.Uncompress(null, 0, 3, buffer, 0));
            Assert.Throws<ArgumentNullException>(() => SnappyCodec.Uncompress(compressed, 0, compressed.Length, null, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Uncompress(compressed, -1, uncompressed.Length, buffer, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Uncompress(compressed, 0, -1, buffer, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Uncompress(compressed, compressed.Length - 2, 4, buffer, 0));
            Assert.Throws<InvalidDataException>(() => SnappyCodec.Uncompress(compressed, 0, 0, buffer, 0));
            Assert.Throws<InvalidDataException>(() => SnappyCodec.Uncompress(compressed, compressed.Length, 0, buffer, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Uncompress(compressed, 0, compressed.Length, buffer, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Uncompress(compressed, 0, compressed.Length, buffer, 101));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Uncompress(compressed, 0, compressed.Length, buffer, 97));
            var rubbish = new byte[10];
            new Random(0).NextBytes(rubbish);
            Assert.Throws<InvalidDataException>(() => SnappyCodec.Uncompress(rubbish, 0, rubbish.Length, buffer, 0));
        }

        [Test]
        public void GetUncompressedLengthExceptions()
        {
            var uncompressed = Encoding.ASCII.GetBytes("Hello, hello, howdy?");
            var compressed = SnappyCodec.Compress(uncompressed);
            var buffer = new byte[100];
            Assert.Throws<ArgumentNullException>(() => SnappyCodec.GetUncompressedLength(null));
            Assert.Throws<ArgumentNullException>(() => SnappyCodec.GetUncompressedLength(null, 0, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.GetUncompressedLength(compressed, -1, uncompressed.Length));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.GetUncompressedLength(compressed, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.GetUncompressedLength(compressed, compressed.Length - 2, 4));
            Assert.Throws<InvalidDataException>(() => SnappyCodec.GetUncompressedLength(compressed, 0, 0));
            Assert.Throws<InvalidDataException>(() => SnappyCodec.GetUncompressedLength(compressed, compressed.Length, 0));
            var rubbish = Enumerable.Repeat((byte)0xff, 10).ToArray();
            Assert.Throws<InvalidDataException>(() => SnappyCodec.GetUncompressedLength(rubbish, 0, rubbish.Length));
        }

        [Test]
        public void Validate()
        {
            var uncompressed = Encoding.ASCII.GetBytes("Hello, hello, howdy?");
            var compressed = SnappyCodec.Compress(uncompressed);
            var buffer = new byte[100];
            Assert.IsTrue(SnappyCodec.Validate(compressed));
            var rubbish = new byte[10];
            new Random(0).NextBytes(rubbish);
            Assert.IsFalse(SnappyCodec.Validate(rubbish, 0, rubbish.Length));
            Assert.IsFalse(SnappyCodec.Validate(compressed, 0, 0));
            Assert.IsFalse(SnappyCodec.Validate(compressed, compressed.Length, 0));
        }

        [Test]
        public void ValidateExceptions()
        {
            var uncompressed = Encoding.ASCII.GetBytes("Hello, hello, howdy?");
            var compressed = SnappyCodec.Compress(uncompressed);
            var buffer = new byte[100];
            Assert.Throws<ArgumentNullException>(() => SnappyCodec.Validate(null));
            Assert.Throws<ArgumentNullException>(() => SnappyCodec.Validate(null, 0, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Validate(compressed, -1, uncompressed.Length));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Validate(compressed, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyCodec.Validate(compressed, compressed.Length - 2, 4));
        }
    }
}
