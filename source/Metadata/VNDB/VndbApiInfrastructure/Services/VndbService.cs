using FlowHttp;
using FlowHttp.Constants;
using Newtonsoft.Json;
using Playnite.SDK;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThrottlerSharp;
using VndbApiDomain.CharacterAggregate;
using VndbApiDomain.DatabaseDumpTraitAggregate;
using VndbApiDomain.ProducerAggregate;
using VndbApiDomain.ReleaseAggregate;
using VndbApiDomain.StaffAggregate;
using VndbApiDomain.TagAggregate;
using VndbApiDomain.TraitAggregate;
using VndbApiDomain.VisualNovelAggregate;
using VndbApiInfrastructure.CharacterAggregate;
using VndbApiInfrastructure.DatabaseDumpTagAggregate;
using VndbApiInfrastructure.ProducerAggregate;
using VndbApiInfrastructure.ReleaseAggregate;
using VndbApiInfrastructure.SharedKernel.Requests;
using VndbApiInfrastructure.SharedKernel.Responses;
using VndbApiInfrastructure.StaffAggregate;
using VndbApiInfrastructure.TagAggregate;
using VndbApiInfrastructure.TraitAggregate;
using VndbApiInfrastructure.VisualNovelAggregate;

namespace VndbApiInfrastructure.Services
{
    public static class VndbService
    {
        private const string _baseApiEndpoint = @"https://api.vndb.org/kana";
        private const string _postVnEndpoint = @"/vn";
        private const string _postReleaseEndpoint = @"/release";
        private const string _postProducerEndpoint = @"/producer";
        private const string _postCharacterEndpoint = @"/character";
        private const string _postStaffEndpoint = @"/staff";
        private const string _postTagEndpoint = @"/tag";
        private const string _postTraitEndpoint = @"/trait";
        private static readonly Dictionary<int, string> _errorMessages;
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly RateLimiter _requestsRateLimiter;

        static VndbService()
        {
            // The server will allow up to 200 requests per 5 minutes and up to 
            // 1 second of execution time per minute. Using less for safety.
            _requestsRateLimiter = RateLimiterBuilder.Create()
                .WithRequestLimit(180, TimeSpan.FromMinutes(5))
                .WithMinInterval(TimeSpan.FromMilliseconds(150))
                .WithWaitForSlotMode()
                .Build();
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

        private static async Task<string> ExecuteRequestAsync(string endpointRoute, string postBody, CancellationToken cancellationToken)
        {
            var url = string.Concat(_baseApiEndpoint, endpointRoute);
            var request = HttpRequestFactory.GetHttpRequest()
                .WithUrl(url)
                .WithPostHttpMethod()
                .WithContent(postBody, HttpContentTypes.Json);

            var operationResult = await _requestsRateLimiter.ExecuteAsync(
                async () => await request.DownloadStringAsync(cancellationToken),
                cancellationToken);

            if (!operationResult.Success)
            {
                return null;
            }

            var result = operationResult.Result;
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

                _logger.Error(result.Error, $"Failed to perform request. Status code: \"{errorCode}\". Reason: \"{errorReason}\". Endpoint: \"{url}\". PostBody: \"{postBody}\"");
            }

            return null;
        }

        public static async Task<VndbDatabaseQueryReponse<Producer>> ExecutePostRequestAsync(ProducerRequestQuery query, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteRequestAsync(_postProducerEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
            if (result is null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<VndbDatabaseQueryReponse<Producer>>(result);
        }

        public static async Task<VndbDatabaseQueryReponse<Staff>> ExecutePostRequestAsync(StaffRequestQuery query, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteRequestAsync(_postStaffEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
            if (result is null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<VndbDatabaseQueryReponse<Staff>>(result);
        }

        public static async Task<VndbDatabaseQueryReponse<Trait>> ExecutePostRequestAsync(TraitRequestQuery query, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteRequestAsync(_postTraitEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
            if (result is null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<VndbDatabaseQueryReponse<Trait>>(result);
        }

        public static async Task<VndbDatabaseQueryReponse<Tag>> ExecutePostRequestAsync(TagRequestQuery query, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteRequestAsync(_postTagEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
            if (result is null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<VndbDatabaseQueryReponse<Tag>>(result);
        }

        public static async Task<VndbDatabaseQueryReponse<Character>> ExecutePostRequestAsync(CharacterRequestQuery query, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteRequestAsync(_postCharacterEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
            if (result is null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<VndbDatabaseQueryReponse<Character>>(result);
        }

        public static async Task<VndbDatabaseQueryReponse<Release>> ExecutePostRequestAsync(ReleaseRequestQuery query, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteRequestAsync(_postReleaseEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
            if (result is null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<VndbDatabaseQueryReponse<Release>>(result);
        }

        public static async Task<VndbDatabaseQueryReponse<VisualNovel>> ExecutePostRequestAsync(VisualNovelRequestQuery query, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteRequestAsync(_postVnEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
            if (result is null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<VndbDatabaseQueryReponse<VisualNovel>>(result);
        }

        public static async Task<string> GetResponseFromPostRequest(ProducerRequestQuery query, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequestAsync(_postProducerEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
        }

        public static async Task<string> GetResponseFromPostRequest(StaffRequestQuery query, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequestAsync(_postStaffEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
        }

        public static async Task<string> GetResponseFromPostRequest(TraitRequestQuery query, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequestAsync(_postTraitEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
        }

        public static async Task<string> GetResponseFromPostRequest(TagRequestQuery query, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequestAsync(_postTagEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
        }

        public static async Task<string> GetResponseFromPostRequest(CharacterRequestQuery query, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequestAsync(_postCharacterEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
        }

        public static async Task<string> GetResponseFromPostRequest(ReleaseRequestQuery query, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequestAsync(_postReleaseEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
        }

        public static async Task<string> GetResponseFromPostRequest(VisualNovelRequestQuery query, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequestAsync(_postVnEndpoint, JsonConvert.SerializeObject(query), cancellationToken);
        }

        public static async Task<List<DatabaseDumpTag>> GetDatabaseDumpsTags(CancellationToken cancellationToken = default)
        {
            return await DownloadDatabaseDump<DatabaseDumpTag>(
                DatabaseDumpsUrls.TagsUrl, cancellationToken);
        }

        public static async Task<List<DatabaseDumpTrait>> GetDatabaseDumpsTraits(CancellationToken cancellationToken = default)
        {
            return await DownloadDatabaseDump<DatabaseDumpTrait>(
                DatabaseDumpsUrls.TraitsUrl, cancellationToken);
        }

        private async static Task<List<T>> DownloadDatabaseDump<T>(string downloadUrl, CancellationToken cancellationToken)
        {
            var tempGzFile = Path.Combine(Path.GetTempPath(), "file.gz");
            var tempExtractedFile = Path.Combine(Path.GetTempPath(), "file.json");

            try
            {
                if (FileSystem.FileExists(tempGzFile))
                {
                    FileSystem.DeleteFileSafe(tempGzFile);
                }

                if (FileSystem.FileExists(tempExtractedFile))
                {
                    FileSystem.DeleteFileSafe(tempExtractedFile);
                }

                var request = HttpRequestFactory.GetHttpFileRequest()
                    .WithUrl(downloadUrl)
                    .WithDownloadTo(tempGzFile);
                var result = await request.DownloadFileAsync(cancellationToken);
                if (!result.IsSuccess)
                {
                    return null;
                }

                ExtractGZipFile(tempGzFile, tempExtractedFile);
                var fileContent = FileSystem.ReadStringFromFile(tempExtractedFile);
                return JsonConvert.DeserializeObject<List<T>>(fileContent);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to get dumps from {downloadUrl}");
                return null;
            }
            finally
            {
                if (FileSystem.FileExists(tempGzFile))
                {
                    FileSystem.DeleteFileSafe(tempGzFile);
                }

                if (FileSystem.FileExists(tempExtractedFile))
                {
                    FileSystem.DeleteFileSafe(tempExtractedFile);
                }
            }
        }

        public static void ExtractGZipFile(string sourceFile, string destinationFile)
        {
            using (FileStream originalFileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
            {
                using (FileStream decompressedFileStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }
        }


    }
}