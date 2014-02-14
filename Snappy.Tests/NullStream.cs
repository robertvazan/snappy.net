using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
#if SNAPPY_ASYNC
using System.Threading.Tasks;
#endif

namespace Snappy.Tests
{
    class NullStream : Stream
    {
#if SNAPPY_ASYNC
        TaskCompletionSource<object> Completed = new TaskCompletionSource<object>();
#endif
        public long Written { get; private set; }

        public override bool CanRead { get { return false; } }
        public override bool CanWrite { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override long Length { get { throw new NotSupportedException(); } }
        public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }

#if SNAPPY_ASYNC
        public NullStream()
        {
            Completed.SetResult(null);
        }
#endif

        public override int Read(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) { Written += count; }
#if SNAPPY_ASYNC
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellation) { Written += count; return Completed.Task; }
#endif
        public override void Flush() { }
#if SNAPPY_ASYNC
        public override Task FlushAsync(CancellationToken cancellation) { return Completed.Task; }
#endif

        public override void SetLength(long value) { throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
    }
}
