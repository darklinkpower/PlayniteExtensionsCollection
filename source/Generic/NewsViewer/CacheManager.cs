using NewsViewer.Models;
using Playnite.SDK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;

namespace NewsViewer
{
    public class CacheManager<T>
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly ConcurrentDictionary<Guid, CacheItem<T>> cacheDictionary = new ConcurrentDictionary<Guid, CacheItem<T>>();
        private readonly DispatcherTimer cacheCleanupTimer;

        public CacheManager()
        {
            cacheCleanupTimer = new DispatcherTimer();
            cacheCleanupTimer.Interval = TimeSpan.FromSeconds(20);
            cacheCleanupTimer.Tick += new EventHandler(CleanCache);
        }

        private void CleanCache(object sender, EventArgs e)
        {
            foreach (var cacheItem in cacheDictionary)
            {
                if (DateTime.Now.Subtract(cacheItem.Value.CreationDate) >= TimeSpan.FromSeconds(120))
                {
                    if (!cacheDictionary.TryRemove(cacheItem.Key, out _))
                    {
                        logger.Error($"Failed to remove cache with key {cacheItem.Key} from cache");
                    }
                }
            }

            if (!cacheDictionary.HasItems())
            {
                cacheCleanupTimer.Stop();
            }
        }

        public CacheItem<T> GetCache(Guid gameId)
        {
            if (cacheDictionary.TryGetValue(gameId, out var cache))
            {
                return cache;
            }
            else
            {
                return null;
            }
        }

        public CacheItem<T> SaveCache(Guid id, T item)
        {
            var cache = new CacheItem<T>(DateTime.Now, item);
            cacheDictionary[id] = cache;
            cacheCleanupTimer.Start();
            return cache;
        }
    }
}