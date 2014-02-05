using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy.Tests
{
    [TestFixture]
    public class SnappyBlockTest
    {
        [Test]
        public void CompressRange()
        {
            var input = Encoding.ASCII.GetBytes("ByeHelloBye");
            var output = new byte[100];
            var length = SnappyBlock.Compress(input, 3, 5, output, 2);
            Assert.AreEqual("Hello", Encoding.ASCII.GetString(SnappyBlock.Uncompress(output.Skip(2).Take(length).ToArray())));
        }

        [Test]
        public void CompressSimple()
        {
            Assert.AreEqual("Hello", Encoding.ASCII.GetString(SnappyBlock.Uncompress(SnappyBlock.Compress(Encoding.ASCII.GetBytes("Hello")))));
        }

        [Test]
        public void CompressEmpty()
        {
            var compressed = SnappyBlock.Compress(new byte[0]);
            Assert.That(compressed.Length, Is.GreaterThan(0));
            Assert.AreEqual(0, SnappyBlock.Uncompress(compressed).Length);
        }

        [Test]
        public void CompressExceptions()
        {
            var input = new byte[100];
            var output = new byte[100];
            Assert.Throws<ArgumentNullException>(() => SnappyBlock.Compress(null));
            Assert.Throws<ArgumentNullException>(() => SnappyBlock.Compress(null, 0, 3, output, 0));
            Assert.Throws<ArgumentNullException>(() => SnappyBlock.Compress(input, 0, 3, null, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyBlock.Compress(input, -1, 3, output, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyBlock.Compress(input, 0, -1, output, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyBlock.Compress(input, 90, 20, output, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyBlock.Compress(input, 0, 3, output, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyBlock.Compress(input, 0, 3, output, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyBlock.Compress(input, 0, 3, output, 101));
            Assert.Throws<ArgumentOutOfRangeException>(() => SnappyBlock.Compress(input, 0, 100, new byte[3], 0));
        }
    }
}
