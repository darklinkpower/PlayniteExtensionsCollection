using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using SearchCollection.BaseClasses;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchCollection.SearchDefinitions
{
    public class SteamSearch : BaseSearchDefinition
    {
        public override string Name => "Steam";
        public override string Icon => "Steam.png";

        protected override string UrlFormat => @"https://store.steampowered.com/search/?term={0}&ignore_preferences=1&category1=998";
        private static string UrlSteamFormat => @"https://store.steampowered.com/app/{0}";
        public override string GetSearchUrl(Game game)
        {
            if (game.Name.IsNullOrEmpty())
            {
                return null;
            }

            var steamId = Steam.GetGameSteamId(game, true);
            if (!steamId.IsNullOrEmpty())
            {
                return string.Format(UrlSteamFormat, steamId);
            }
            else if (PlayniteUtilities.IsGamePcGame(game))
            {
                return string.Format(UrlFormat, game.Name.UrlEncode());
            }

            return null;
        }
    }
}