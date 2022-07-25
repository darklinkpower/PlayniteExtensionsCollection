using Playnite.SDK;
using SteamWishlistDiscountNotifier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier
{
    // TODO Refactor and move everything data-obtain related here
    public class SteamLogin
    {
        public static void GetLoggedInSteamId64(IWebView webView, out AuthStatus status, out string steamId)
        {
            webView.NavigateAndWait(@"https://steamcommunity.com/login/home/?goto=https://steamcommunity.com/my/recommended/");
            var address = webView.GetCurrentAddress();
            if (address.IsNullOrEmpty())
            {
                status = AuthStatus.NoConnection;
                steamId = null;
                return;
            }
            else if (address.StartsWith(@"https://steamcommunity.com/id/") ||
                address.StartsWith(@"https://steamcommunity.com/profiles/"))
            {
                var source = webView.GetPageSource();
                var idMatch = Regex.Match(source, @"g_steamID = ""(\d+)""");
                if (idMatch.Success)
                {
                    status = AuthStatus.Ok;
                    steamId = idMatch.Groups[1].Value;
                    return;
                }
                else
                {
                    idMatch = Regex.Match(source, @"steamid"":""(\d+)""");
                    if (idMatch.Success)
                    {
                        status = AuthStatus.Ok;
                        steamId = idMatch.Groups[1].Value;
                        return;
                    }
                }
            }

            status = AuthStatus.AuthRequired;
            steamId = null;

            return;
        }
    }
}