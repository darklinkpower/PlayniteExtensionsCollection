using Playnite.SDK;
using SteamWishlistDiscountNotifier.Domain.Enums;
using SteamWishlistDiscountNotifier.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Application.Steam.Login
{
    public class SteamLoginService
    {
        private const string _webViewUserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 Vivaldi/4.3";
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILogger _logger;

        public SteamLoginService(IPlayniteAPI playniteApi, ILogger logger)
        {
            _playniteApi = playniteApi;
            _logger = logger;
        }

        public SteamAccountInfo GetLoggedInStatus()
        {
            string username = null;
            string steamId = null;
            AuthStatus authStatus;
            string walletString = null;

            using (var webView = _playniteApi.WebViews.CreateOffscreenView())
            {
                webView.NavigateAndWait(@"https://store.steampowered.com/account/?l=english");
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
                    _logger.Debug($"Steam GetLoggedInSteamId64 not logged. Address: {address}");
                    authStatus = AuthStatus.AuthRequired;
                }
            }

            return new SteamAccountInfo(username, steamId, authStatus, walletString); ;
        }

        public void Logout()
        {
            using (var webView = _playniteApi.WebViews.CreateOffscreenView())
            {
                LogoutInternal(webView);
            }
        }

        public void LogoutInternal(IWebView webView)
        {
            var cookiesDomainsToDelete = new List<string>
            {
                "steamcommunity.com",
                "store.steampowered.com",
                "help.steampowered.com",
                "steampowered.com",
                "steam.tv",
                "checkout.steampowered.com",
                "login.steampowered.com"
            };

            foreach (var domain in cookiesDomainsToDelete)
            {
                webView.DeleteDomainCookies(domain);
                webView.DeleteDomainCookies($".{domain}"); //Cookies can also have a domain starting with a period
            }
        }

        public AuthStatus Login()
        {
            var status = AuthStatus.AuthRequired;
            try
            {
                var webViewSettings = new WebViewSettings { UserAgent = _webViewUserAgent, WindowWidth = 675, WindowHeight = 640 };
                using (var webView = _playniteApi.WebViews.CreateView(webViewSettings))
                {
                    webView.LoadingChanged += async (_, __) =>
                    {
                        var address = webView.GetCurrentAddress();
                        if (address.IsNullOrEmpty())
                        {
                            status = AuthStatus.NoConnection;
                            webView.Close();
                        }
                        else if (address.Contains(@"steampowered.com"))
                        {
                            var source = await webView.GetPageSourceAsync();
                            if (source == @"<html><head></head><body></body></html>")
                            {
                                status = AuthStatus.NoConnection;
                                webView.Close();
                            }

                            var idMatch = Regex.Match(source, @"<div class=""youraccount_steamid"">[^\d]+(\d+)");
                            if (idMatch.Success)
                            {
                                status = AuthStatus.Ok;
                                webView.Close();
                            }
                        }
                    };

                    LogoutInternal(webView);
                    webView.Navigate(@"https://store.steampowered.com/login/?redir=account%2F&redir_ssl=1");
                    webView.OpenDialog();
                }

            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to authenticate user.");
            }

            return status;
        }
    }
}