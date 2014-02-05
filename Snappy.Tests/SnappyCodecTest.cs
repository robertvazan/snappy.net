using NUnit.Framework;
using System;
using System.Collections.Generic;
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
            var length = SnappyCodec.Compress(input, 3, 5, output, 2);
            Assert.AreEqual("Hello", Encoding.ASCII.GetString(SnappyCodec.Uncompress(output.Skip(2).Take(length).ToArray())));
        }

        [Test]
        public void CompressSimple()
        {
            Assert.AreEqual("Hello", Encoding.ASCII.GetString(SnappyCodec.Uncompress(SnappyCodec.Compress(Encoding.ASCII.GetBytes("Hello")))));
        }

        [Test]
        public void CompressEmpty()
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
    }
}
