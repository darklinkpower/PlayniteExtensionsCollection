using SteamShortcuts.Domain.ValueObjects;
using SteamCommon;
using System.Threading;
using System.Threading.Tasks;
using SteamShortcuts.Infrastructure;
using System.Diagnostics;
using Playnite.SDK;

namespace SteamShortcuts.Application
{
    public class DiscoverSupportedFeaturesService
    {
        private class ResponseCache
        {
            public string steamId;
            public SupportedFeatures feature;

            public ResponseCache() { }
            public ResponseCache(string steamId, SupportedFeatures feature)
            {
                this.steamId = steamId;
                this.feature = feature;
            }
        }

        private static readonly object _lock = new object();
        private static CancellationTokenSource _cts;
        private static string _runningSteamId = null;
        private static Task<ResponseCache> _runningRequestTask = Task.FromResult((ResponseCache)null);
        private static ResponseCache _cache = new ResponseCache();
        
        public SupportedFeatures GetCachedFeature(string steamId)
        {
            if (_cache is null)
            {
                return null;
            }

            if (steamId == _cache.steamId)
            {
                return _cache.feature;
            }

            return null;
        }

        public async Task<SupportedFeatures> GetFeatures(string steamId)
        {
            Task<ResponseCache> task;
            lock (_lock)
            {
                if (_cache != null && steamId == _cache.steamId)
                {
                    return _cache.feature;
                }

                CancellationToken cancelToken;
                if (_runningRequestTask.IsCompleted || steamId != _runningSteamId)
                {
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _cts = new CancellationTokenSource();
                    cancelToken = _cts.Token;
                    _runningSteamId = steamId;
                    _runningRequestTask = SendRequest(steamId, cancelToken);
                }

                task = _runningRequestTask;
            }

            return (await task)?.feature;
        }

        private async Task<ResponseCache> SendRequest(string steamId, CancellationToken cancelToken)
        {
            var t1 = Task.Run(() => {
                return SteamWeb.GetSteamAppDetails(steamId, cancelToken);
            });

            var t2 = QueryRewardItems.GetPointsShopCount(steamId, cancelToken);

            var appDetails = await t1;
            var pointsShopCount = await t2;

            if (appDetails == null || pointsShopCount == null)
            {
                return null;
            }

            var achievements = false;
            var workshop = false;

            if (appDetails.success)
            {
                foreach (var category in appDetails.data.categories)
                {
                    switch (category.id)
                    {
                        case 22:
                            achievements = true;
                            break;
                        case 30:
                            workshop = true;
                            break;
                    }
                }
            }

            _cache = new ResponseCache(steamId, new SupportedFeatures
            {
                Achievements = achievements,
                Workshop = workshop,
                PointsShop = pointsShopCount.response.total_count > 0,
            });

            return _cache;
        }
    }
}
