using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;

namespace GamesSizeCalculator.GOG
{
    /// <summary>
    /// Shamelessly stolen from https://github.com/JosefNemec/PlayniteExtensions/blob/master/source/Libraries/GogLibrary/Services/GogApiClient.cs
    /// </summary>
    public class GogApiClient
    {
        private static string EnStoreLocaleString = "US_USD_en-US";
        private ILogger logger = LogManager.GetLogger();

        public IHttpDownloader HttpDownloader { get; }

        public GogApiClient(IHttpDownloader httpDownloader)
        {
            HttpDownloader = httpDownloader;
        }

        public StorePageResult.ProductDetails GetGameStoreData(string gameUrl)
        {
            string[] data;

            try
            {
                data = HttpDownloader.DownloadString(gameUrl, new List<System.Net.Cookie>() { new System.Net.Cookie("gog_lc", EnStoreLocaleString) }).Split('\n');
            }
            catch (System.Net.WebException)
            {
                return null;
            }

            var dataStarted = false;
            var stringData = string.Empty;
            foreach (var line in data)
            {
                var trimmed = line.TrimStart();
                if (line.TrimStart().StartsWith("window.productcardData"))
                {
                    dataStarted = true;
                    stringData = trimmed.Substring(25).TrimEnd(';');
                    continue;
                }

                if (line.TrimStart().StartsWith("window.activeFeatures"))
                {
                    var desData = Newtonsoft.Json.JsonConvert.DeserializeObject<StorePageResult>(stringData.TrimEnd(';'));
                    if (desData.cardProduct == null)
                    {
                        return null;
                    }

                    return desData.cardProduct;
                }

                if (dataStarted)
                {
                    stringData += trimmed;
                }
            }

            logger.Warn("Failed to get store data from page, no data found. " + gameUrl);
            return null;
        }

        public ProductApiDetail GetGameDetails(string id)
        {
            var baseUrl = @"https://api.gog.com/products/{0}?expand=description";

            try
            {
                var stringData = HttpDownloader.DownloadString(string.Format(baseUrl, id), new List<System.Net.Cookie>() { new System.Net.Cookie("gog_lc", EnStoreLocaleString) });
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ProductApiDetail>(stringData);
            }
            catch (System.Net.WebException exc)
            {
                logger.Warn(exc, "Failed to download GOG game details for " + id);
                return null;
            }
        }

        public List<StoreGamesFilteredListResponse.Product> GetStoreSearch(string searchTerm)
        {
            var baseUrl = @"https://www.gog.com/games/ajax/filtered?limit=20&search={0}";
            var url = string.Format(baseUrl, System.Net.WebUtility.UrlEncode(searchTerm));

            try
            {
                var stringData = HttpDownloader.DownloadString(url);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<StoreGamesFilteredListResponse>(stringData)?.products;
            }
            catch (System.Net.WebException exc)
            {
                logger.Warn(exc, "Failed to get GOG store search data for " + searchTerm);
                return null;
            }
        }
    }
}
