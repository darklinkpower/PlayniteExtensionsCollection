using OpenCriticMetadata.Models;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowHttp;
using System.Windows.Threading;
using System.Threading;
using RateLimiter;
using ComposableAsync;
using FlowHttp.Results;

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

        private async Task<HttpContentResult<string>> ExecuteRequestAsync(string requestUrl, CancellationToken cancelToken)
        {
            await timeConstraint;
            return HttpRequestFactory.GetHttpRequest()
                .WithUrl(requestUrl)
                .AddHeader("Referer", @"https://opencritic.com")
                .DownloadString(cancelToken);
        }

        public List<OpenCriticGameResult> GetGameSearchResults(string searchTerm, CancellationToken cancelToken = default)
        {
            var requestUrl = string.Format(searchGameTemplate, searchTerm.EscapeDataString());
            var result = Task.Run(async () => await ExecuteRequestAsync(requestUrl, cancelToken)).Result;
            if (result.IsSuccess)
            {
                return Serialization.FromJson<List<OpenCriticGameResult>>(result.Content);
            }
            else
            {
                return new List<OpenCriticGameResult>();
            }
        }

        public OpenCriticGameData GetGameData(OpenCriticGameResult gameData, CancellationToken cancelToken = default)
        {
            return GetGameData(gameData.Id.ToString(), cancelToken);
        }

        public OpenCriticGameData GetGameData(string gameId, CancellationToken cancelToken = default)
        {
            var requestUrl = string.Format(getGameDataTemplate, gameId);
            var result = Task.Run(async () => await ExecuteRequestAsync(requestUrl, cancelToken)).Result;
            if (result.IsSuccess)
            {
                return Serialization.FromJson<OpenCriticGameData>(result.Content);
            }
            else
            {
                return null;
            }
        }

    }
}
