using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PluginsCommon
{
    internal class TaskExecutor: IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly object _lockObject = new object();
        private bool _isDisposed = false;

        internal TaskExecutor(int maxDegreeOfParallelism)
        {
            _semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
        }

        public async Task ExecuteAsync(IEnumerable<Func<Task>> tasks)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(TaskExecutor));
            }

            var taskList = new List<Task>();
            foreach (var task in tasks)
            {
                var taskWithSemaphore = ExecuteTaskAsync(task);
                taskList.Add(taskWithSemaphore);
            }

            await Task.WhenAll(taskList);
        }

        private async Task ExecuteTaskAsync(Func<Task> task)
        {
            await _semaphore.WaitAsync();

            try
            {
                await task();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static readonly Playnite.SDK.ILogger _logger = Playnite.SDK.LogManager.GetLogger();

        public void Dispose()
        {
            lock (_lockObject)
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