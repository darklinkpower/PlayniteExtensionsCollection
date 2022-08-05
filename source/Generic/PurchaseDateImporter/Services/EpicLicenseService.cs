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
        private const string epicUserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 Vivaldi/4.3";

        public static Dictionary<string, LicenseData> GetLicensesDict()
        {
            var licensesDictionary = new Dictionary<string, LicenseData>();
            var licenses = GetLicenses();
            foreach (var license in licenses)
            {
                licensesDictionary[license.Name.GetMatchModifiedName()] = license;
            }

            return licensesDictionary;
        }

        public static List<LicenseData> GetLicenses()
        {
            var licensesList = new List<LicenseData>();
            var apiTemplate = "https://www.epicgames.com/account/v2/payment/ajaxGetOrderHistory?page={0}&lastCreatedAt={1}";

            var createdAtValue = DateTime.Now.ToString("u").Replace(" ", "T");
            using (var webView = Playnite.SDK.API.Instance.WebViews.CreateOffscreenView(new WebViewSettings { UserAgent = epicUserAgent }))
            {
                for (int i = 0; true; i++)
                {
                    var apiUrl = string.Format(apiTemplate, i, createdAtValue);
                    webView.NavigateAndWait(apiUrl);
                    var pageSource = webView.GetPageSource();
                    var json = PlayniteUtilities.GetEmbeddedJsonFromWebViewSource(webView.GetPageSource());
                    if (json.IsNullOrEmpty())
                    {
                        break;
                    }

                    var response = Serialization.FromJson<EpicGetOrderHistoryResponse>(json);
                    if (response.Orders.Count == 0)
                    {
                        break;
                    }

                    foreach (var order in response.Orders)
                    {
                        var orderCreation = order.CreatedAtMillis.ToString();
                        var secondsToAdd = long.Parse(orderCreation.Remove(orderCreation.Length - 3));
                        var transactionDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(secondsToAdd);
                        createdAtValue = transactionDate.ToString("u").Replace(" ", "T");

                        foreach (var item in order.Items)
                        {
                            licensesList.Add(new LicenseData(item.Description.HtmlDecode(), transactionDate.ToLocalTime()));
                        }
                    }
                }
            }

            return licensesList;
        }
    }
}