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
using System.Threading;
using System.Threading.Tasks;

namespace NVIDIAGeForceNowEnabler.Services
{

    public static class GeforceNowService
    {
        private static ILogger logger = LogManager.GetLogger();
        private const string graphQlEndpoint = @"https://api-prod.nvidia.com/services/gfngames/v1/gameList";
        private const string queryBaseString = @"
        {{
            apps(country:""US"", language:""en_US"", after: ""{0}"") {{
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
                  }}
                  osType,
                  storeId
                }}
              }}
            }}
        }}";

        public static List<GeforceNowItem> GetGeforceNowDatabase()
        {
            logger.Debug($"Get GeForce Now database start");
            var afterValue = "";
            var geforceNowItems = new List<GeforceNowItem>();
            while (true)
            {
                var query = string.Format(queryBaseString, afterValue);
                //var json = Serialization.ToJson(new { query });
                //logger.Debug($"json request gfn: " + json);
                var downloadedString = HttpRequestFactory.GetHttpRequest()
                    .WithUrl(graphQlEndpoint)
                    .WithContent(query, HttpContentTypes.Json)
                    .WithPostHttpMethod()
                    .DownloadString();

                if (!downloadedString.IsSuccess)
                {
                    break;
                }

                var response = Serialization.FromJson<GfnGraphQlResponse>(downloadedString.Content);
                foreach (var geforceNowItem in response.Data.Apps.Items)
                {
                    geforceNowItems.Add(geforceNowItem);
                }

                if (response.Data.Apps.PageInfo.HasNextPage)
                {
                    afterValue = response.Data.Apps.PageInfo.EndCursor;
                    continue;
                }
                else
                {
                    break;
                }
            }

            logger.Debug($"Returning GeForce NOW database with {geforceNowItems.Count} items");
            return geforceNowItems;
        }
    }
}