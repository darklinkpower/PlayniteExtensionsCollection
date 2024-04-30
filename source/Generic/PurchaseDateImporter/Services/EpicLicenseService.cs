using Playnite.SDK;
using Playnite.SDK.Data;
using PlayniteUtilitiesCommon;
using PurchaseDateImporter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseDateImporter.Services
{
    public static class EpicLicenseService
    {
        public static Guid PluginId = Guid.Parse("00000002-dbd1-46c6-b5d0-b1ba559d10e4");
        public const string LibraryName = "Epic";
        public const string LoginUrl = @"https://store.epicgames.com/";
        private const string epicUserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 Vivaldi/4.3";

        public static Dictionary<string, LicenseData> GetLicensesDict()
        {
            var licensesDictionary = new Dictionary<string, LicenseData>();
            var licenses = GetLicenses();
            foreach (var license in licenses)
            {
                licensesDictionary[license.Name.Normalize()] = license;
            }

            return licensesDictionary;
        }

        public static List<LicenseData> GetLicenses()
        {
            var licensesList = new List<LicenseData>();
            var apiTemplate = "https://www.epicgames.com/account/v2/payment/ajaxGetOrderHistory?sortDir=DESC&sortBy=DATE&locale=en-US&nextPageToken={0}";

            var nextPageToken = DateTime.Now.ToString("u").Replace(" ", "T");
            using (var webView = Playnite.SDK.API.Instance.WebViews.CreateOffscreenView(new WebViewSettings { UserAgent = epicUserAgent }))
            {
                while (true)
                {
                    var apiUrl = string.Format(apiTemplate, nextPageToken);
                    webView.NavigateAndWait(apiUrl);
                    var pageSource = webView.GetPageSource();
                    var json = webView.GetPageText();
                    if (json.IsNullOrEmpty())
                    {
                        break;
                    }

                    if (!Serialization.TryFromJson<EpicGetOrderHistoryResponse>(json, out var response))
                    {
                        break;
                    }

                    if (!response.Orders.HasItems())
                    {
                        break;
                    }

                    foreach (var order in response.Orders)
                    {
                        var orderCreation = order.CreatedAtMillis.ToString();
                        var secondsToAdd = long.Parse(orderCreation.Remove(orderCreation.Length - 3));
                        var utcDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(secondsToAdd);
                        var localDateTime = utcDateTimeOffset.LocalDateTime;
                        foreach (var item in order.Items)
                        {
                            licensesList.Add(new LicenseData(item.Description.HtmlDecode(), localDateTime));
                        }
                    }

                    if (response.NextPageToken.IsNullOrEmpty())
                    {
                        break;
                    }

                    nextPageToken = response.NextPageToken;
                }
            }

            return licensesList;
        }
    }
}