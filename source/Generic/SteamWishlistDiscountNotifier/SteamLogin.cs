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
            webView.NavigateAndWait(@"https://store.steampowered.com/login/?redir=account%2F&redir_ssl=1");
            var address = webView.GetCurrentAddress();
            if (address.IsNullOrEmpty())
            {
                status = AuthStatus.NoConnection;
                steamId = null;
                return;
            }
            else if (address.StartsWith(@"https://store.steampowered.com/account/"))
            {
                var source = webView.GetPageSource();
                var idMatch = Regex.Match(source, @"<div class=""youraccount_steamid"">[^\d]+(\d+)");
                if (idMatch.Success)
                {
                    status = AuthStatus.Ok;
                    steamId = idMatch.Groups[1].Value;
                    return;
                }
            }

            status = AuthStatus.AuthRequired;
            steamId = null;

            return;
        }
    }
}