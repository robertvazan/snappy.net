using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
#if SNAPPY_ASYNC
using System.Threading.Tasks;
#endif

namespace Snappy
{
    public class SnappyStream : Stream
    {
        Stream Stream;
        readonly CompressionMode Mode;
        readonly bool LeaveOpen;
        SnappyFrame Frame = new SnappyFrame();
        byte[] Buffer = new byte[256];
        int BufferUsage;
        int BufferRead;
        bool InitializedStream;
        bool BadStream;

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
        }

        protected override void Dispose(bool disposing)
        {
            if (Mode == CompressionMode.Compress)
                Flush();
            if (!LeaveOpen)
                Stream.Close();
            Stream = null;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                EnsureDecompressionMode();
                ValidateRange(buffer, offset, count);
                InitializeStream();
                int total = 0;
                while (count > 0)
                {
                    if (!EnsureAvailable())
                        return total;
                    int append = Math.Min(count, BufferUsage - BufferRead);
                    Array.Copy(Buffer, BufferRead, buffer, offset, append);
                    total += append;
                    offset += append;
                    count -= append;
                    BufferRead += append;
                }
                return total;
            }
            catch
            {
                BadStream = true;
                throw;
            }
        }

#if SNAPPY_ASYNC
        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellation)
        {
            try
            {
                EnsureDecompressionMode();
                ValidateRange(buffer, offset, count);
                await InitializeStreamAsync(cancellation);
                int total = 0;
                while (count > 0)
                {
                    if (!await EnsureAvailableAsync(cancellation))
                        return total;
                    int append = Math.Min(count, BufferUsage - BufferRead);
                    Array.Copy(Buffer, BufferRead, buffer, offset, append);
                    total += append;
                    offset += append;
                    count -= append;
                    BufferRead += append;
                }
                return total;
            }
            catch
            {
                BadStream = true;
                throw;
            }
        }
#endif

        public override int ReadByte()
        {
            try
            {
                EnsureDecompressionMode();
                InitializeStream();
                if (!EnsureAvailable())
                    return -1;
                byte result = Buffer[BufferRead];
                ++BufferRead;
                return result;
            }
            catch
            {
                BadStream = true;
                throw;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                EnsureCompressionMode();
                ValidateRange(buffer, offset, count);
                InitializeStream();
                while (count > 0)
                {
                    int append = Math.Min(count, SnappyFrame.MaxFrameSize - BufferUsage);
                    EnsureBuffer(BufferUsage + append);
                    Array.Copy(buffer, offset, Buffer, BufferUsage, append);
                    offset += append;
                    count -= append;
                    BufferUsage += append;
                    if (BufferUsage == SnappyFrame.MaxFrameSize)
                        Flush();
                }
            }
            catch
            {
                BadStream = true;
                throw;
            }
        }

#if SNAPPY_ASYNC
        public async override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellation)
        {
            try
            {
                EnsureCompressionMode();
                ValidateRange(buffer, offset, count);
                await InitializeStreamAsync(cancellation);
                while (count > 0)
                {
                    int append = Math.Min(count, SnappyFrame.MaxFrameSize - BufferUsage);
                    EnsureBuffer(BufferUsage + append);
                    Array.Copy(buffer, offset, Buffer, BufferUsage, append);
                    offset += append;
                    count -= append;
                    BufferUsage += append;
                    if (BufferUsage == SnappyFrame.MaxFrameSize)
                        await FlushAsync(cancellation);
                }
            }
            catch
            {
                BadStream = true;
                throw;
            }
        }
#endif

        public override void WriteByte(byte value)
        {
            try
            {
                EnsureCompressionMode();
                InitializeStream();
                EnsureBuffer(BufferUsage + 1);
                Buffer[BufferUsage] = value;
                ++BufferUsage;
                if (BufferUsage == SnappyFrame.MaxFrameSize)
                    Flush();
            }
            catch
            {
                BadStream = true;
                throw;
            }
        }

        public override void Flush()
        {
            try
            {
                EnsureCompressionMode();
                InitializeStream();
                if (BufferUsage > 0)
                {
                    Frame.SetCompressed(Buffer, 0, BufferUsage);
                    BufferUsage = 0;
                    Frame.Write(Stream);
                }
            }
            catch
            {
                BadStream = true;
                throw;
            }
        }

#if SNAPPY_ASYNC
        public async override Task FlushAsync(CancellationToken cancellation)
        {
            try
            {
                EnsureCompressionMode();
                await InitializeStreamAsync(cancellation);
                if (BufferUsage > 0)
                {
                    Frame.SetCompressed(Buffer, 0, BufferUsage);
                    BufferUsage = 0;
                    await Frame.WriteAsync(Stream, cancellation);
                }
            }
            catch
            {
                BadStream = true;
                throw;
            }
        }
#endif

        public override void SetLength(long value) { throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }

        void InitializeStream()
        {
            if (!InitializedStream)
            {
                if (Mode == CompressionMode.Compress)
                {
                    Frame.SetStreamIdentifier();
                    Frame.Write(Stream);
                }
                else
                {
                    if (!Frame.Read(Stream))
                        throw new EndOfStreamException();
                    if (Frame.Type != SnappyFrameType.StreamIdentifier)
                        throw new InvalidDataException();
                }
                InitializedStream = true;
            }
        }

#if SNAPPY_ASYNC
        async Task InitializeStreamAsync(CancellationToken cancellation)
        {
            if (!InitializedStream)
            {
                if (Mode == CompressionMode.Compress)
                {
                    Frame.SetStreamIdentifier();
                    await Frame.WriteAsync(Stream, cancellation);
                }
                else
                {
                    if (!await Frame.ReadAsync(Stream, cancellation))
                        throw new EndOfStreamException();
                    if (Frame.Type != SnappyFrameType.StreamIdentifier)
                        throw new InvalidDataException();
                }
                InitializedStream = true;
            }
        }
#endif

        bool EnsureAvailable()
        {
            if (BufferRead >= BufferUsage)
            {
                do
                {
                    if (!Frame.Read(Stream))
                        return false;
                } while (Frame.Type != SnappyFrameType.Compressed && Frame.Type != SnappyFrameType.Uncompressed || Frame.DataLength == 0);
                EnsureBuffer(Frame.DataLength);
                BufferRead = 0;
                BufferUsage = Frame.DataLength;
                Frame.GetData(Buffer);
            }
            return true;
        }

#if SNAPPY_ASYNC
        async Task<bool> EnsureAvailableAsync(CancellationToken cancellation)
        {
            if (BufferRead >= BufferUsage)
            {
                do
                {
                    if (!await Frame.ReadAsync(Stream, cancellation))
                        return false;
                } while (Frame.Type != SnappyFrameType.Compressed && Frame.Type != SnappyFrameType.Uncompressed || Frame.DataLength == 0);
                EnsureBuffer(Frame.DataLength);
                BufferRead = 0;
                BufferUsage = Frame.DataLength;
                Frame.GetData(Buffer);
            }
            return true;
        }
#endif

        void EnsureCompressionMode()
        {
            CheckStream();
            if (Mode != CompressionMode.Compress)
                throw new InvalidOperationException("Use read operations on decompression stream");
        }

        void EnsureDecompressionMode()
        {
            CheckStream();
            if (Mode != CompressionMode.Decompress)
                throw new InvalidOperationException("Use write operations on compression stream");
        }

        void CheckStream()
        {
            if (Stream == null)
                throw new ObjectDisposedException("SnappyStream");
            if (BadStream)
                throw new InvalidOperationException();
        }

        void ValidateRange(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException();
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();
        }

        void EnsureBuffer(int size)
        {
            if (size > Buffer.Length)
            {
                var newSize = 2 * Buffer.Length;
                while (newSize < size)
                    newSize *= 2;
                var newBuffer = new byte[newSize];
                Array.Copy(Buffer, 0, newBuffer, 0, BufferUsage);
                Buffer = newBuffer;
            }
        }
    }
}
