using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy
{
    public class SnappyStream : Stream
    {
        const int MaxFrameSize = 1 << 16;
        Stream Stream;
        readonly CompressionMode Mode;
        readonly bool LeaveOpen;
        readonly StreamChunker Chunker;
        readonly StreamDechunker Dechunker;

        public override bool CanRead { get { return Stream != null && Mode == CompressionMode.Decompress && Stream.CanRead; } }
        public override bool CanWrite { get { return Stream != null && Mode == CompressionMode.Compress && Stream.CanWrite; } }
        public override bool CanSeek { get { return false; } }
        public override long Length { get { throw new NotSupportedException(); } }
        public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }

        public SnappyStream(Stream stream, CompressionMode mode) : this(stream, mode, false) { }

        public SnappyStream(Stream stream, CompressionMode mode, bool leaveOpen)
        {
            Stream = stream;
            Mode = mode;
            LeaveOpen = leaveOpen;
            if (mode == CompressionMode.Compress)
                Chunker = new StreamChunker(stream);
            else
                Dechunker = new StreamDechunker(stream);
        }

        public override void Close()
        {
            if (!LeaveOpen)
                Stream.Close();
            base.Close();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureCompressionMode();
            ValidateRange(buffer, offset, count);
            var slice = new byte[count];
            Array.Copy(buffer, offset, slice, 0, count);
            Chunker.Write(slice);
        }

        public override void Flush()
        {
            EnsureCompressionMode();
            Chunker.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureDecompressionMode();
            ValidateRange(buffer, offset, count);
            var slice = Dechunker.Read(count);
            Array.Copy(slice, 0, buffer, offset, slice.Length);
            return slice.Length;
        }

        public override void SetLength(long value) { throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }

        void WriteChunk(byte type, byte[] buffer, int offset, int count, bool crc)
        {
            Stream.WriteByte(type);
            Stream.WriteByte((byte)count);
            Stream.WriteByte((byte)(count >> 8));
            Stream.WriteByte((byte)(count >> 16));
            if (crc)
            {
                Stream.WriteByte(0);
                Stream.WriteByte(0);
                Stream.WriteByte(0);
                Stream.WriteByte(0);
            }
            Stream.Write(buffer, offset, count);
        }

        void EnsureCompressionMode()
        {
            if (Stream == null)
                throw new ObjectDisposedException("SnappyStream");
            if (Mode != CompressionMode.Compress)
                throw new InvalidOperationException("Use read operations on decompression stream");
        }

        void EnsureDecompressionMode()
        {
            if (Stream == null)
                throw new ObjectDisposedException("SnappyStream");
            if (Mode != CompressionMode.Decompress)
                throw new InvalidOperationException("Use write operations on compression stream");
        }

        void ValidateRange(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException();
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();
        }
    }
}
