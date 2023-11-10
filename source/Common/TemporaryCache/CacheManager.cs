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
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly ConcurrentDictionary<TKey, CacheItem<TValue>> cacheDictionary = new ConcurrentDictionary<TKey, CacheItem<TValue>>();
        private readonly TimeSpan _cacheAliveTime;
        private readonly DispatcherTimer _cacheCleanupTimer;
        private bool _cleanupInProgress = false;

        public CacheManager(TimeSpan cacheAliveTime)
        {
            _cacheAliveTime = cacheAliveTime;
            _cacheCleanupTimer = new DispatcherTimer();
            _cacheCleanupTimer.Interval = TimeSpan.FromSeconds(10);
            _cacheCleanupTimer.Tick += new EventHandler(CleanCache);
        }

        private void CleanCache(object sender, EventArgs e)
        {
            if (_cleanupInProgress)
            {
                return;
            }

            _cleanupInProgress = true;
            var currentTime = DateTime.Now;
            foreach (var cacheItem in cacheDictionary)
            {
                if (currentTime >= cacheItem.Value.ExpirationDate)
                {
                    if (!cacheDictionary.TryRemove(cacheItem.Key, out _))
                    {
                        logger.Error($"Failed to remove cache with key {cacheItem.Key} from cache");
                    }
                }
            }

            if (cacheDictionary.Values.Count == 0)
            {
                _cacheCleanupTimer.Stop();
            }

            _cleanupInProgress = false;
        }

        public CacheItem<TValue> GetCache(TKey key, bool refreshExpirationDate = false)
        {
            if (cacheDictionary.TryGetValue(key, out var cache))
            {
                if (refreshExpirationDate)
                {
                    cache.RefreshExpirationDate();
                }

                return cache;
            }

            return null;
        }

        public CacheItem<TValue> SaveCache(TKey key, TValue item)
        {
            var cache = new CacheItem<TValue>(_cacheAliveTime, item);
            cacheDictionary[key] = cache;
            _cacheCleanupTimer.Start();
            return cache;
        }
    }
}