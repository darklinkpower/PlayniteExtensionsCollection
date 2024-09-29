using TemporaryCache.Models;
using Playnite.SDK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;

namespace TemporaryCache
{
    public class CacheManager<TKey, TValue>
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly ConcurrentDictionary<TKey, CacheItem<TValue>> _cacheDictionary = new ConcurrentDictionary<TKey, CacheItem<TValue>>();
        private readonly TimeSpan _defaultExpirationDuration;
        private readonly DispatcherTimer _cacheCleanupTimer;
        private bool _cleanupInProgress = false;

        public CacheManager(TimeSpan expirationDuration)
        {
            _defaultExpirationDuration = expirationDuration;
            _cacheCleanupTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(8)
            };

            _cacheCleanupTimer.Tick += new EventHandler(CleanupExpiredItems);
        }

        public TValue Add(TKey key, TValue value, TimeSpan? expirationTime = null)
        {
            var expiration = DateTime.UtcNow.Add(expirationTime ?? _defaultExpirationDuration);
            _cacheDictionary[key] = new CacheItem<TValue>(value, expiration);

            if (!_cacheCleanupTimer.IsEnabled)
            {
                _cacheCleanupTimer.Start();
            }

            return value;
        }

        public bool Remove(TKey key)
        {
            return _cacheDictionary.TryRemove(key, out _);
        }

        public void Clear()
        {
            _cacheDictionary.Clear();
            _cacheCleanupTimer.Stop();
        }

        public bool TryGetValue(TKey key, out TValue value, bool refreshExpiration = true, TimeSpan? refreshExpirationTime = null)
        {
            if (_cacheDictionary.TryGetValue(key, out var cacheItem))
            {
                if (!cacheItem.IsExpired)
                {
                    value = cacheItem.Value;

                    if (refreshExpiration)
                    {
                        var newExpiration = DateTime.UtcNow.Add(refreshExpirationTime ?? _defaultExpirationDuration);
                        cacheItem.RefreshExpiration(newExpiration);
                    }

                    return true;
                }
                else
                {
                    _cacheDictionary.TryRemove(key, out _);
                }
            }

            value = default;
            return false;
        }

        private async void CleanupExpiredItems(object sender, EventArgs e)
        {
            if (_cleanupInProgress)
            {
                return;
            }

            _cleanupInProgress = true;

            await Task.Run(() =>
            {
                var expiredItems = _cacheDictionary.Where(x => x.Value.IsExpired).ToList();
                foreach (var cacheItem in expiredItems)
                {
                    if (!_cacheDictionary.TryRemove(cacheItem.Key, out _))
                    {
                        _logger.Error($"Failed to remove cache with key {cacheItem.Key} from cache");
                    }
                }
            });

            if (_cacheDictionary.IsEmpty)
            {
                _cacheCleanupTimer.Stop();
            }

            _cleanupInProgress = false;
        }

    }
}