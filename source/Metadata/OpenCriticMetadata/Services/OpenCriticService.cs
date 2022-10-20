using OpenCriticMetadata.Models;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCommon;
using System.Windows.Threading;
using System.Threading;

namespace OpenCriticMetadata.Services
{
    public class OpenCriticService
    {
        private const string searchGameTemplate = @"https://api.opencritic.com/api/game/search?criteria={0}";
        private const string getGameDataTemplate = @"https://api.opencritic.com/api/game/{0}";
        private readonly int maxRequestsPerSecond;
        private readonly DispatcherTimer rateLimitedTimer;
        private int apiRequestsRemaining;

        public OpenCriticService()
        {
            maxRequestsPerSecond = 4;
            apiRequestsRemaining = maxRequestsPerSecond;
            rateLimitedTimer = new DispatcherTimer();
            rateLimitedTimer.Interval = TimeSpan.FromMilliseconds(5000);
            rateLimitedTimer.Tick += new EventHandler(RateLimitedTimer_Tick);
        }

        private void RateLimitedTimer_Tick(object sender, EventArgs e)
        {
            if (apiRequestsRemaining < maxRequestsPerSecond)
            {
                apiRequestsRemaining++;
            }

            if (apiRequestsRemaining == maxRequestsPerSecond)
            {
                rateLimitedTimer.Stop();
            }
        }

        public static List<OpenCriticGameResult> GetGameSearchResults(string searchTerm)
        {
            var requestUrl = string.Format(searchGameTemplate, searchTerm.UrlEncode());
            var request = HttpDownloader.DownloadStringWithHeaders(requestUrl, GetSearchHeaders());
            if (request.Success)
            {
                return Serialization.FromJson<List<OpenCriticGameResult>>(request.Result);
            }
            else
            {
                return new List<OpenCriticGameResult>();
            }
        }

        public OpenCriticGameData GetGameData(OpenCriticGameResult gameData)
        {
            return GetGameData(gameData.Id.ToString());
        }

        public OpenCriticGameData GetGameData(string gameId)
        {
            var requestUrl = string.Format(getGameDataTemplate, gameId);
            var request = ProcessRequest(requestUrl);
            if (request.Success)
            {
                return Serialization.FromJson<OpenCriticGameData>(request.Result);
            }
            else
            {
                return null;
            }
        }

        private DownloadStringResult ProcessRequest(string requestUrl)
        {
            if (apiRequestsRemaining <= 0)
            {
                Thread.Sleep(250);
            }

            var request = HttpDownloader.DownloadStringWithHeaders(requestUrl, GetSearchHeaders());
            apiRequestsRemaining--;
            if (!rateLimitedTimer.IsEnabled)
            {
                rateLimitedTimer.Start();
            }

            return request;
        }

        private static Dictionary<string, string> GetSearchHeaders()
        {
            return new Dictionary<string, string> {
                {
                    "Referer", @"https://opencritic.com"
                }
            };
        }
    }
}
