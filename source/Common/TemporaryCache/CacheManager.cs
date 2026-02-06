using Playnite.SDK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;
using TemporaryCache.Models;

namespace TemporaryCache
{
    public sealed class CacheManager<TKey, TValue> : IDisposable
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly ConcurrentDictionary<TKey, CacheItem<TValue>> _cache = new ConcurrentDictionary<TKey, CacheItem<TValue>>();
        private readonly Timer _cleanupTimer;
        private readonly List<Func<CacheItem<TValue>, DateTime, bool>> _ruleBuilder = new List<Func<CacheItem<TValue>, DateTime, bool>>();
        private Func<CacheItem<TValue>, DateTime, bool>[] _compiledRules;
        private volatile bool _isFrozen;
        private int _cleanupInProgress; // 0 = stopped, 1 = running
        private int _cleanupTimerRunning; // 0 = stopped, 1 = running
        private int _disposed;

        public CacheManager()
        {
            _cleanupTimer = new Timer(
                CleanupExpiredItems,
                null,
                Timeout.InfiniteTimeSpan,
                TimeSpan.FromSeconds(8)
            );
        }


        public CacheManager<TKey, TValue> WithItemLifetime(TimeSpan lifetime)
        {
            ThrowIfDisposed();
            EnsureNotFrozen();
            _ruleBuilder.Add(
                (item, now) => now - item.CreatedAtUtc >= lifetime
            );

            return this;
        }

        public CacheManager<TKey, TValue> WithSlidingExpiration(TimeSpan idleTime)
        {
            ThrowIfDisposed();
            EnsureNotFrozen();
            _ruleBuilder.Add(
                (item, now) => now - item.LastAccessedUtc >= idleTime
            );

            return this;
        }

        public CacheManager<TKey, TValue> WithValueInvalidation(
            Func<TValue, bool> isInvalid)
        {
            ThrowIfDisposed();
            EnsureNotFrozen();
            _ruleBuilder.Add(
                (item, _) => isInvalid(item.Value)
            );

            return this;
        }

        public CacheManager<TKey, TValue> WithMaxItemCount(int maxItems)
        {
            ThrowIfDisposed();
            EnsureNotFrozen();
            _ruleBuilder.Add(
                (_, __) => _cache.Count > maxItems
            );

            return this;
        }

        public CacheManager<TKey, TValue> WithExpirationAt(DateTime utcDeadline)
        {
            ThrowIfDisposed();
            EnsureNotFrozen();
            _ruleBuilder.Add(
                (_, now) => now >= utcDeadline
            );

            return this;
        }

        public CacheManager<TKey, TValue> WithExternalInvalidation(
            Func<bool> shouldInvalidate)
        {
            ThrowIfDisposed();
            EnsureNotFrozen();
            _ruleBuilder.Add(
                (_, __) => shouldInvalidate()
            );

            return this;
        }

        public CacheManager<TKey, TValue> WithCustomStalenessRule(
            Func<CacheItem<TValue>, DateTime, bool> rule)
        {
            ThrowIfDisposed();
            EnsureNotFrozen();
            _ruleBuilder.Add(rule);

            return this;
        }

        private void EnsureNotFrozen()
        {
            if (_isFrozen)
            {
                throw new InvalidOperationException(
                    "CacheManager configuration cannot be modified after first use.");
            }
        }

        private void FreezeIfNeeded()
        {
            if (_isFrozen)
            {
                return;
            }

            lock (_ruleBuilder)
            {
                if (_isFrozen)
                {
                    return;
                }

                _compiledRules = _ruleBuilder.ToArray();
                _isFrozen = true;
            }
        }

        private bool IsStale(CacheItem<TValue> item, DateTime now)
        {
            FreezeIfNeeded();
            var rules = _compiledRules;
            if (rules is null || rules.Length == 0)
            {
                return false;
            }

            foreach (var rule in rules)
            {
                if (rule(item, now))
                {
                    return true;
                }
            }

            return false;
        }

        public TValue Add(TKey key, TValue value)
        {
            ThrowIfDisposed();
            var now = DateTime.UtcNow;
            _cache[key] = new CacheItem<TValue>(value, now);

            TryStartCleanupTimer();
            return value;
        }

        public bool TryGetValue(
            TKey key,
            out TValue value)
        {
            ThrowIfDisposed();
            var now = DateTime.UtcNow;

            if (_cache.TryGetValue(key, out var item))
            {
                if (!IsStale(item, now))
                {
                    value = item.Value;
                    item.Touch(now);
                    return true;
                }

                _cache.TryRemove(key, out _);
            }

            value = default;
            return false;
        }

        public bool Remove(TKey key)
        {
            ThrowIfDisposed();
            return _cache.TryRemove(key, out _);
        }

        public void Clear()
        {
            ThrowIfDisposed();
            _cache.Clear();
            TryStopCleanupTimer();
        }

        private void TryStartCleanupTimer()
        {
            // Only one thread may transition 0 -> 1
            if (Interlocked.CompareExchange(ref _cleanupTimerRunning, 1, 0) == 0)
            {
                _cleanupTimer.Change(
                    TimeSpan.FromSeconds(8),
                    TimeSpan.FromSeconds(8)
                );
            }
        }

        private void TryStopCleanupTimer()
        {
            // Only one thread may transition 1 -> 0
            if (Interlocked.CompareExchange(ref _cleanupTimerRunning, 0, 1) == 1)
            {
                _cleanupTimer.Change(
                    Timeout.InfiniteTimeSpan,
                    Timeout.InfiniteTimeSpan
                );
            }
        }

        private void CleanupExpiredItems(object state)
        {
            if (Volatile.Read(ref _disposed) == 1)
            {
                return;
            }

            if (Interlocked.Exchange(ref _cleanupInProgress, 1) == 1)
            {
                return;
            }

            try
            {
                var now = DateTime.UtcNow;
                foreach (var entry in _cache)
                {
                    if (IsStale(entry.Value, now))
                    {
                        if (!_cache.TryRemove(entry.Key, out _))
                        {
                            _logger.Error($"Failed to remove cache item with key {entry.Key}");
                        }
                    }
                }

                if (_cache.IsEmpty)
                {
                    TryStopCleanupTimer();
                }
            }
            finally
            {
                Interlocked.Exchange(ref _cleanupInProgress, 0);
            }
        }

        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed) == 1)
            {
                throw new ObjectDisposedException(nameof(CacheManager<TKey, TValue>));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }

            if (disposing)
            {
                try
                {
                    _cleanupTimer.Change(
                        Timeout.InfiniteTimeSpan,
                        Timeout.InfiniteTimeSpan);
                }
                catch (ObjectDisposedException)
                {
                    
                }

                _cleanupTimer.Dispose();
                _cache.Clear();
            }
        }

    }

}