using PurchaseDateImporter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PurchaseDateImporter.Services
{
    public static class SteamLicenseService
    {
        public static Guid PluginId = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");
        public const string LibraryName = "Steam";

        public static Dictionary<string, LicenseData> GetLicensesDict()
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

        public static List<LicenseData> GetSteamLicenses()
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

        private static string GetLicensesPageContent()
        {
            using (var webView = Playnite.SDK.API.Instance.WebViews.CreateOffscreenView())
            {
                webView.NavigateAndWait("https://store.steampowered.com/account/licenses/?l=english");
                return webView.GetPageSource();
            }
        }

        private static List<string> GetEndStringsToRemove()
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
    }
}