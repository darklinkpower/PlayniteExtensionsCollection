using AngleSharp.Parser.Html;
using Playnite.SDK;
using Playnite.SDK.Data;
using WebCommon;
using SteamCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Threading;

namespace SteamCommon
{
    class SteamWeb
    {
        private static ILogger logger = LogManager.GetLogger();
        private const string steamGameSearchUrl = @"https://store.steampowered.com/search/?term={0}&ignore_preferences=1&category1=998&ndl=1";

        public static List<GenericItemOption> GetSteamSearchGenericItemOptions(string searchTerm)
        {
            return GetSteamSearchResults(searchTerm).Select(x => new GenericItemOption(x.Name, x.GameId)).ToList();
        }

        public static string GetSteamIdFromSearch(string searchTerm, string steamApiCountry = null, CancellationToken cancelToken = default)
        {
            var normalizedName = searchTerm.NormalizeGameName();
            var results = GetSteamSearchResults(normalizedName);
            results.ForEach(a => a.Name = a.Name.NormalizeGameName());

            var matchingGameName = normalizedName.GetMatchModifiedName();
            var exactMatch = results.FirstOrDefault(x => x.Name.GetMatchModifiedName() == matchingGameName);
            if (!(exactMatch is null))
            {
                logger.Info($"Found steam id for search {searchTerm} via steam search, Id: {exactMatch.GameId}");
                return exactMatch.GameId;
            }

            logger.Info($"Steam id for search {searchTerm} not found");
            return null;
        }

        public static List<StoreSearchResult> GetSteamSearchResults(string searchTerm, string steamApiCountry = null, CancellationToken cancelToken = default)
        {
            var results = new List<StoreSearchResult>();
            var searchPageSrc = HttpDownloader.GetRequestBuilder()
                .WithUrl(GetStoreSearchUrl(searchTerm, steamApiCountry))
                .WithCancellationToken(cancelToken)
                .DownloadString();
            if (searchPageSrc.IsSuccessful)
            {
                var parser = new HtmlParser();
                var searchPage = parser.Parse(searchPageSrc.Response.Content);
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
                    var discountPercentage = 0;
                    double priceFinal = 0;
                    double priceOriginal = 0;
                    var isDiscounted = false;
                    string currency = null;
                    var isReleased = false;
                    var isFree = false;

                    var priceData = gameElem.QuerySelector(".search_discount_and_price");
                    if (!priceData.InnerHtml.IsNullOrWhiteSpace())
                    {
                        // Game has pricing data
                        var discountBlock = priceData.QuerySelector(".discount_block");
                        if (discountBlock.HasAttribute("data-discount"))
                        {
                            discountPercentage = int.Parse(discountBlock.GetAttribute("data-discount"));
                        }

                        if (discountBlock.HasAttribute("data-price-final"))
                        {
                            priceFinal = int.Parse(discountBlock.GetAttribute("data-price-final")) * 0.01;
                        }

                        priceOriginal = GetSearchOriginalPrice(priceFinal, discountPercentage);
                        isDiscounted = priceFinal != priceOriginal && priceOriginal != 0;
                        GetCurrencyFromSearchPriceDiv(priceData, out currency, out isReleased, out isFree);
                    }

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
                        IsFree = isFree,
                        IsReleased = isReleased,
                        Currency = currency,
                        BannerImageUrl = capsuleUrl
                    });
                }
            }

            logger.Debug($"Obtained {results.Count} games from Steam search term {searchTerm}");
            return results;
        }

        private static void GetCurrencyFromSearchPriceDiv(AngleSharp.Dom.IElement priceBlock, out string currency, out bool isReleased, out bool isFree)
        {
            currency = GetCurrencyFromPriceString(priceBlock.QuerySelector(".discount_final_price").InnerHtml);
            var noDiscount = priceBlock.QuerySelector(".search_discount_block no_discount");
            if (noDiscount != null)
            {
                // Non discounted item
                isReleased = true;
                isFree = currency == null;
                return;
            }

            var discountDiv = priceBlock.QuerySelector(".search_discount_block");
            if (discountDiv != null)
            {
                // Non discounted item
                isReleased = true;
                isFree = currency == null;
                return;
            }

            isReleased = false;
            currency = null;
            isFree = false;
            return;
        }

        private static string GetCurrencyFromPriceString(string priceString)
        {
            if (!Regex.IsMatch(priceString, @"\d"))
            {
                // Game is free
                return null;
            }

            return Regex.Match(priceString, @"[^\s]+").Value;
        }

        private static string GetStoreSearchUrl(string searchTerm, string steamApiCountry)
        {
            var searchUrl = string.Format(steamGameSearchUrl, searchTerm.EscapeDataString());
            if (!steamApiCountry.IsNullOrEmpty())
            {
                searchUrl += $"&cc={steamApiCountry}";
            }

            return searchUrl;
        }

        private static double GetSearchOriginalPrice(double priceFinal, int discountPercentage)
        {
            if (discountPercentage == 0)
            {
                return priceFinal;
            }

            return (100 * priceFinal) / (100 - discountPercentage);
        }

        private const string steamAppDetailsMask = @"https://store.steampowered.com/api/appdetails?appids={0}";
        public static SteamAppDetails GetSteamAppDetails(string steamId, CancellationToken cancelToken = default)
        {
            var url = string.Format(steamAppDetailsMask, steamId);
            var request = HttpDownloader.GetRequestBuilder().WithUrl(url).WithCancellationToken(cancelToken);
            var downloadedString = request.DownloadString();
            if (downloadedString.IsSuccessful)
            {
                var parsedData = Serialization.FromJson<Dictionary<string, SteamAppDetails>>(downloadedString.Response.Content);
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