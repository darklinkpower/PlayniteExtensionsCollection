using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier
{
    public class SteamLogin
    {
        public static string GetLoggedInSteamId64(IWebView webView)
        {
            webView.NavigateAndWait(@"https://steamcommunity.com/my/recommended/");
            var source = webView.GetPageSource();
            var idMatch = Regex.Match(source, @"g_steamID = ""(\d+)""");
            if (idMatch.Success)
            {
                return idMatch.Groups[1].Value;
            }
            else
            {
                idMatch = Regex.Match(source, @"steamid"":""(\d+)""");
                if (idMatch.Success)
                {
                    return idMatch.Groups[1].Value;
                }
            }

            return null;
        }
    }
}
