using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.ProgramsHelper.Models;
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
        bool ApplyAssetsToCache(string gameId, IEnumerable<JastAssetWrapper> assetWrappers);
        List<JastGameWrapper> LibraryGames { get; }
        GameCache GetCacheById(string gameId);
    }
}