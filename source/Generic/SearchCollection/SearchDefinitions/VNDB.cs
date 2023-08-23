using Playnite.SDK.Models;
using SearchCollection.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SearchCollection.SearchDefinitions
{
    public class VNDB : BaseSearchDefinition
    {
        public override string Name => "VNDB";
        public override string Icon => "Vndb.png";

        protected override string UrlFormat => @"https://vndb.org/v/all?q={0}";
        private static string UrlIdFormat => @"https://vndb.org/v{0}";
        public override string GetSearchUrl(Game game)
        {
            var vndbId = GetVndbIdFromLinks(game);
            if (!vndbId.IsNullOrEmpty())
            {
                return string.Format(UrlIdFormat, vndbId);
            }

            return GetSearchUrl(game.Name);
        }

        private static string GetVndbIdFromLinks(Game game)
        {
            if (!game.Links.HasItems())
            {
                return null;
            }

            foreach (var gameLink in game.Links)
            {
                if (gameLink.Url.IsNullOrEmpty())
                {
                    continue;
                }

                var match = Regex.Match(gameLink.Url, @"^https?:\/\/vndb.org\/v(\d+)");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return null;
        }
    }
}