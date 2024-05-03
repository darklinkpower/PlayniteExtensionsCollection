using JastUsaLibrary.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using WebCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Net.Http;
using WebCommon.Constants;

namespace JastUsaLibrary.Services
{
    public class JastUsaAccountClient
    {
        private ILogger logger = LogManager.GetLogger();
        private IPlayniteAPI playniteApi;
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
            var headers = new Dictionary<string, string>
            {
                ["Accept"] = "application/json",
                ["Accept-Encoding"] = "utf-8"
            };

            var request = HttpBuilderFactory.GetStringClientBuilder().WithUrl(authenticationTokenUrl)
                .WithPostHttpMethod()
                .WithContent(Serialization.ToJson(authentication), HttpContentTypes.Json)
                .WithHeaders(headers)
                .Build();

            var downloadStringResult = request.DownloadString();
            if (downloadStringResult.IsSuccess)
            {
                return Serialization.FromJson<AuthenticationToken>(downloadStringResult.Content);
            }
            else
            {
                if (showErrors)
                {
                    if (downloadStringResult.HttpStatusCode is HttpStatusCode.Unauthorized)
                    {
                        playniteApi.Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageAuthenticateIncorrectCredentials"), "JAST USA Library");
                    }
                    else
                    {
                        playniteApi.Dialogs.ShowErrorMessage(string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageAuthenticateError"), downloadStringResult.Error?.Message), "JAST USA Library");
                    }
                }

                logger.Error(downloadStringResult.Error, $"Failed during GetAuthenticationToken. Status: {downloadStringResult.HttpStatusCode}");
                return null;
            }
        }

        public GameTranslationsResponse GetGameTranslations(AuthenticationToken authenticationToken, int translationId)
        {
            if (authenticationToken == null)
            {
                return null;
            }

            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {authenticationToken.Token.UrlDecode()}",
                ["Accept-Encoding"] = "utf-8"
            };
            
            var translationsUrl = string.Format(@"https://app.jastusa.com/api/v2/shop/account/game-translations/{0}", translationId);
            var request = HttpBuilderFactory.GetStringClientBuilder()
                .WithUrl(translationsUrl)
                .WithHeaders(headers)
                .Build();
            var downloadStringResult = request.DownloadString();
            if (!downloadStringResult.IsSuccess)
            {
                return null;
            }

            var response = Serialization.FromJson<GameTranslationsResponse>(downloadStringResult.Content);
            // We remove all the assets that are not for Windows because Playnite only supports windows after all
            foreach (var gameLinkItem in response.GamePathLinks.ToList())
            {
                if (!gameLinkItem.Platforms.Any(x => x == JastPlatform.Windows))
                {
                    response.GamePathLinks.Remove(gameLinkItem);
                }
            }

            foreach (var gameLinkItem in response.GamePatchLinks.ToList())
            {
                if (!gameLinkItem.Platforms.Any(x => x == JastPlatform.Windows))
                {
                    response.GamePathLinks.Remove(gameLinkItem);
                }
            }

            foreach (var gameLinkItem in response.GameExtraLinks.ToList())
            {
                if (!gameLinkItem.Platforms.Any(x => x == JastPlatform.Windows))
                {
                    response.GamePathLinks.Remove(gameLinkItem);
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

            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = "Bearer " + authenticationToken.Token.UrlDecode(),
                ["Accept-Encoding"] = "utf-8"
            };

            var currentPage = 0;
            while (true)
            {
                currentPage++;
                var url = string.Format(getGamesUrlTemplate, currentPage);
                var request = HttpBuilderFactory.GetStringClientBuilder()
                    .WithUrl(url)
                    .WithHeaders(headers)
                    .Build();
                var downloadStringResult = request.DownloadString();
                if (!downloadStringResult.IsSuccess)
                {
                    return null;
                }

                var response = Serialization.FromJson<UserGamesResponse>(downloadStringResult.Content);
                foreach (var product in response.Products)
                {
                    products.Add(product);
                }

                logger.Debug($"GetGames current page: {currentPage}, total pages: {response.Pages}");
                if (response.Pages == currentPage)
                {
                    break;
                }
            }

            return products;
        }

        internal Uri GetAssetDownloadLinkAsync(int gameId, int gameLinkId)
        {
            var tokens = GetAuthenticationToken();
            if (tokens == null)
            {
                playniteApi.Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageCouldNotObtainAuthTokens"), "JAST USA Library");
                return null;
            }

            var headers = new Dictionary<string, string>
            {
                ["Accept"] = "application/json",
                ["Accept-Encoding"] = "utf-8",
                ["Authorization"] = "Bearer " + tokens.Token.UrlDecode()
            };

            var jsonPostContent = Serialization.ToJson(new GenerateLinkRequest { downloaded = true, gameId = gameId, gameLinkId = gameLinkId });
            var request = HttpBuilderFactory.GetStringClientBuilder()
                .WithUrl(generateLinkUrl)
                .WithPostHttpMethod()
                .WithContent(jsonPostContent, HttpContentTypes.Json)
                .WithHeaders(headers)
                .Build();
            var downloadStringResult = request.DownloadString();
            if (downloadStringResult.IsSuccess)
            {
                return Serialization.FromJson<GenerateLinkResponse>(downloadStringResult.Content).Url;
            }
            else
            {
                playniteApi.Dialogs.ShowErrorMessage(string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageGenerateLinkError"), downloadStringResult.Error?.Message), "JAST USA Library");
                logger.Warn(downloadStringResult.Error, $"Error while obtaining downlink link with params gameId {gameId} and gameLinkId {gameLinkId}");
            }

            return null;
        }
    }
}