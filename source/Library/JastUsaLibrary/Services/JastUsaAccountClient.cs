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
        private const string baseApiUrl = @"https://app.jastusa.com/api/";
        private const string getGamesUrlTemplate = @"https://app.jastusa.com/api/v2/shop/account/user-games-dev?localeCode=en_US&phrase=&page={0}&itemsPerPage=1000";
        private const string tokenRefreshUrl = @"https://app.jastusa.com/api/v2/shop/authentication-refresh?refresh_token={0}";
        private const string generateLinkUrl = @"https://app.jastusa.com/api/v2/shop/account/user-games/generate-link";

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
                client.Headers.Clear();
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

                    // We can't reliably close the window on login because the user token
                    // cookie is not set inmediatamente after loggin in. It gets set after a small time

                    //webView.LoadingChanged += async (s, e) =>
                    //{
                    //    var address = webView.GetCurrentAddress();
                    //    var source = await webView.GetPageSourceAsync();
                    //    if (source.Contains(@"<div class=""account-mobile__logout"">") || source.Contains(@"<div class=""sidebar-mobile"" logged-in=""true"">"))
                    //    {
                    //        isLoggedIn = true;
                    //        webView.Close();
                    //    }
                    //};

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

        public GameTranslationsResponse GetGameTranslations(int translationId)
        {
            var tokens = LoadTokens();
            if (tokens == null)
            {
                return null;
            }

            client.Headers.Clear();
            client.Headers.Add(@"Authorization", "Bearer " + tokens.Token.UrlDecode());
            var translationsUrl = string.Format(@"https://app.jastusa.com/api/v2/shop/account/game-translations/{0}", translationId);
            var responseString = client.DownloadString(translationsUrl);
            if (responseString.IsNullOrEmpty())
            {
                return null;
            }

            var response = Serialization.FromJson<GameTranslationsResponse>(responseString);
            foreach (var gameLinkItem in response.GamePathLinks.HydraMember.ToList())
            {
                if (!gameLinkItem.Platforms.Any(x => x == "windows"))
                {
                    response.GamePathLinks.HydraMember.Remove(gameLinkItem);
                }
            }

            foreach (var gameLinkItem in response.GamePatchLinks.HydraMember.ToList())
            {
                if (!gameLinkItem.Platforms.Any(x => x == "windows"))
                {
                    response.GamePathLinks.HydraMember.Remove(gameLinkItem);
                }
            }

            foreach (var gameLinkItem in response.GameExtraLinks.HydraMember.ToList())
            {
                if (!gameLinkItem.Platforms.Any(x => x == "windows"))
                {
                    response.GamePathLinks.HydraMember.Remove(gameLinkItem);
                }
            }

            return response;
        }

        internal List<JastProduct> GetGames()
        {
            var products = new List<JastProduct>();
            var tokens = LoadTokens();
            if (tokens == null)
            {
                return products;
            }

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

        internal string GetAssetDownloadLinkAsync(int gameId, int gameLinkId)
        {
            var tokens = LoadTokens();
            if (tokens == null)
            {
                return null;
            }

            client.Headers.Clear();
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            client.Headers.Add(@"Authorization", "Bearer " + tokens.Token.UrlDecode());

            var requestParams = Serialization.ToJson(new GenerateLinkRequest { downloaded = false, gameId = gameId, gameLinkId = gameLinkId });

            try
            {
                var response = client.UploadString(new Uri(generateLinkUrl), "POST", requestParams);
                return Serialization.FromJson<GenerateLinkResponse>(response).Url;
            }
            catch (Exception e)
            {
                playniteApi.Dialogs.ShowErrorMessage($"Error while obtaining downlink link with params gameId {gameId} and gameLinkId {gameLinkId}. Error: {e.Message}", "JAST USA Library");
                logger.Error(e, $"Error while obtaining downlink link with params gameId {gameId} and gameLinkId {gameLinkId}");
                return null;
            }
        }
    }
}