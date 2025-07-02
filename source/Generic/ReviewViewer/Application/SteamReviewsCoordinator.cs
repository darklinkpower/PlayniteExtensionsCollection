using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DatabaseCommon;
using Playnite.SDK.Data;
using ReviewViewer.Domain;
using ReviewViewer.Infrastructure;

namespace ReviewViewer.Application
{
    public class SteamReviewsCoordinator
    {
        private readonly ISteamReviewsProvider _reviewProvider;
        private readonly LiteDbRepository<ReviewsResponseRecord> _repository;
        private readonly TimeSpan _cacheDuration;

        public SteamReviewsCoordinator(
            ISteamReviewsProvider reviewProvider,
            LiteDbRepository<ReviewsResponseRecord> repository,
            TimeSpan cacheDuration)
        {
            _reviewProvider = reviewProvider;
            _repository = repository;
            _cacheDuration = cacheDuration;
        }

        /// <summary>
        /// Get reviews for appId with given options.
        /// Will return cached data if fresh, otherwise fetches fresh data.
        /// </summary>
        public async Task<ReviewsResponseDto> GetReviewsAsync(
            int appId,
            QueryOptions options,
            bool forceRefresh,
            CancellationToken cancellationToken,
            string cursor = "*")
        {
            var cacheKey = $"{appId}_{Serialization.ToJson(options)}_{cursor}";
            var hashedCacheKey = cacheKey.ToHashedKey();
            var existingCache = _repository.Find(x => x.CacheKey == hashedCacheKey).FirstOrDefault();
            if (!forceRefresh && existingCache != null && (DateTime.UtcNow - existingCache.CreatedAt) < _cacheDuration)
            {
                return existingCache.Response;
            }

            var freshData = await _reviewProvider.GetReviewsAsync(appId, options, cancellationToken, cursor);
            if (freshData != null)
            {
                if (existingCache != null)
                {
                    _repository.Delete(existingCache.Id);
                }

                var newRecord = new ReviewsResponseRecord
                {
                    CacheKey = hashedCacheKey,
                    Response = freshData
                };

                _repository.Insert(newRecord);
                return newRecord.Response;
            }
            else if (existingCache != null)
            {
                // Fallback to stale cache
                return existingCache.Response;
            }

            return null;
        }

    }
}
