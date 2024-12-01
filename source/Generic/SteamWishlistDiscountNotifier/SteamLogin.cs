using Playnite.SDK;
using SteamWishlistDiscountNotifier.Enums;
using SteamWishlistDiscountNotifier.Models;
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
        private static readonly ILogger logger = LogManager.GetLogger();

        public static SteamAccountInfo GetLoggedInSteamId64(IWebView webView)
        {
            webView.NavigateAndWait(@"https://store.steampowered.com/account/?l=english");
            string username = null;
            string steamId = null;
            AuthStatus authStatus;
            string walletString = null;

            var address = webView.GetCurrentAddress();
            if (address.IsNullOrEmpty())
            {
                authStatus = AuthStatus.NoConnection;
            }
            else if (address.StartsWith(@"https://store.steampowered.com/account/"))
            {
                var source = webView.GetPageSource();
                authStatus = AuthStatus.Ok;

                // Username
                var regeMatch = Regex.Match(source, @"<span class=""account_name"">(.+)<\/span>");
                if (regeMatch.Success)
                {
                    username = regeMatch.Groups[1].Value;
                }

                // SteamId
                regeMatch = Regex.Match(source, @"<div class=""youraccount_steamid"">[^\d]+(\d+)");
                if (regeMatch.Success)
                {
                    steamId = regeMatch.Groups[1].Value;
                }

                // Wallet
                regeMatch = Regex.Match(source, @"<a href=""https:\/\/store\.steampowered\.com\/account\/history\/"">(.+)<\/a>");
                if (regeMatch.Success)
                {
                    walletString = regeMatch.Groups[1].Value;
                }
            }
            else
            {
                logger.Debug($"Steam GetLoggedInSteamId64 not logged. Address: {address}");
                authStatus = AuthStatus.AuthRequired;
            }

            return new SteamAccountInfo(username, steamId, authStatus, walletString); ;
        }
    }
}