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
    public class SteamGridDB : BaseSearchDefinition
    {
        public override string Name => "SteamGridDB";
        public override string Icon => "SteamGridDB.png";

        protected override string UrlFormat => @"https://www.steamgriddb.com/search/grids?term={0}";
        private static string UrlSteamFormat => @"https://www.steamgriddb.com/steam/{0}";
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