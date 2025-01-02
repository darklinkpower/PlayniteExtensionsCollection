using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.JastLibraryCacheService.Interfaces;
using JastUsaLibrary.ProgramsHelper.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            var cache = _libraryCachePersistence.GetCacheById(databaseGame.GameId);
            if (cache != null)
            {
                cache.Program = program;
                _libraryCachePersistence.SaveCache(cache);

                databaseGame.InstallDirectory = Path.GetDirectoryName(program.Path);
                databaseGame.IsInstalled = true;
                _playniteApi.Database.Games.Update(databaseGame);
                return true;
            }

            return false;
        }

        public bool ApplyAssetsToCache(string gameId, IEnumerable<JastAssetWrapper> assetWrappers)
        {
            var cache = _libraryCachePersistence.GetCacheById(gameId);
            if (cache != null)
            {
                cache.Assets = assetWrappers.ToObservable();
                _libraryCachePersistence.SaveCache(cache);
            }

            return false;
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
                        return new JastGameWrapper(game, cache.Assets);
                    }
                    else
                    {
                        return new JastGameWrapper(game, new ObservableCollection<JastAssetWrapper>());
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