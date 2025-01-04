using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.ProgramsHelper.Models;
using JastUsaLibrary.Services.JastLibraryCacheService.Entities;
using JastUsaLibrary.Services.JastUsaIntegration.Domain.Entities;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastLibraryCacheService.Application
{
    public interface ILibraryCacheService
    {
        List<JastGameWrapper> GetLibraryGames();
        GameCache GetCacheById(int gameId);
        void SaveCache(GameCache gameCache);
        void RemoveCacheById(int id);
        List<GameCache> GetAllCache();
    }
}