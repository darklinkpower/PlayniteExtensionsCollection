using SteamCommon.Models;
using SteamScreenshots.Domain.Interfaces;
using SteamScreenshots.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamScreenshots.Infrastructure.Providers
{
    public class SteamScreenshotProvider : IScreenshotProvider
    {
        private readonly ISteamAppDetailsService _steamAppDetailsService;

        public SteamScreenshotProvider(ISteamAppDetailsService steamAppDetailsService)
        {
            _steamAppDetailsService = steamAppDetailsService;
        }

        public List<ScreenshotData> GetScreenshots(string id, CancellationToken cancellationToken = default)
        {
            var appDetails = TryGetAppDetails(id, cancellationToken);
            if (appDetails?.data?.screenshots.HasItems() != true)
            {
                return new List<ScreenshotData>();
            }

            var screenshotsData = appDetails.data.screenshots
                .Select(s => new ScreenshotData(s.PathThumbnail.ToString(), s.PathFull.ToString()))
                .ToList();

            return screenshotsData;
        }

        private SteamAppDetails TryGetAppDetails(string id, CancellationToken cancellationToken)
        {
            var appDetails = _steamAppDetailsService.GetAppDetails(id, true, cancellationToken);
            if (appDetails is null || !appDetails.success)
            {
                // #550 Due to unknown circumstances, the response can return success:false
                // despite data being available but a redownload fixes it
                _steamAppDetailsService.DeleteAppDetails(id);
                appDetails = _steamAppDetailsService.GetAppDetails(id, true, cancellationToken);
            }

            return appDetails;
        }
    }
}