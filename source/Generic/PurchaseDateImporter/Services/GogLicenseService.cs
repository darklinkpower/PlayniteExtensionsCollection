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
    public static class GogLicenseService
    {
        public static Guid PluginId = Guid.Parse("aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e");
        public const string LibraryName = "GOG";
        public const string LoginUrl = @"https://www.gog.com/";

        public static Dictionary<string, LicenseData> GetLicensesDict()
        {
            var licensesDictionary = new Dictionary<string, LicenseData>();
            var licenses = GetLicenses();
            foreach (var license in licenses)
            {
                licensesDictionary[license.Id] = license;
            }

            return licensesDictionary;
        }

        public static List<LicenseData> GetLicenses()
        {
            var licensesList = new List<LicenseData>();
            var apiTemplate = "https://www.gog.com/account/settings/orders/data?canceled=0&completed=1&in_progress=1&not_redeemed=1&page={0}&pending=1&redeemed=1";

            using (var webView = Playnite.SDK.API.Instance.WebViews.CreateOffscreenView())
            {
                for (int i = 0; true; i++)
                {
                    var apiUrl = string.Format(apiTemplate, i);
                    webView.NavigateAndWait(apiUrl);
                    var pageText = webView.GetPageText();
                    if (pageText.IsNullOrEmpty())
                    {
                        break;
                    }

                    var response = Serialization.FromJson<GogOrderResponse>(pageText);
                    if (response.Orders.Count == 0)
                    {
                        break;
                    }

                    foreach (var order in response.Orders)
                    {
                        var utcDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(order.Date);
                        var localDateTime = utcDateTimeOffset.LocalDateTime; 
                        foreach (var product in order.Products)
                        {
                            licensesList.Add(new LicenseData(product.Title, localDateTime, product.Id));
                        }
                    }
                }
            }

            return licensesList;
        }
    }
}