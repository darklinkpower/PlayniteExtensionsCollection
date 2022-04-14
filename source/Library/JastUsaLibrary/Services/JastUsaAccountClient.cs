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
        private readonly string authenticationPath;
        private const string loginUrl = @"https://jastusa.com/my-account";
        private const string jastDomain = @"jastusa.com";
        private const string baseApiUrl = @"https://app.jastusa.com/api/";
        private const string getGamesUrlTemplate = @"https://app.jastusa.com/api/v2/shop/account/user-games-dev?localeCode=en_US&phrase=&page={0}&itemsPerPage=1000";
        private const string tokenRefreshUrl = @"https://app.jastusa.com/api/v2/shop/authentication-refresh?refresh_token={0}";
        private const string generateLinkUrl = @"https://app.jastusa.com/api/v2/shop/account/user-games/generate-link";
        private const string authenticationTokenUrl = @"https://app.jastusa.com/api/v2/shop/authentication-token";

        public JastUsaAccountClient(IPlayniteAPI api, string authenticationPath)
        {
            playniteApi = api;
            client = new WebClient {Encoding = Encoding.UTF8};
            this.authenticationPath = authenticationPath;
        }

        public bool GetIsUserLoggedIn()
        {
            return GetAuthenticationToken() != null;
        }

        private AuthenticationTokenRequest LoadAuthentication()
        {
            if (!FileSystem.FileExists(authenticationPath))
            {
                return null;
            }

            try
            {
                return Serialization.FromJson<AuthenticationTokenRequest>(
                    Encryption.DecryptFromFile(
                        authenticationPath,
                        Encoding.UTF8,
                        WindowsIdentity.GetCurrent().User.Value));
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to load saved authentication.");
                FileSystem.DeleteFileSafe(authenticationPath);
            }

            return null;
        }

        private bool SaveAuthentication(AuthenticationTokenRequest authentication)
        {
            try
            {
                var serializedJson = Serialization.ToJson(authentication);
                Encryption.EncryptToFile(
                    authenticationPath,
                    serializedJson,
                    Encoding.UTF8,
                    WindowsIdentity.GetCurrent().User.Value);
                logger.Debug("Tokens stored");
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to store Authentication response");
                FileSystem.DeleteFileSafe(authenticationPath);
                return false;
            }
        }
        public bool Login(string loginEmail, string loginPassword)
        {
            FileSystem.DeleteFile(authenticationPath);

            var authentication = new AuthenticationTokenRequest(loginEmail, loginPassword);
            if (GetAuthenticationToken(authentication, true) != null)
            {
                return SaveAuthentication(authentication);
            }

            return false;
        }

        public AuthenticationToken GetAuthenticationToken()
        {
            var authentication = LoadAuthentication();
            if (authentication != null)
            {
                return GetAuthenticationToken(authentication);
            }

            return null;
        }

        public AuthenticationToken GetAuthenticationToken(AuthenticationTokenRequest authentication, bool showErrors = false)
        {
            try
            {
                client.Headers.Clear();
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                var requestParams = Serialization.ToJson(authentication);
                var response = client.UploadString(new Uri(authenticationTokenUrl), "POST", requestParams);
                return Serialization.FromJson<AuthenticationToken>(response);
            }
            catch (WebException e)
            {
                if (showErrors)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        playniteApi.Dialogs.ShowErrorMessage($"Authentication credentials are incorrect", "JAST USA Library");
                    }
                    else
                    {
                        playniteApi.Dialogs.ShowErrorMessage($"Error during login. Error: {e.Message}", "JAST USA Library");
                    }
                }

                logger.Error(e, $"Failed during GetAuthenticationToken. WebException status: {e.Status}");
                return null;
            }
            catch (Exception e)
            {
                if (showErrors)
                {
                    playniteApi.Dialogs.ShowErrorMessage($"Failed during login. Error: {e.Message}", "JAST USA Library");
                }

                logger.Error(e, $"Failed during GetAuthenticationToken");
                return null;
            }
        }

        public GameTranslationsResponse GetGameTranslations(AuthenticationToken authenticationToken, int translationId)
        {
            if (authenticationToken == null)
            {
                return null;
            }

            client.Headers.Clear();
            client.Headers.Add(@"Authorization", "Bearer " + authenticationToken.Token.UrlDecode());
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

        internal List<JastProduct> GetGames(AuthenticationToken authenticationToken)
        {
            var products = new List<JastProduct>();
            if (authenticationToken == null)
            {
                return products;
            }

            client.Headers.Clear();
            client.Headers.Add(@"Authorization", "Bearer " + authenticationToken.Token.UrlDecode());
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
            var tokens = GetAuthenticationToken();
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
            catch (WebException e)
            {
                playniteApi.Dialogs.ShowErrorMessage($"Error while obtaining downlink link with params gameId {gameId} and gameLinkId {gameLinkId}. Error: {e.Message}", "JAST USA Library");
                logger.Error(e, $"Error while obtaining downlink link with params gameId {gameId} and gameLinkId {gameLinkId}. WebException status: {e.Status}");
                return null;
            }
        }
    }
}