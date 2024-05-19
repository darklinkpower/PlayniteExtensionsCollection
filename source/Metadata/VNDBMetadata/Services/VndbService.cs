using ComposableAsync;
using FlowHttp;
using FlowHttp.Constants;
using Newtonsoft.Json;
using Playnite.SDK;
using RateLimiter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VNDBMetadata.Queries;
using VNDBMetadata.Requests.PostProducer;

namespace VNDBMetadata.Services
{
    public class VndbService
    {
        private const string baseApiEndpoint = @"https://api.vndb.org/kana";
        private const string postVnEndpoint = @"/vn";
        private const string postReleaseEndpoint = @"/release";
        private const string postProducerEndpoint = @"/producer";
        private const string postCharacterEndpoint = @"/character";
        private const string postStaffEndpoint = @"/staff";
        private const string postTagEndpoint = @"/tag";
        private const string postTraitEndpoint = @"/trait";
        private readonly TimeLimiter _requestsLimiter;
        private readonly Dictionary<int, string> _errorMessages;
        private static readonly ILogger _logger = LogManager.GetLogger();

        public VndbService()
        {
            // The server will allow up to 200 requests per 5 minutes and up to 1 second of execution time per minute.
            // Using less for safety
            _requestsLimiter = TimeLimiter.GetFromMaxCountByInterval(30, TimeSpan.FromMinutes(1));
            _errorMessages = new Dictionary<int, string>
            {
                { 400, "Invalid request body or query, the included error message hopefully points at the problem." },
                { 401, "Invalid authentication token." },
                { 404, "Invalid API path or HTTP method." },
                { 429, "Throttled." },
                { 500, "Server error, usually points to a bug if this persists." },
                { 502, "Server is down, should be temporary." }
            };
        }

        private async Task<string> ExecuteRequestAsync(string endpointRoute, string postBody, CancellationToken cancellationToken)
        {
            var url = string.Concat(baseApiEndpoint, endpointRoute);
            var request = HttpRequestFactory.GetHttpRequest()
                .WithUrl(url)
                .WithPostHttpMethod()
                .WithContent(postBody, HttpContentTypes.Json);
            var result = await _requestsLimiter.Enqueue(
                () => request.DownloadString(cancellationToken),
                cancellationToken);

            if (result.IsSuccess)
            {
                return result.Content;
            }
            else if (!result.IsCancelled)
            {
                var errorReason = "Unknown error.";
                int? errorCode = null;
                if (result.HttpStatusCode != null)
                {
                    errorCode = (int)result.HttpStatusCode;
                    _errorMessages.TryGetValue(errorCode.Value, out errorReason);
                    var isRateLimited = errorCode == 429;
                }

                _logger.Error(result.Error, $"Failed to perform request. Status code: \"{errorCode}\". Reason: \"{errorReason}\". Message: \"{result.ResponseReaderPhrase}\"");
            }

            return null;
        }

        public async Task<ProducerResponse> ExecutePostRequestAsync(PostProducerQuery query, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteRequestAsync(postProducerEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
            if (result is null)
            {
                return null;
            }

            var deserializedObject = JsonConvert.DeserializeObject<ProducerResponse>(result);
            return deserializedObject;
        }

    }
}
