using JastUsaLibrary.JastUsaIntegration.Application.DTOs;
using JastUsaLibrary.JastUsaIntegration.Application.Interfaces;
using JastUsaLibrary.JastUsaIntegration.Domain.Entities;
using JastUsaLibrary.JastUsaIntegration.Domain.Enums;
using JastUsaLibrary.JastUsaIntegration.Infrastructure.External;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Application.Services
{
    public class JastUsaAccountClient : IAccountClient
    {
        private readonly ILogger _logger = LogManager.GetLogger();
        private readonly IPlayniteAPI _playniteApi;
        private readonly JastUsaApiClient _apiClient;
        private readonly IAuthenticationPersistence _authenticationPersistence;

        public JastUsaAccountClient(
            IPlayniteAPI playniteApi,
            JastUsaApiClient apiClient,
            IAuthenticationPersistence authenticationPersistence)
        {
            _playniteApi = playniteApi;
            _apiClient = apiClient;
            _authenticationPersistence = authenticationPersistence;
        }

        public bool GetIsUserLoggedIn()
        {
            return _authenticationPersistence.LoadAuthentication() != null;
        }

        public bool Login(string email, string password, bool rememberMe)
        {
            _authenticationPersistence.DeleteAuthentication();
            var authRequest = new AuthenticationTokenRequest(email, password, rememberMe);
            var token = _apiClient.GetAuthenticationToken(authRequest);
            if (token != null)
            {
                return _authenticationPersistence.SaveAuthentication(authRequest);
            }

            _playniteApi.Dialogs.ShowErrorMessage("Login failed. Please check your credentials.", "JAST USA Library");
            return false;
        }

        public AuthenticationToken GetAuthenticationToken(CancellationToken cancellationToken = default)
        {
            var authRequest = _authenticationPersistence.LoadAuthentication();
            return authRequest != null ? _apiClient.GetAuthenticationToken(authRequest, cancellationToken) : null;
        }

        public async Task<List<JastProduct>> GetGamesAsync(CancellationToken cancellationToken = default)
        {
            var token = GetAuthenticationToken(cancellationToken);
            return await _apiClient.GetProductsAsync(token, cancellationToken);
        }

        public async Task<GameTranslationsResponse> GetGameTranslationsAsync(UserGamesResponseTranslation userGamesResponseTranslation, CancellationToken cancellationToken = default)
        {
            var token = GetAuthenticationToken(cancellationToken);
            var response = await _apiClient.GetGameTranslationsAsync(token, userGamesResponseTranslation.Id, cancellationToken);

            // We remove all the assets that are not for Windows because Playnite only supports windows after all
            void FilterNonWindowsLinks(List<GameLink> links)
            {
                links.RemoveAll(item => !item.Platforms.Contains(JastPlatform.Windows));
            }

            FilterNonWindowsLinks(response.GamePathLinks);
            FilterNonWindowsLinks(response.GamePatchLinks);
            FilterNonWindowsLinks(response.GameExtraLinks);

            return response;
        }

        public async Task<Uri> GetAssetDownloadLinkAsync(GameLink gameLink, CancellationToken cancellationToken = default)
        {
            var token = GetAuthenticationToken(cancellationToken);
            var requestBody = new GenerateLinkRequest(gameLink);
            return await _apiClient.GenerateDownloadLinkAsync(token, requestBody, cancellationToken);
        }
    }
}