using JastUsaLibrary.JastUsaIntegration.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Application.Interfaces
{
    public interface IAccountClient
    {
        bool GetIsUserLoggedIn();
        bool Login(string email, string password);
        AuthenticationToken GetAuthenticationToken(CancellationToken cancellationToken = default);
        Task<List<JastProduct>> GetGamesAsync(CancellationToken cancellationToken = default);
        Task<Uri> GetAssetDownloadLinkAsync(GameLink gameLink, CancellationToken cancellationToken = default);
        Task<GameTranslationsResponse> GetGameTranslationsAsync(UserGamesResponseTranslation userGamesResponseTranslation, CancellationToken cancellationToken = default);
    }
}