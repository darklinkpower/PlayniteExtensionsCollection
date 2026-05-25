using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKHandler.Infrastructure
{
    public sealed class NamedEventSignalWatcher : IDisposable
    {
        private readonly Dictionary<string, EventWaitHandle> _handles
            = new Dictionary<string, EventWaitHandle>();

        private readonly object _lock = new object();

        private CancellationTokenSource _cts;
        private Task _watchTask;

        private bool _started;
        private bool _disposed;

        public event EventHandler<string> SignalReceived;

        public void Register(string eventName)
        {
            if (eventName == null)
            {
                throw new ArgumentNullException(
                    nameof(eventName));
            }

            lock (_lock)
            {
                ThrowIfDisposed();

                if (_started)
                {
                    throw new InvalidOperationException(
                        "Cannot register after watcher has started.");
                }

                if (_handles.ContainsKey(eventName))
                {
                    return;
                }

                _handles[eventName] =
                    new EventWaitHandle(
                        false,
                        EventResetMode.AutoReset,
                        eventName);
            }
        }

        public void Start()
        {
            lock (_lock)
            {
                ThrowIfDisposed();

                if (_started)
                {
                    return;
                }

                _started = true;
                _cts = new CancellationTokenSource();
                _watchTask = Task.Run(() =>
                    WatchLoop(_cts.Token));
            }
        }

        private void WatchLoop(
            CancellationToken token)
        {
            try
            {
                KeyValuePair<string, EventWaitHandle>[] entries;

                lock (_lock)
                {
                    entries =
                        _handles.ToArray();
                }

                if (entries.Length == 0)
                {
                    return;
                }

                var handles = entries
                    .Select(x => x.Value)
                    .ToArray();

                while (!token.IsCancellationRequested)
                {
                    int index = WaitHandle.WaitAny(
                        handles,
                        1000);

                    if (index == WaitHandle.WaitTimeout)
                    {
                        continue;
                    }

                    if (index >= 0 && index < entries.Length)
                    {
                        SignalReceived?.Invoke(
                            this,
                            entries[index].Key);
                    }
                }
            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
            }

            try
            {
                _cts?.Cancel();

                try
                {
                    _watchTask?.Wait(
                        TimeSpan.FromSeconds(2));
                }
                catch
                {
                }

                foreach (var handle in _handles.Values)
                {
                    handle.Dispose();
                }

                _handles.Clear();

                _cts?.Dispose();
            }
            finally
            {
                SignalReceived = null;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    nameof(NamedEventSignalWatcher));
            }
        }
    }
}
