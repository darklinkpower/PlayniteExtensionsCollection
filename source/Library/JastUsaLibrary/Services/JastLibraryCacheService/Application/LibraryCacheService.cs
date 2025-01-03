using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.JastLibraryCacheService.Interfaces;
using JastUsaLibrary.ProgramsHelper.Models;
using JastUsaLibrary.Services.JastLibraryCacheService.Entities;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastLibraryCacheService.Application
{
    public class LibraryCacheService : ILibraryCacheService
    {
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILibraryCachePersistence _libraryCachePersistence;
        private readonly Guid _pluginId;
        public List<JastGameWrapper> LibraryGames => GetLibraryGames();

        public LibraryCacheService(IPlayniteAPI playniteApi, ILibraryCachePersistence libraryCachePersistence, Guid pluginId)
        {
            _playniteApi = playniteApi;
            _libraryCachePersistence = libraryCachePersistence;
            _pluginId = pluginId;
        }

        public bool ApplyProgramToGameCache(Game databaseGame, Program program)
        {
            var gameCache = _libraryCachePersistence.GetCacheById(databaseGame.GameId);
            if (gameCache != null)
            {
                return ApplyProgramToGameCache(gameCache, program);
            }

            return false;
        }

        public bool ApplyProgramToGameCache(GameCache gameCache, Program program)
        {
            var databaseGame = _playniteApi.Database.Games.FirstOrDefault(g => g.PluginId == _pluginId && g.GameId == gameCache.GameId);
            if (databaseGame is null)
            {
                return false;
            }

            ApplyProgramToGameCache(databaseGame, gameCache, program);
            return true;
        }

        public bool ApplyProgramToGameCache(Game databaseGame, GameCache gameCache, Program program)
        {
            gameCache.UpdateProgram(program);
            _libraryCachePersistence.SaveCache(gameCache);

            databaseGame.InstallDirectory = Path.GetDirectoryName(program.Path);
            databaseGame.IsInstalled = true;
            _playniteApi.Database.Games.Update(databaseGame);
            return true;
        }

        public GameCache GetCacheById(string gameId)
        {
            var cache = _libraryCachePersistence.GetCacheById(gameId);
            if (cache != null)
            {
                return cache;
            }

            return null;
        }

        private List<JastGameWrapper> GetLibraryGames()
        {
            return _playniteApi.Database.Games
                .Where(g => g.PluginId == _pluginId)
                .OrderBy(g => g.Name)
                .Select(game =>
                {
                    var cache = _libraryCachePersistence.GetCacheById(game.GameId);
                    if (cache != null)
                    {
                        return new JastGameWrapper(game, cache);
                    }
                    else
                    {
                        return new JastGameWrapper(game, null);
                    }
                }).ToList();
        }

        public void SaveCache(GameCache gameCache)
        {
            _libraryCachePersistence.SaveCache(gameCache);
        }

        public void RemoveCacheById(string id)
        {
            _libraryCachePersistence.RemoveCacheById(id);
        }

        public List<GameCache> GetAllCache()
        {
            return _libraryCachePersistence.GetAllCache();
        }
    }
}