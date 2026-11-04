using SteamCommon;
using SteamShortcuts.Application.Interfaces;
using SteamShortcuts.Domain.Models;
using SteamShortcuts.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamShortcuts.Infrastructure
{
    public class SteamSupportedFeaturesProvider : ISupportedFeaturesProvider
    {
        public async Task<SupportedFeatures> GetFeaturesAsync(
            string steamId,
            CancellationToken cancellationToken)
        {
            var t1 = Task.Run(() =>
            {
                return SteamWeb.GetSteamAppDetails(steamId, cancellationToken);
            }, cancellationToken);

            var t2 = QueryRewardItems.GetPointsShopCount(
                steamId,
                cancellationToken);

            var appDetails = await t1;
            var pointsShopCount = await t2;

            if (pointsShopCount is null || pointsShopCount.response is null)
            {
                return null;
            }

            var achievements = false;
            var workshop = false;

            // Appdetails is not returned for games removed on the store so we still store its status to not make unnecessary requests to the API for removed games.
            if (appDetails != null &&
                appDetails.success &&
                appDetails.data != null &&
                appDetails.data.categories != null)
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

            return new SupportedFeatures
            {
                Achievements = achievements,
                Workshop = workshop,
                PointsShop = pointsShopCount.response.total_count > 0
            };
        }
    }
}
