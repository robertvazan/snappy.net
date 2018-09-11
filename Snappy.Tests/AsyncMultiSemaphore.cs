// Part of Snappy for Windows: https://snappy.machinezoo.com/
﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
#if SNAPPY_ASYNC
using System.Threading.Tasks;
#endif

namespace Snappy.Tests
{
    class AsyncMultiSemaphore
    {
        int CurrentCount;
#if SNAPPY_ASYNC
        TaskCompletionSource<object> Available = new TaskCompletionSource<object>();
#else
        ManualResetEvent Available = new ManualResetEvent(false);
#endif

        public void Add(int count)
        {
            lock (this)
            {
                if (count < 0)
                    throw new ArgumentOutOfRangeException();
                if (CurrentCount > 0)
                    CurrentCount += count;
                else
                {
                    CurrentCount = count;
#if SNAPPY_ASYNC
                    Available.SetResult(null);
#else
                    Available.Set();
#endif
                }
            }
        }

#if SNAPPY_ASYNC
        public async Task<int> TakeAsync(int max)
        {
            if (max <= 0)
                throw new ArgumentOutOfRangeException();
            while (true)
            {
                Task available;
                lock (this)
                {
                    if (CurrentCount > 0)
                    {
                        var taken = Math.Min(CurrentCount, max);
                        CurrentCount -= taken;
                        if (CurrentCount == 0)
                            Available = new TaskCompletionSource<object>();
                        return taken;
                    }
                    available = Available.Task;
                }
                await available;
            }
        }

        public int Take(int max) { return TakeAsync(max).Result; }
#else
        public int Take(int max)
        {
            if (max <= 0)
                throw new ArgumentOutOfRangeException();
            while (true)
            {
                lock (this)
                {
                    if (CurrentCount > 0)
                    {
                        var taken = Math.Min(CurrentCount, max);
                        CurrentCount -= taken;
                        if (CurrentCount == 0)
                            Available.Reset();
                        return taken;
                    }
                }
                Available.WaitOne();
            }
        }
#endif
    }
}
