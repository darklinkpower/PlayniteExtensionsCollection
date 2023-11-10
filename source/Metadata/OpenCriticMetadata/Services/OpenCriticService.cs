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
using RateLimiter;
using ComposableAsync;
using WebCommon.Models;

namespace OpenCriticMetadata.Services
{
    public class OpenCriticService
    {
        private const string searchGameTemplate = @"https://api.opencritic.com/api/game/search?criteria={0}";
        private const string getGameDataTemplate = @"https://api.opencritic.com/api/game/{0}";
        private readonly TimeLimiter timeConstraint;

        public OpenCriticService()
        {
            timeConstraint = TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(600));
        }

        public async Task<StringHttpDownloaderResult> ExecuteRequestAsync(string requestUrl)
        {
            await timeConstraint;
            return HttpDownloader.GetRequestBuilder()
                .WithUrl(requestUrl)
                .WithHeaders(GetSearchHeaders())
                .DownloadString();
        }

        public List<OpenCriticGameResult> GetGameSearchResults(string searchTerm)
        {
            var requestUrl = string.Format(searchGameTemplate, searchTerm.EscapeDataString());
            var result = Task.Run(async () => await ExecuteRequestAsync(requestUrl)).Result;
            if (result.IsSuccessful)
            {
                return Serialization.FromJson<List<OpenCriticGameResult>>(result.Response.Content);
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
            var result = Task.Run(async () => await ExecuteRequestAsync(requestUrl)).Result;
            if (result.IsSuccessful)
            {
                return Serialization.FromJson<OpenCriticGameData>(result.Response.Content);
            }
            else
            {
                return null;
            }
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
