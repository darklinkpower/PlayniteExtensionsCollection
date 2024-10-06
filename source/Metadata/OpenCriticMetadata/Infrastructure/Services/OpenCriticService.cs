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
using OpenCriticMetadata.Domain.Entities;
using OpenCriticMetadata.Domain.Interfaces;

namespace OpenCriticMetadata.Infrastructure.Services
{
    public class OpenCriticService : IOpenCriticService
    {
        private const string _searchGameTemplate = @"https://api.opencritic.com/api/game/search?criteria={0}";
        private const string _getGameDataTemplate = @"https://api.opencritic.com/api/game/{0}";
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
        public async Task<List<OpenCriticGameResult>> GetGameSearchResultsAsync(string searchTerm, CancellationToken cancelToken = default)
        {
            var requestUrl = string.Format(_searchGameTemplate, searchTerm.EscapeDataString());
            var result = await ExecuteRequestAsync(requestUrl, cancelToken);
            if (result.IsSuccess)
            {
                return Serialization.FromJson<List<OpenCriticGameResult>>(result.Content);
            }
            else
            {
                return new List<OpenCriticGameResult>();
            }
        }

        public async Task<OpenCriticGameData> GetGameDataAsync(OpenCriticGameResult gameData, CancellationToken cancelToken = default)
        {
            return await GetGameDataAsync(gameData.Id.ToString(), cancelToken);
        }

        public async Task<OpenCriticGameData> GetGameDataAsync(string gameId, CancellationToken cancelToken)
        {
            var requestUrl = string.Format(_getGameDataTemplate, gameId);
            var result = await ExecuteRequestAsync(requestUrl, cancelToken);
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