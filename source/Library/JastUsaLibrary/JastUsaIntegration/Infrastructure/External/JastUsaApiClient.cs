using FlowHttp;
using FlowHttp.Constants;
using JastUsaLibrary.JastUsaIntegration.Application.DTOs;
using JastUsaLibrary.JastUsaIntegration.Domain.Entities;
using JastUsaLibrary.JastUsaIntegration.Domain.Exceptions;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Infrastructure.External
{
    public class JastUsaApiClient
    {
        private readonly ILogger _logger = LogManager.GetLogger();

        public JastUsaApiClient()
        {

        }

        public AuthenticationToken GetAuthenticationToken(AuthenticationTokenRequest authenticationRequest, CancellationToken cancellationToken = default)
        {
            var headers = new Dictionary<string, string>
            {
                ["Accept"] = "application/json",
                ["Accept-Encoding"] = "utf-8"
            };

            var request = HttpRequestFactory.GetHttpRequest()
                .WithUrl(JastUrls.Api.Authentication.AuthenticationToken)
                .WithPostHttpMethod()
                .WithContent(Serialization.ToJson(authenticationRequest), HttpContentTypes.Json)
                .WithHeaders(headers);

            var downloadStringResult = request.DownloadString(cancellationToken);
            if (downloadStringResult.IsSuccess)
            {
                return Serialization.FromJson<AuthenticationToken>(downloadStringResult.Content);
            }

            if (downloadStringResult.IsCancelled)
            {
                return null;
            }

            _logger.Error(downloadStringResult.Error, "Failed to retrieve authentication token");
            if (downloadStringResult.HttpStatusCode is HttpStatusCode.Unauthorized)
            {
                throw new InvalidLoginCredentialsException(authenticationRequest);
                //_playniteApi.Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOC_JUL_DialogMessageAuthenticateIncorrectCredentials"), "JAST USA Library");
            }
            else
            {         
                //_playniteApi.Dialogs.ShowErrorMessage(string.Format(ResourceProvider.GetString("LOC_JUL_DialogMessageAuthenticateError"), downloadStringResult.Error?.Message), "JAST USA Library");
            }

            throw new AuthenticationErrorException(authenticationRequest);
        }

        public async Task<GameTranslationsResponse> GetGameTranslationsAsync(AuthenticationToken token, int translationId, CancellationToken cancellationToken = default)
        {
            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token.Token.UrlDecode()}",
                ["Accept-Encoding"] = "utf-8"
            };

            var translationsUrl = string.Format(@"https://app.jastusa.com/api/v2/shop/account/game-translations/{0}", translationId);
            var request = HttpRequestFactory.GetHttpRequest()
                .WithUrl(translationsUrl)
                .WithHeaders(headers);

            var result = await request.DownloadStringAsync(cancellationToken);
            return result.IsSuccess ? Serialization.FromJson<GameTranslationsResponse>(result.Content) : null;
        }

        public async Task<List<JastProduct>> GetProductsAsync(AuthenticationToken token, CancellationToken cancellationToken = default)
        {
            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token.Token.UrlDecode()}",
                ["Accept-Encoding"] = "utf-8"
            };

            var products = new List<JastProduct>();
            var page = 1;
            List<JastProduct> pageProducts;
            do
            {
                var url = string.Format(JastUrls.Api.Account.GetGamesTemplate, page);
                var request = HttpRequestFactory.GetHttpRequest()
                    .WithUrl(url)
                    .WithHeaders(headers);

                var result = await request.DownloadStringAsync(cancellationToken);
                if (!result.IsSuccess)
                {
                    _logger.Error(result.Error, "Failed to retrieve games");
                    return new List<JastProduct>();
                }

                var response = Serialization.FromJson<UserGamesResponse>(result.Content);
                products.AddRange(response.Products);
                pageProducts = response.Products;
                page++;
            }
            while (pageProducts.Count > 0);

            return products;
        }

        public async Task<Uri> GenerateDownloadLinkAsync(AuthenticationToken token, GenerateLinkRequest requestBody, CancellationToken cancellationToken = default)
        {
            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token.Token.UrlDecode()}",
                ["Accept"] = "application/json",
                ["Accept-Encoding"] = "utf-8"
            };

            var request = HttpRequestFactory.GetHttpRequest()
                .WithUrl(JastUrls.Api.Account.GenerateLink)
                .WithPostHttpMethod()
                .WithContent(Serialization.ToJson(requestBody), HttpContentTypes.Json)
                .WithHeaders(headers);

            var result = await request.DownloadStringAsync(cancellationToken);
            if (result.IsSuccess)
            {
                return Serialization.FromJson<GenerateLinkResponse>(result.Content).Url;
            }

            _logger.Error(result.Error, "Failed to generate download link");
            //if (!result.IsCancelled)
            //{
            //    _playniteApi.Dialogs.ShowErrorMessage(string.Format(ResourceProvider.GetString("LOC_JUL_DialogMessageGenerateLinkError"), downloadStringResult.Error?.Message), "JAST USA Library");
            //    _logger.Warn(downloadStringResult.Error, $"Error while obtaining download link with params gameId {gameLink.GameId} and gameLinkId {gameLink.GameLinkId}");
            //}

            return null;
        }
    }
}
