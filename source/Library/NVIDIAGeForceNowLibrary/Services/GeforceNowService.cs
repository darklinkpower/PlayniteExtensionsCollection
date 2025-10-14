using FlowHttp;
using FlowHttp.Constants;
using NVIDIAGeForceNowEnabler.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NVIDIAGeForceNowEnabler.Services
{
    
    public static class GeforceNowService
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private const string GraphQlEndpoint = @"https://api-prod.nvidia.com/services/gfngames/v1/gameList";

        private static string GetQuery(
            string after,
            string country = "US",
            string language = "en_US") => $@"
{{
    apps(country:""{country}"", language:""{language}"", after: ""{after}"") {{
        numberReturned,
        pageInfo {{
            hasNextPage,
            endCursor
        }},
        items {{
            id,
            cmsId,
            title,
            type,
            variants {{
                id,
                title,
                appStore,
                gfn {{
                    releaseDate
                }},
                osType,
                storeId
            }}
        }}
    }}
}}";

        public static List<GeforceNowItem> GetGeforceNowDatabase()
        {
            var afterValue = string.Empty;
            var geforceNowItems = new List<GeforceNowItem>();
            int pageCount = 0;

            _logger.Info("Fetching GeForce NOW database...");
            while (true)
            {
                pageCount++;
                var query = GetQuery(afterValue);
                var downloadedString = HttpRequestFactory.GetHttpRequest()
                    .WithUrl(GraphQlEndpoint)
                    .WithContent(query, HttpContentTypes.Json)
                    .WithPostHttpMethod()
                    .DownloadString();

                if (!downloadedString.IsSuccess)
                {
                    _logger.Warn($"Request failed at page {pageCount}. Stopping fetch.");
                    break;
                }

                var response = Serialization.FromJson<GfnGraphQlResponse>(downloadedString.Content);
                geforceNowItems.AddRange(response.Data.Apps.Items);
                _logger.Debug($"Fetched page {pageCount}: {response.Data.Apps.Items.Count} items");

                if (!response.Data.Apps.PageInfo.HasNextPage)
                {
                    break;
                }

                afterValue = response.Data.Apps.PageInfo.EndCursor;
            }

            _logger.Info($"Finished fetching GeForce NOW database. Total pages: {pageCount}, Total items: {geforceNowItems.Count}");
            return geforceNowItems;
        }
    }
}