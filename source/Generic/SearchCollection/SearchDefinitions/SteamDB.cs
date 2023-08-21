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
    public class SteamDB : BaseSearchDefinition
    {
        public override string Name => "SteamDB";
        public override string Icon => "SteamDb.png";

        protected override string UrlFormat => @"https://steamdb.info/search/?a=app&q={0}&&type=1&category=0";
        private static string UrlSteamFormat => @"https://steamdb.info/app/{0}";
        public override string GetSearchUrl(Game game)
        {
            if (game.Name.IsNullOrEmpty())
            {
                return null;
            }

            if (!PlayniteUtilities.IsGamePcGame(game))
            {
                return null;
            }

            var steamId = Steam.GetGameSteamId(game, true);
            if (!steamId.IsNullOrEmpty())
            {
                return string.Format(UrlFormat, steamId);
            }
            else
            {
                return string.Format(UrlSteamFormat, game.Name.UrlEncode());
            }
        }
    }
}
