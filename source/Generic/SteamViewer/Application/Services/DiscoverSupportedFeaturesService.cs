using SteamShortcuts.Application.Interfaces;
using SteamShortcuts.Application.Models;
using SteamShortcuts.Domain.Models;
using SteamShortcuts.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamShortcuts.Application.Services
{
    public class DiscoverSupportedFeaturesService
    {
        private readonly ISupportedFeaturesCacheRepository _cacheRepository;
        private readonly ISupportedFeaturesProvider _featuresProvider;
        private readonly TimeSpan? _cacheExpiration;

        private readonly object _lock = new object();

        private readonly Dictionary<string, Task<SupportedFeatures>> _runningRequests =
            new Dictionary<string, Task<SupportedFeatures>>();

        public DiscoverSupportedFeaturesService(
            ISupportedFeaturesCacheRepository cacheRepository,
            ISupportedFeaturesProvider featuresProvider,
            TimeSpan? cacheExpiration = null)
        {
            _cacheRepository = cacheRepository;
            _featuresProvider = featuresProvider;
            _cacheExpiration = cacheExpiration;
        }

        public SupportedFeatures GetCachedFeatures(string steamId)
        {
            var entry = _cacheRepository.Get(steamId);
            return entry?.Features;
        }

        public async Task<SupportedFeatures> GetFeaturesAsync(string steamId)
        {
            if (steamId.IsNullOrEmpty())
            {
                return null;
            }

            var cachedEntry = _cacheRepository.Get(steamId);
            if (cachedEntry != null && !IsExpired(cachedEntry))
            {
                return cachedEntry.Features;
            }

            return await GetOrStartRequestAsync(steamId, cachedEntry);
        }

        private bool IsExpired(SupportedFeaturesCacheEntry entry)
        {
            if (!_cacheExpiration.HasValue)
            {
                return false;
            }

            return DateTime.UtcNow - entry.CreatedAt >= _cacheExpiration;
        }

        private Task<SupportedFeatures> GetOrStartRequestAsync(
            string steamId,
            SupportedFeaturesCacheEntry cachedEntry)
        {
            lock (_lock)
            {
                Task<SupportedFeatures> runningTask;

                if (_runningRequests.TryGetValue(steamId, out runningTask))
                {
                    return runningTask;
                }

                runningTask = RefreshFeaturesAsync(steamId, cachedEntry);

                _runningRequests.Add(steamId, runningTask);

                return runningTask;
            }
        }

        private async Task<SupportedFeatures> RefreshFeaturesAsync(
            string steamId,
            SupportedFeaturesCacheEntry cachedEntry)
        {
            try
            {
                var features = await _featuresProvider.GetFeaturesAsync(
                    steamId,
                    CancellationToken.None);

                if (features is null)
                {
                    return cachedEntry?.Features;
                }

                var entry = new SupportedFeaturesCacheEntry(
                    steamId,
                    features);

                try
                {
                    _cacheRepository.Save(entry);
                }
                catch
                {
                    // Persistence failure must not invalidate successful discovery.
                }

                return features;
            }
            catch
            {
                // Preserve previously cached information when refreshing fails.
                return cachedEntry?.Features;
            }
            finally
            {
                lock (_lock)
                {
                    _runningRequests.Remove(steamId);
                }
            }
        }
    }
}
