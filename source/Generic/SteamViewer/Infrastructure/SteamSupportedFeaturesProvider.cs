using SteamCommon;
using SteamShortcuts.Application.Interfaces;
using SteamShortcuts.Domain.Enums;
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
        public async Task<SupportedFeatures?> GetFeaturesAsync(
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

            // Points shop API always returns a response object even if the app is not in the points shop, so we can check for null to determine if the request failed.
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
            else
            {
                // If there is no appdetails we assume the game has achievements because most games support this feature
                // otherwise we have no way of knowing if the game has achievements or not. A better approach could be investigated.
                achievements = true;
            }

            var supportedFeatures = SupportedFeatures.None;
            if (achievements)
            {
                supportedFeatures |= SupportedFeatures.Achievements;
            }

            if (workshop)
            {
                supportedFeatures |= SupportedFeatures.Workshop;
            }

            if (pointsShopCount.response.total_count > 0)
            {
                supportedFeatures |= SupportedFeatures.PointsShop;
            }

            return supportedFeatures;
        }
    }
}
