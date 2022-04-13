using JastUsaLibrary.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using PluginsCommon.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace JastUsaLibrary.Services
{
    public class JastUsaAccountClient
    {
        private ILogger logger = LogManager.GetLogger();
        private IPlayniteAPI playniteApi;
        private readonly WebClient client;
        private readonly string tokensPath;
        private const string loginUrl = @"https://jastusa.com/my-account";
        private const string jastDomain = @"jastusa.com";
        private const string getGamesUrlTemplate = @"https://app.jastusa.com/api/v2/shop/account/user-games-dev?localeCode=en_US&phrase=&page={0}&itemsPerPage=1000";
        private const string tokenRefreshUrl = @"https://app.jastusa.com/api/v2/shop/authentication-refresh?refresh_token={0}";

        public JastUsaAccountClient(IPlayniteAPI api, string tokensPath)
        {
            playniteApi = api;
            client = new WebClient {Encoding = Encoding.UTF8};
            this.tokensPath = tokensPath;
        }

        public bool GetIsUserLoggedIn()
        {
            //using (var webView = playniteApi.WebViews.CreateOffscreenView())
            //{
            //    webView.NavigateAndWait(loginUrl);
            //    //var cookie = webView.GetCookies().FirstOrDefault(x => x.Domain == jastDomain && x.Name == @"auth._token.local");
            //    //if (cookie == null && cookie.Value == "false")
            //    //{
            //    //    webView.Close();
            //    //    return GetAndStoreTokens(cookie.Value);
            //    //}

            //    // Try to refresh token


            //    webView.Close();
            //}

            return RefreshTokens();
        }

        private bool RefreshTokens()
        {
            var tokens = LoadTokens();
            if (tokens != null)
            {
                var refreshSuccess = GetAndStoreTokensResponse(tokens.RefreshToken);
                if (refreshSuccess)
                {
                    return true;
                }
            }

            using (var webView = playniteApi.WebViews.CreateOffscreenView())
            {
                var refreshCookie = webView.GetCookies().FirstOrDefault(x => x.Domain == jastDomain && x.Name == @"refreshToken");
                if (refreshCookie != null && refreshCookie.Value != "false")
                {
                    webView.Close();
                    return GetAndStoreTokensResponse(refreshCookie.Value);
                }

                var refreshCookiess = webView.GetCookies().Where(x => x.Domain == jastDomain);
                webView.Close();
                return false;
            }
        }


        private bool GetAndStoreTokensResponse(string refreshTokenValue)
        {
            try
            {
                var url = string.Format(tokenRefreshUrl, refreshTokenValue);
                var responseString = client.DownloadString(url);
                var tokensResponse = Serialization.FromJson<AuthenticationRefreshResponse>(responseString);
                Encryption.EncryptToFile(
                    tokensPath,
                    responseString,
                    Encoding.UTF8,
                    WindowsIdentity.GetCurrent().User.Value);
                logger.Debug("Tokens stored");
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to get tokens response");
                FileSystem.DeleteFile(tokensPath);
                return false;
            }
        }

        private AuthenticationRefreshResponse LoadTokens()
        {
            if (FileSystem.FileExists(tokensPath))
            {
                try
                {
                    return Serialization.FromJson<AuthenticationRefreshResponse>(
                        Encryption.DecryptFromFile(
                            tokensPath,
                            Encoding.UTF8,
                            WindowsIdentity.GetCurrent().User.Value));
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to load saved tokens.");
                }
            }

            return null;
        }

        public bool Login()
        {
            FileSystem.DeleteFile(tokensPath);
            var isLoggedIn = false;
            try
            {
                using (var webView = playniteApi.WebViews.CreateView(1024, 800, Colors.Black))
                {
                    webView.LoadingChanged += async (s, e) =>
                    {
                        var address = webView.GetCurrentAddress();
                        var source = await webView.GetPageSourceAsync();
                        if (source.Contains(@"<div class=""account-mobile__logout"">") || source.Contains(@"<div class=""sidebar-mobile"" logged-in=""true"">"))
                        {
                            isLoggedIn = true;
                            //webView.Close();
                        }
                    };

                    webView.DeleteDomainCookies(jastDomain);
                    webView.Navigate(loginUrl);
                    webView.OpenDialog();
                }
            }
            catch (Exception e)
            {
                playniteApi.Dialogs.ShowErrorMessage("Failed to authenticate user.", "");
                logger.Error(e, "Failed to authenticate user.");
            }

            return RefreshTokens();
        }

        internal List<JastProduct> GetGames()
        {
            var products = new List<JastProduct>();
            var tokens = LoadTokens();
            if (tokens == null)
            {
                return products;
            }

            //using (var webView = playniteApi.WebViews.CreateOffscreenView())
            //{
            //    var cookie = webView.GetCookies().FirstOrDefault(x => x.Domain == jastDomain && x.Name == @"auth._token.local");
            //    if (cookie != null && cookie.Value != "false")
            //    {
            //        bearerToken = cookie.Value.UrlDecode();
            //    }
            //}

            //if (bearerToken.IsNullOrEmpty())
            //{
            //    logger.Error("Couldn't get bearer token from cookie");
            //    return products;
            //}

            client.Headers.Clear();
            client.Headers.Add(@"Authorization", "Bearer " + tokens.Token.UrlDecode());
            var currentPage = 0;
            while (true)
            {
                currentPage++;
                var url = string.Format(getGamesUrlTemplate, currentPage);
                var responseString = client.DownloadString(url);
                var response = Serialization.FromJson<UserGamesResponse>(responseString);

                foreach (var product in response.Products.JastProducts)
                {
                    products.Add(product);
                }

                if (response.Pages == currentPage)
                {
                    break;
                }

                break;
            }

            client.Headers.Clear();
            return products;
        }
    }
}