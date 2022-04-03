using NVIDIAGeForceNowEnabler.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon.Web;
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
        private static ILogger logger = LogManager.GetLogger();
        private const string graphQlEndpoint = @"https://games.geforce.com/graphql?query=";
        private const string queryBaseString = @"
        {{
            apps(vpcId: ""NP-SEA-01"" language: ""en_US"" first: 1300, after: ""{0}"") {{
              numberReturned
              pageInfo {{
                hasNextPage
                endCursor
              }}
              items {{
                id
                cmsId
                title
                type
                variants {{
                  id
                  title
                  appStore
                  gfn {{
                    status
                    releaseDate
                  }}
                  osType
                  storeId
                }}
              }}
            }}
        }}";

        public static List<GeforceNowItem> GetGeforceNowDatabase()
        {
            logger.Debug($"Get GeForce Now database start");
            var afterValue = "0".Base64Encode();
            var geforceNowItems = new List<GeforceNowItem>();
            while (true)
            {
                var queryString = string.Format(queryBaseString, afterValue).UrlEncode();
                var uri = graphQlEndpoint + queryString;
                var downloadedString = HttpDownloader.DownloadString(uri);
                if (downloadedString.IsNullOrEmpty())
                {
                    break;
                }

                var response = Serialization.FromJson<GfnGraphQlResponse>(downloadedString);
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