using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
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
        private static readonly Regex steamLinkRegex = new Regex(@"^https?:\/\/store\.steampowered\.com\/app\/(\d+)", RegexOptions.Compiled);

        public static string GetGameSteamId(Game game, bool useLinksDetection = false)
        {
            if (IsGameSteamGame(game))
            {
                logger.Debug("Steam id found for Steam game by pluginId");
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

            foreach (Link gameLink in game.Links)
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
    }
}
