using ComposableAsync;
using FlowHttp;
using RateLimiter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCriticMetadata.Infrastructure.Http
{
    internal class RateLimitedHttpRequester
    {
        private readonly TimeLimiter limiter;

        public RateLimitedHttpRequester(int maxRequests, TimeSpan interval)
        {
            limiter = TimeLimiter.GetFromMaxCountByInterval(maxRequests, interval);
        }

        public async Task<string> GetStringAsync(
            string url,
            Dictionary<string, string> headers,
            CancellationToken cancelToken)
        {
            await limiter;
            var request = HttpRequestFactory.GetHttpRequest()
                .WithUrl(url);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.AddHeader(header.Key, header.Value);
                }
            }

            var result = request.DownloadString(cancelToken);
            if (!result.IsSuccess)
            {
                throw new HttpRequestException(
                    $"HTTP request failed: {result.Error}. StatusCode: {result.HttpStatusCode}");
            }

            return result.Content;
        }
    }
}
