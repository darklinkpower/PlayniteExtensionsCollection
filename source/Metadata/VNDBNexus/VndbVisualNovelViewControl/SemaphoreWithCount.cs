using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VNDBNexus.VndbVisualNovelViewControlAggregate
{
    internal class SemaphoreWithCount
    {
        private readonly SemaphoreSlim _semaphore;
        private int _count = 0;
        private bool _isDisposed = false;
        private readonly object _lock = new object();
        public bool IsInUse => GetIsInUse();

        public SemaphoreWithCount(int initialCount, int maxCount)
        {
            _semaphore = new SemaphoreSlim(initialCount, maxCount);
        }

        private bool GetIsInUse()
        {
            lock (_lock)
            {
                return _count > 0;
            }
        }

        public void Increment()
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(SemaphoreWithCount));
                }
                    
                Interlocked.Increment(ref _count);
            }
        }

        public async Task WaitAsync()
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(SemaphoreWithCount));
                }

                Interlocked.Increment(ref _count);
            }

            await _semaphore.WaitAsync();
        }

        public void Release()
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(SemaphoreWithCount));
                }

                Interlocked.Decrement(ref _count);
            }

            _semaphore.Release();
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (!_isDisposed)
                {
                    _semaphore.Dispose();
                    _isDisposed = true;
                }
            }
        }
    }
}