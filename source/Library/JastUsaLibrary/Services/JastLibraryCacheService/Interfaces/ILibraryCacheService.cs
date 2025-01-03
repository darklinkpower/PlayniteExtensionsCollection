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

namespace JastUsaLibrary.JastLibraryCacheService.Interfaces
{
    public interface ILibraryCacheService
    {
        bool ApplyProgramToGameCache(Game databaseGame, Program program);
        bool ApplyProgramToGameCache(GameCache gameCache, Program program);
        bool ApplyProgramToGameCache(Game databaseGame, GameCache gameCache, Program program);
        List<JastGameWrapper> LibraryGames { get; }
        GameCache GetCacheById(string gameId);
        void SaveCache(GameCache gameCache);
        void RemoveCacheById(string id);
        List<GameCache> GetAllCache();
    }
}