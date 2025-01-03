using JastUsaLibrary.Services.JastUsaIntegration.Domain.Entities;
using JastUsaLibrary.Services.JastUsaIntegration.Domain.ValueObjects;
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
        bool Login(string email, string password, bool rememberMe);
        AuthenticationToken GetAuthenticationToken(CancellationToken cancellationToken = default);
        Task<List<JastGameData>> GetGamesAsync(CancellationToken cancellationToken = default);
        Task<Uri> GetAssetDownloadLinkAsync(JastGameDownloadData downloadData, CancellationToken cancellationToken = default);
        Task<JastGameDownloads> GetGameTranslationsAsync(int id, CancellationToken cancellationToken = default);
    }
}