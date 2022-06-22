using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SteamCommon
{
    public static class Steam
    {
        private static ILogger logger = LogManager.GetLogger();
        private static Guid steamPluginId = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");
        private static readonly Regex steamLinkRegex = new Regex(@"^https?:\/\/store\.steampowered\.com\/app\/(\d+)", RegexOptions.None);

        public static string GetGameSteamId(Game game, bool useLinksDetection = false)
        {
            if (IsGameSteamGame(game))
            {
                return game.GameId;
            }
            else if (useLinksDetection)
            {
                return GetSteamIdFromLinks(game);
            }

            return null;
        }

        private static string GetSteamIdFromLinks(Game game)
        {
            if (game.Links == null)
            {
                return null;
            }

            foreach (var gameLink in game.Links)
            {
                var linkMatch = steamLinkRegex.Match(gameLink.Url);
                if (linkMatch.Success)
                {
                    var id = linkMatch.Groups[1].Value;
                    logger.Debug($"Steam id {id} for {game.Name} found via link.");
                    return id;
                }
            }

            return null;
        }

        public static bool IsGameSteamGame(Game game)
        {
            return game.PluginId == steamPluginId;
        }

        public static string GetSteamApiMatchingLanguage(string playniteLanguage)
        {
            // https://partner.steamgames.com/doc/store/localization
            switch (playniteLanguage)
            {
                case "en_US":
                    return "english";
                case "es_ES":
                    return "spanish";
                case "ar_SA":
                    return "ar";
                case "ca_ES":
                    return "spanish";
                case "cs_CZ":
                    return "cs";
                case "de_DE":
                    return "de";
                case "el_GR":
                    return "el";
                case "fa_IR":
                    return "english";
                case "fi_FI":
                    return "fi";
                case "fr_FR":
                    return "fr";
                case "he_IL":
                    return "english";
                case "hr_HR":
                    return "english";
                case "hu_HU":
                    return "hu";
                case "id_ID":
                    return "english";
                case "it_IT":
                    return "it";
                case "ja_JP":
                    return "ja";
                case "ko_KR":
                    return "ko";
                case "lt_LT":
                    return "english";
                case "nl_NL":
                    return "nl";
                case "no_NO":
                    return "no";
                case "pl_PL":
                    return "pl";
                case "pt_BR":
                    return "pt-BR";
                case "pt_PT":
                    return "pt";
                case "ro_RO":
                    return "ro";
                case "ru_RU":
                    return "ru";
                case "sk_SK":
                    return "english";
                case "sr_SP":
                    return "english";
                case "sv_SE":
                    return "sv";
                case "tr_TR":
                    return "tr";
                case "uk_UA":
                    return "english";
                case "zh_CN":
                    return "zh-CN";
                case "zh_TW":
                    return "zh-TW";
                default:
                    return "english";
            }
        }

        public static Dictionary<string, string> GetSteamCurrenciesDictionary()
        {
            return new Dictionary<string, string>
            {
                { "USD", "United States Dollar" },
                { "GBP", "United Kingdom Pound" },
                { "EUR", "European Union Euro" },
                { "CHF", "Swiss Francs" },
                { "RUB", "Russian Rouble" },
                { "PLN", "Polish Złoty" },
                { "BRL", "Brazilian Reals" },
                { "JPY", "Japanese Yen" },
                { "NOK", "Norwegian Krone" },
                { "IDR", "Indonesian Rupiah" },
                { "MYR", "Malaysian Ringgit" },
                { "PHP", "Philippine Peso" },
                { "SGD", "Singapore Dollar" },
                { "THB", "Thai Baht" },
                { "VND", "Vietnamese Dong" },
                { "KRW", "South Korean Won" },
                { "TRY", "Turkish Lira" },
                { "UAH", "Ukrainian Hryvnia" },
                { "MXN", "Mexican Peso" },
                { "CAD", "Canadian Dollars" },
                { "AUD", "Australian Dollars" },
                { "NZD", "New Zealand Dollar" },
                { "CNY", "Chinese Renminbi (yuan)" },
                { "INR", "Indian Rupee" },
                { "CLP", "Chilean Peso" },
                { "PEN", "Peruvian Sol" },
                { "COP", "Colombian Peso" },
                { "ZAR", "South African Rand" },
                { "HKD", "Hong Kong Dollar" },
                { "TWD", "New Taiwan Dollar" },
                { "SAR", "Saudi Riyal" },
                { "AED", "United Arab Emirates Dirham" },
                { "ARS", "Argentine Peso" },
                { "ILS", "Israeli New Shekel" },
                { "KZT", "Kazakhstani Tenge" },
                { "KWD", "Kuwaiti Dinar" },
                { "QAR", "Qatari Riyal" },
                { "CRC", "Costa Rican Colón" },
                { "UYU", "Uruguayan Peso" }
            };
        }

        public static Dictionary<string, string> GetCountryLocCurrencyDictionary()
        {
            var twoLetterCountryCodes = new List<string>
            {
                "US",
                "GB",
                "ES",
                "CH",
                "RU",
                "PL",
                "BR",
                "JP",
                "NO",
                "ID",
                "MY",
                "PH",
                "SG",
                "TH",
                "VN",
                "KR",
                "TR",
                "UA",
                "MX",
                "CA",
                "AU",
                "NZ",
                "CN",
                "IN",
                "CL",
                "PE",
                "CO",
                "ZA",
                "HK",
                "TW",
                "SA",
                "AE",
                "AR",
                "IL",
                "KZ",
                "KW",
                "QA",
                "CR",
                "UY"
            };

            var apiDictionary = new Dictionary<string, string>();
            foreach (var countryCode in twoLetterCountryCodes)
            {
                var regionInfo = new RegionInfo(countryCode);
                apiDictionary[countryCode] = $"{regionInfo.CurrencyNativeName} ({regionInfo.ISOCurrencySymbol})";
            }

            return apiDictionary;
        }
    }
}