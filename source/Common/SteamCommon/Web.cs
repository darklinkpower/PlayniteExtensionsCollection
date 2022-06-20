using AngleSharp.Parser.Html;
using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon.Web;
using SteamCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SteamCommon
{
    class SteamWeb
    {
        private static ILogger logger = LogManager.GetLogger();
        private const string steamGameSearchUrl = @"https://store.steampowered.com/search/?term={0}&ignore_preferences=1&category1=998";
        public static List<GenericItemOption> GetSteamSearchGenericItemOptions(string searchTerm)
        {
            return GetSteamSearchResults(searchTerm).Select(x => new GenericItemOption(x.Name, x.GameId)).ToList();
        }

        public static List<StoreSearchResult> GetSteamSearchResults(string searchTerm)
        {
            var results = new List<StoreSearchResult>();
            var searchPageSrc = HttpDownloader.DownloadStringAsync(string.Format(steamGameSearchUrl, searchTerm)).GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(searchPageSrc))
            {
                var parser = new HtmlParser();
                var searchPage = parser.Parse(searchPageSrc);
                foreach (var gameElem in searchPage.QuerySelectorAll(".search_result_row"))
                {
                    if (gameElem.HasAttribute("data-ds-packageid"))
                    {
                        continue;
                    }

                    // Game Data
                    var title = gameElem.QuerySelector(".title").InnerHtml;
                    var releaseDate = gameElem.QuerySelector(".search_released").InnerHtml;
                    var gameId = gameElem.GetAttribute("data-ds-appid");

                    // Prices Data
                    var priceData = gameElem.QuerySelector(".search_price_discount_combined");
                    var discountPercentage = GetSteamSearchDiscount(priceData);
                    var priceFinal = GetSteamSearchFinalPrice(priceData);
                    var priceOriginal = GetSearchOriginalPrice(priceFinal, discountPercentage);
                    var isDiscounted = priceFinal != priceOriginal;

                    //Urls
                    var storeUrl = gameElem.GetAttribute("href");
                    var capsuleUrl = gameElem.QuerySelector(".search_capsule").Children[0].GetAttribute("src");

                    results.Add(new StoreSearchResult
                    {
                        Name = HttpUtility.HtmlDecode(title),
                        Description = HttpUtility.HtmlDecode(releaseDate),
                        GameId = gameId,
                        PriceOriginal = priceOriginal,
                        PriceFinal = priceFinal,
                        IsDiscounted = isDiscounted,
                        DiscountPercentage = discountPercentage,
                        StoreUrl = storeUrl,
                        BannerImageUrl = capsuleUrl
                    });
                }
            }

            logger.Debug($"Obtained {results.Count} games from Steam search term {searchTerm}");
            return results;
        }

        private static double GetSearchOriginalPrice(double priceFinal, int discountPercentage)
        {
            if (discountPercentage == 0)
            {
                return priceFinal;
            }

            return (100 * priceFinal) / (100 - discountPercentage);
        }

        private static int GetSteamSearchDiscount(AngleSharp.Dom.IElement priceData)
        {
            var searchDiscountQuery = priceData.QuerySelector(".search_discount");
            if (searchDiscountQuery.ChildElementCount == 1)
            {
                //TODO Improve parsing
                return int.Parse(searchDiscountQuery.Children[0].TextContent.Replace("-", "").Replace("%", "").Trim());
            }

            return 0;
        }

        private static double GetSteamSearchFinalPrice(AngleSharp.Dom.IElement priceData)
        {
            return int.Parse(priceData.GetAttribute("data-price-final")) * 0.01;
        }

        private const string steamAppDetailsMask = @"https://store.steampowered.com/api/appdetails?appids={0}";
        public static SteamAppDetails GetSteamAppDetails(string steamId)
        {
            var url = string.Format(steamAppDetailsMask, steamId);
            var downloadedString = HttpDownloader.DownloadStringAsync(url).GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(downloadedString))
            {
                var parsedData = Serialization.FromJson<Dictionary<string, SteamAppDetails>>(downloadedString);
                if (parsedData.Keys?.Any() == true)
                {
                    var response = parsedData[parsedData.Keys.First()];
                    if (response.success == true && response.data != null)
                    {
                        return response;
                    }
                }
            }

            return null;
        }
    }
}