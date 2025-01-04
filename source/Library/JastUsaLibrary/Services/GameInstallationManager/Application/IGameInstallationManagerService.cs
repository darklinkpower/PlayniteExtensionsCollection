using System;
using System.Collections.Generic;
using JastUsaLibrary.ProgramsHelper.Models;
using JastUsaLibrary.Services.GameInstallationManager.Entities;
using Playnite.SDK.Models;

namespace JastUsaLibrary.Services.GameInstallationManager.Application
{
    public interface IGameInstallationManagerService
    {
        bool ApplyProgramToGameCache(Game game, Program program);
        GameInstallCache GetCacheById(Guid id);
        void SaveCache(GameInstallCache gameCache);
        void RemoveCacheById(Guid id);
        List<GameInstallCache> GetAllCache();
    }
}