using FlowHttp;
using Playnite.SDK.Data;
using SteamShortcuts.Domain.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace SteamShortcuts.Infrastructure
{
    class QueryRewardItems
    {
        private const string queryUrl = @"https://api.steampowered.com/ILoyaltyRewardsService/QueryRewardItems/v1/?appids[0]={0}&count=1";

        public static async Task<PointsShopCount> GetPointsShopCount(string appId, CancellationToken cancelToken = default)
        {
            var url = string.Format(queryUrl, appId);
            var request = HttpRequestFactory.GetHttpRequest().WithUrl(url);
            var downloadedString = await request.DownloadStringAsync(cancelToken);

            if (downloadedString.IsSuccess)
            {
                return Serialization.FromJson<PointsShopCount>(downloadedString.Content);
            }

            return null;
        }
    }
}
