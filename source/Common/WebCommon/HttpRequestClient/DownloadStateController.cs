using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebCommon.HttpRequestClient
{
    /// <summary>
    /// Controls the state of a download operation, allowing pausing, resuming, and cancelling.
    /// </summary>
    public class DownloadStateController : IDisposable
    {
        private readonly object _lock = new object();
        private bool _isPaused = false;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private TaskCompletionSource<bool> _pauseTaskCompletionSource;

        /// <summary>
        /// Gets the CancellationToken associated with the download operation.
        /// </summary>
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        /// <summary>
        /// Pauses the download operation.
        /// </summary>
        public void Pause()
        {
            lock (_lock)
            {
                _isPaused = true;
                _pauseTaskCompletionSource = new TaskCompletionSource<bool>();
            }
        }

        public async Task PauseAsync()
        {
            lock (_lock)
            {
                _isPaused = true;
            }

            _pauseTaskCompletionSource = new TaskCompletionSource<bool>();
            await _pauseTaskCompletionSource.Task;
        }

        /// <summary>
        /// Resumes the download operation.
        /// </summary>
        public void Resume()
        {
            lock (_lock)
            {
                _isPaused = false;
            }

            _pauseTaskCompletionSource?.TrySetResult(true);
        }

        /// <summary>
        /// Checks if the download operation is paused.
        /// </summary>
        /// <returns>True if the download is paused, otherwise false.</returns>
        public bool IsPaused()
        {
            lock (_lock)
            {
                return _isPaused;
            }
        }

        /// <summary>
        /// Cancels the download operation.
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Resets the download state controller, clearing the pause state and creating a new CancellationTokenSource.
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _isPaused = false;
                _pauseTaskCompletionSource?.TrySetResult(true);
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }
        }

        /// <summary>
        /// Disposes of the resources used by the download state controller.
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }
    }

}