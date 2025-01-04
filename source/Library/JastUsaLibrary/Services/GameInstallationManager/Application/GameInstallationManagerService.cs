using JastUsaLibrary.ProgramsHelper.Models;
using JastUsaLibrary.Services.GameInstallationManager.Entities;
using JastUsaLibrary.Services.JastLibraryCacheService.Entities;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.GameInstallationManager.Application
{
    public class GameInstallationManagerService : IGameInstallationManagerService
    {
        private readonly IPlayniteAPI _playniteApi;
        private readonly IGameInstallationManagerPersistence _gameInstallationManagerPersistence;

        public GameInstallationManagerService(
            IPlayniteAPI playniteApi,
            IGameInstallationManagerPersistence gameInstallationManagerPersistence)
        {
            _playniteApi = Guard.Against.Null(playniteApi);
            _gameInstallationManagerPersistence = Guard.Against.Null(gameInstallationManagerPersistence);
        }

        public bool ApplyProgramToGameCache(Game game, Program program)
        {
            var gameCache = _gameInstallationManagerPersistence.GetById(game.Id) ?? new GameInstallCache(game.Id);
            gameCache.Program = program;
            _gameInstallationManagerPersistence.PersistData(gameCache);
            if (gameCache.Program != null)
            {
                game.InstallDirectory = Path.GetDirectoryName(program.Path);
                game.IsInstalled = true;
                _playniteApi.Database.Games.Update(game);
            }
            else
            {
                game.InstallDirectory = string.Empty;
                game.IsInstalled = false;
                _playniteApi.Database.Games.Update(game);
            }

            return false;
        }

        public List<GameInstallCache> GetAllCache()
        {
            return _gameInstallationManagerPersistence.LoadPersistedData();
        }

        public GameInstallCache GetCacheById(Guid id)
        {
            return _gameInstallationManagerPersistence.GetById(id);
        }

        public void RemoveCacheById(Guid id)
        {
            _gameInstallationManagerPersistence.RemoveById(id);
        }

        public void SaveCache(GameInstallCache gameCache)
        {
            _gameInstallationManagerPersistence.PersistData(gameCache);
        }
    }
}
