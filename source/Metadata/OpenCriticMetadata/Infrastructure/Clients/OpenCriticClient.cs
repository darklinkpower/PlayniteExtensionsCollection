using OpenCriticMetadata.Infrastructure.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCriticMetadata.Infrastructure.Clients
{
    internal class OpenCriticClient
    {
        private readonly OpenCriticMetadataSettings settings;
        private readonly RateLimitedHttpRequester requester;

        public OpenCriticClient(OpenCriticMetadataSettings settings)
        {
            this.settings = settings;
            requester = new RateLimitedHttpRequester(1, TimeSpan.FromMilliseconds(600));
        }

        private static string NormalizeApiKey(string apiKey)
        {
            if (apiKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("OpenCritic API key is missing.", nameof(apiKey));
            }

            const string bearerPrefix = "Bearer ";
            apiKey = apiKey.Trim();
            if (apiKey.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                apiKey = apiKey.Substring(bearerPrefix.Length).Trim();
            }

            return apiKey;
        }

        public async Task<string> GetAsync(string url, CancellationToken token)
        {
            var apiKey = NormalizeApiKey(settings.ApiKey);
            return await requester.GetStringAsync(
                url,
                new Dictionary<string, string>
                {
                { "Authorization", $"Bearer {apiKey}" }
                },
                token);
        }
    }
}
