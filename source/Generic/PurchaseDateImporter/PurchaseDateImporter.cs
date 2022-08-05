using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon.Web;
using PurchaseDateImporter.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Xml;

namespace PurchaseDateImporter
{
    public class PurchaseDateImporter : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static Guid steamPluginId = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");

        private PurchaseDateImporterSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("eb9abc51-93a4-4db6-b2c9-159eb531b0f2");

        public PurchaseDateImporter(IPlayniteAPI api) : base(api)
        {
            settings = new PurchaseDateImporterSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new PurchaseDateImporterSettingsView();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = "Test",
                    MenuSection = "@Purchase Date Importer",
                    Action = a => {
                        GetOriginLicenses();
                    }
                }
            };
        }
        private void ApplyPurchaseDataToSteam()
        {
            var steamLicenses = GetSteamLicensesDict();
            if (!steamLicenses.HasItems())
            {
                return;
            }

            var updated = 0;
            PlayniteApi.Database.BufferedUpdate();
            foreach (var game in PlayniteApi.Database.Games)
            {
                if (game.PluginId != steamPluginId)
                {
                    continue;
                }

                var matchingName = game.Name.GetMatchModifiedName();
                if (steamLicenses.TryGetValue(matchingName, out var licenseData))
                {
                    if (IsDateDifferent(game.Added, licenseData.PurchaseDate))
                    {
                        game.Added = licenseData.PurchaseDate;
                        PlayniteApi.Database.Games.Update(game);
                        updated++;
                    }
                }
            }

            PlayniteApi.Dialogs.ShowMessage($"Changed {updated}");
        }

        private bool IsDateDifferent(DateTime? added, DateTime purchaseDate)
        {
            if (added == null)
            {
                return true;
            }

            // We only change the Added date if the date is different,
            // to not override the hour of the day, which could be more accurate
            if (added.Value.Day != purchaseDate.Day)
            {
                return true;
            }
            else if (added.Value.Month != purchaseDate.Month)
            {
                return true;
            }
            else if (added.Value.Year != purchaseDate.Year)
            {
                return true;
            }

            return false;
        }

        private Dictionary<string, LicenseData> GetSteamLicensesDict()
        {
            var licensesDictionary = new Dictionary<string, LicenseData>();
            var licenses = GetSteamLicenses();
            var endStringsToRemove = GetEndStringsToRemove();
            foreach (var license in licenses)
            {
                var licenseNameMatch = license.Name;
                foreach (var suffixString in endStringsToRemove)
                {
                    if (licenseNameMatch.EndsWith(suffixString))
                    {
                        licenseNameMatch = licenseNameMatch.Remove(licenseNameMatch.Length - suffixString.Length);
                    }
                }

                licenseNameMatch = Regex.Replace(licenseNameMatch.Replace(" and ", "")
                    .Replace("Game of the Year", "GOTY"), "(GOTY)$", "GOTY Edition");
                licensesDictionary[licenseNameMatch.GetMatchModifiedName()] = license;
            }

            return licensesDictionary;
        }

        private List<LicenseData> GetSteamLicenses()
        {
            var licensesList = new List<LicenseData>();
            var licensesRegex = @"(?:<td class=""license_date_col"">)(.*?(?=<\/td>))(?:<\/td>\s+<td>)(?:\s+<div class=""free_license_remove_link"">(?:[\s\S]*?(?=<\/div>))<\/div>)?(?:\s+)([^\t]+)";
            var licensePageSource = GetLicensesPageContent();
            if (licensePageSource.IsNullOrEmpty())
            {
                return licensesList;
            }

            var licenseMatches = Regex.Matches(licensePageSource, licensesRegex);
            if (licenseMatches.Count == 0)
            {
                return licensesList;
            }

            foreach (Match licenseMatch in licenseMatches)
            {
                licensesList.Add(new LicenseData(licenseMatch.Groups[2].Value.HtmlDecode(), DateTime.Parse(licenseMatch.Groups[1].Value)));
            }

            return licensesList;
        }

        private Dictionary<string, LicenseData> GetEpicLicensesDict()
        {
            var licensesDictionary = new Dictionary<string, LicenseData>();
            var licenses = GetEpicLicenses();
            foreach (var license in licenses)
            {
                licensesDictionary[license.Name.GetMatchModifiedName()] = license;
            }

            return licensesDictionary;
        }

        private const string epicUserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 Vivaldi/4.3";
        private List<LicenseData> GetEpicLicenses()
        {
            var licensesList = new List<LicenseData>();
            var apiTemplate = "https://www.epicgames.com/account/v2/payment/ajaxGetOrderHistory?page={0}&lastCreatedAt={1}";

            var createdAtValue = DateTime.Now.ToString("u").Replace(" ", "T");
            using (var webView = PlayniteApi.WebViews.CreateOffscreenView(new WebViewSettings {UserAgent = epicUserAgent }))
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
                            licensesList.Add(new LicenseData(item.Description.HtmlDecode(), transactionDate));
                        }
                    }
                }
            }

            return licensesList;
        }

        private string GetLicensesPageContent()
        {
            using (var webView = PlayniteApi.WebViews.CreateOffscreenView())
            {
                webView.NavigateAndWait("https://store.steampowered.com/account/licenses/?l=english");
                return webView.GetPageSource();
            }
        }

        private List<string> GetEndStringsToRemove()
        {
            // Create prefix strings to remove
            var endStringsToRemove = new List<string>
            {
                // Regions
                " (Latam)",
                " (LATAM/IN)",
                " (LATAM/RU/CN/IN/TR)",
                " Latam",
                " (US)",
                " (US/AU)",
                " (NA)",
                " (NA+ROW)",
                " (ROW Launch)",
                " (ROW)",
                " (Rest of World)",
                " ROW Release",
                " ROW",
                " (RU)",
                " RU",
                " (South America)",
                " SA",
                " (WW)",
                " WW Digital Distribution",
                " (Key-only WW)",
                " WW",

                // Release type
                " Collection Retail",
                " (Retail)",
                " Retail Key",
                " Retail Rtd",
                " - The Full Package Retail",
                " (Digital Retail)",
                " [DIGITAL RETAIL]",
                " - [Digital]",
                " [Digital]",


                " (preorder)",
                " (Pre-Order)",
                " (pre-purchase)",
                " Pre-Purchase",
                " (Post-Launch)",
                " Post-Launch",
                " CD key",
                " Retail",
                " Digital",
                // Free
                " - Free Giveaway",
                " - Free for 24 Hours",
                " - Free For A Limited Time!",
                " (Free)",
                " Free Giveaway",
                " Free",

                // Editions
                " Deluxe Edition",
                " Complete Edition",
                " Standard Edition",
                " Voiced Edition",
                " - Digital Edition of Light",
                "  Digital Edition",
                " Digital Distribution",
                " Day One Edition",
                " Enhanced Edition",
                " - Legacy Edition",
                " - Starter Pack",
                " - Starter Edition",
                " Special Edition",
                " Standard",
                " Launch",
                " Paper's Cut Edition",
                " - War Chest Edition",
                " - Special Steam Edition",
                ": Assassins of Kings Enhanced Edition",
                " - Beta",
                " for Beta Testing",

                // Other
                " (Rebellion Store)",
                " PROMO",
                " Gift Copy - Hades Purchase",
                " - Gift",
                " Gift",
                " Steam Store and Retail Key",
                " (100% off week)",
                " - Complimentary (Opt In)",
                " - Holiday Pack",
                " Bundle (Summer 2012)",
                ": REVENGEANCE",
                " Care Package",
                " Complete Season (Episodes 1-5)",
                " Deluxe - Includes OST and an exclusive Artbook",
                " Free On Demand",
                " 4-Pack",
                " 2 Pack",
                " Free Edition - Français + Italiano + 한국어 [Learn French + Italian + Korean]",
                ": 20 Year Celebration",
                ": Complete Story",
                " Free Access"
            };

            var months = new List<string>
            {
                "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Aug", "Sep", "Oct", "Nov", "Dec"
            };

            var years = new List<string>
            {
                "2016", "2017", "2018", "2019", "2020", "2021, 2022", "2023", "2024"
            };

            foreach (var year in years)
            {
                foreach (var month in months)
                {
                    var newRegexString = $" Limited Free Promotional Package - {month} {year}";
                    endStringsToRemove.Add(newRegexString);
                }
            }

            return endStringsToRemove;
        }

        // Use Id
        private List<LicenseData> GetGogLicenses()
        {
            var licensesList = new List<LicenseData>();
            var apiTemplate = "https://www.gog.com/account/settings/orders/data?canceled=0&completed=1&in_progress=1&not_redeemed=1&page={0}&pending=1&redeemed=1";

            using (var webView = PlayniteApi.WebViews.CreateOffscreenView())
            {
                for (int i = 0; true; i++)
                {
                    var apiUrl = string.Format(apiTemplate, i);
                    webView.NavigateAndWait(apiUrl);
                    var pageSource = webView.GetPageSource();
                    var json = PlayniteUtilities.GetEmbeddedJsonFromWebViewSource(webView.GetPageSource());
                    if (json.IsNullOrEmpty())
                    {
                        break;
                    }

                    var response = Serialization.FromJson<GogOrderResponse>(json);
                    if (response.Orders.Count == 0)
                    {
                        break;
                    }

                    foreach (var order in response.Orders)
                    {
                        var transactionDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(order.Date);
                        foreach (var product in order.Products)
                        {
                            licensesList.Add(new LicenseData(product.Title, transactionDate, product.Id));
                        }
                    }
                }
            }

            return licensesList;
        }

        private List<LicenseData> GetOriginLicenses()
        {
            var licensesList = new List<LicenseData>();
            var authResponse = GetEaAuthResponse();
            if (authResponse == null)
            {
                return licensesList;
            }

            var entitlementsResponse = GetEaEntitlementsResponse(authResponse);
            if (entitlementsResponse == null)
            {
                return licensesList;
            }

            foreach (var entitlement in entitlementsResponse.Entitlements)
            {
                var offerType = entitlement.OfferType.Replace(" ", "").ToLower();
                if (offerType == "basegame" || offerType == "demo")
                {
                    licensesList.Add(new LicenseData(entitlement.OfferId, entitlement.GrantDate, entitlement.OfferId));
                }
            }

            return licensesList;
        }

        private EaAuthResponse GetEaAuthResponse()
        {
            using (var webView = PlayniteApi.WebViews.CreateOffscreenView())
            {
                var authResponseUrl = @"https://accounts.ea.com/connect/auth?client_id=ORIGIN_JS_SDK&response_type=token&redirect_uri=nucleus:rest&prompt=none";
                webView.NavigateAndWait(authResponseUrl);

                var authJson = PlayniteUtilities.GetEmbeddedJsonFromWebViewSource(webView.GetPageSource());
                if (authJson.StartsWith(@"{""error_code"""))
                {
                    // User is not logged in
                    return null;
                }

                if (authJson.IsNullOrEmpty())
                {
                    return null;
                }

                return Serialization.FromJson<EaAuthResponse>(authJson);
            }
        }

        private EaEntitlementsResponse GetEaEntitlementsResponse(EaAuthResponse authResponse)
        {
            using (var webClient = new WebClient { Encoding = Encoding.UTF8 })
            {
                //webClient.Headers.Add(HttpRequestHeader.Accept, "application/xml");
                webClient.Headers.Add("Authorization", $"{authResponse.TokenType} {authResponse.AccessToken}");
                webClient.Headers.Add("accept", "application/vnd.origin.v3+json; x-cache/force-write");

                var identityResponse = webClient.DownloadString("https://gateway.ea.com/proxy/identity/pids/me");
                if (identityResponse.IsNullOrEmpty())
                {
                    return null;
                }

                var identity = Serialization.FromJson<EaIdentityResponse>(identityResponse);

                // For some reason somtimes the response is in XML format when the Headers contain the
                // Authorization header
                webClient.Headers.Clear();
                webClient.Headers.Add("accept", "application/vnd.origin.v3+json; x-cache/force-write");
                webClient.Headers.Add("authtoken", authResponse.AccessToken);
                var url = string.Format("https://api1.origin.com/ecommerce2/consolidatedentitlements/{0}?machine_hash=1", identity.Pid.PidId);
                var entitlementsResponseData = webClient.DownloadString(url);
                return Serialization.FromJson<EaEntitlementsResponse>(entitlementsResponseData);
            }
        }


    }
}