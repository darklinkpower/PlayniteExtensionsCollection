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
    public class NewsCacheManager
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly ConcurrentDictionary<Guid, NewsRequestCache> cacheDictionary = new ConcurrentDictionary<Guid, NewsRequestCache>();
        private readonly DispatcherTimer cacheCleanupTimer;

        public NewsCacheManager()
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

        public NewsRequestCache GetCache(Guid gameId)
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

        public NewsRequestCache SaveCache(Guid gameId, XmlNodeList xmlNodeList)
        {
            var cache = new NewsRequestCache(DateTime.Now, xmlNodeList);
            cacheDictionary[gameId] = cache;
            cacheCleanupTimer.Start();
            return cache;
        }
    }
}