using NewsViewer.Models;
using Playnite.SDK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NewsViewer
{
    public class PlayersCountCacheManager
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly ConcurrentDictionary<Guid, GamePlayersCountCache> cacheDictionary = new ConcurrentDictionary<Guid, GamePlayersCountCache>();
        private readonly DispatcherTimer cacheCleanupTimer;

        public PlayersCountCacheManager()
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

        public GamePlayersCountCache GetCache(Guid gameId)
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

        public GamePlayersCountCache SaveCache(Guid gameId, long playersCount)
        {
            var cache = new GamePlayersCountCache(DateTime.Now, playersCount);
            cacheDictionary[gameId] = cache;
            cacheCleanupTimer.Start();
            return cache;
        }
    }
}