using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VNDBNexus
{
    public static class VndbUtilities
    {
        private static Guid steamPluginId = Guid.Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab");
        private static readonly Regex vndbGameIdRegex = new Regex(@"^https:\/\/vndb\.org\/(v\d+)$", RegexOptions.None);

        public static string GetVndbIdFromLinks(Game game)
        {
            if (game.Links is null)
            {
                return null;
            }

            foreach (var gameLink in game.Links)
            {
                if (string.IsNullOrEmpty(gameLink.Url))
                {
                    continue;
                }

                var linkMatch = vndbGameIdRegex.Match(gameLink.Url);
                if (linkMatch.Success)
                {
                    return linkMatch.Groups[1].Value;
                }
            }

            return null;
        }
    }
}
