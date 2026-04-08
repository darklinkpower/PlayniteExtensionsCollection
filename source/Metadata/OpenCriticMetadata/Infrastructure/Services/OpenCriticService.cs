using ComposableAsync;
using FlowHttp;
using FlowHttp.Results;
using OpenCriticMetadata.Domain.Entities;
using OpenCriticMetadata.Domain.Interfaces;
using Playnite.SDK.Data;
using RateLimiter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

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

        private static void ValidateApiKey(string apiKey)
        {
            if (apiKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("OpenCritic API key is missing.", nameof(apiKey));
            }
        }

        private async Task<string> ExecuteRequestAsync(
            string apiKey, string requestUrl, CancellationToken cancelToken)
        {
            await timeConstraint;
            var requestResult = HttpRequestFactory.GetHttpRequest()
                .WithUrl(requestUrl)
                .AddHeader("Authorization", $"Bearer {apiKey}")
                .DownloadString(cancelToken);

            if (!requestResult.IsSuccess)
            {
                throw new HttpRequestException(
                    $"OpenCritic API request failed: {requestResult.Error}. StatusCode: {requestResult.HttpStatusCode}");
            }

            return requestResult.Content;
        }

        public async Task<List<OpenCriticGameResult>> GetGameSearchResultsAsync(
            string apiKey, string searchTerm, CancellationToken cancelToken = default)
        {
            ValidateApiKey(apiKey);
            var requestUrl = string.Format(_searchGameTemplate, searchTerm.EscapeDataString());
            var requestResult = await ExecuteRequestAsync(apiKey, requestUrl, cancelToken);
            var result = Serialization.FromJson<List<OpenCriticGameResult>>(requestResult);
            return result;
        }

        public async Task<OpenCriticGameData> GetGameDataAsync(
            string apiKey, OpenCriticGameResult gameData, CancellationToken cancelToken = default)
        {
            if (gameData is null)
            {
                throw new ArgumentNullException(nameof(gameData));
            }

            return await GetGameDataAsync(apiKey, gameData.Id.ToString(), cancelToken);
        }

        public async Task<OpenCriticGameData> GetGameDataAsync(
            string apiKey, string gameId, CancellationToken cancelToken)
        {
            ValidateApiKey(apiKey);
            if (gameId.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("gameId is missing.", nameof(gameId));
            }

            var requestUrl = string.Format(_getGameDataTemplate, gameId.EscapeDataString());
            var requestResult = await ExecuteRequestAsync(apiKey, requestUrl, cancelToken);
            var result = Serialization.FromJson<OpenCriticGameData>(requestResult);
            return result;
        }
    }
}