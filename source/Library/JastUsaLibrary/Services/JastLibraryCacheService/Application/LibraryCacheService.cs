using JastUsaLibrary.DownloadManager.Domain.Entities;
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

        public LibraryCacheService(IPlayniteAPI playniteApi, ILibraryCachePersistence libraryCachePersistence, Guid pluginId)
        {
            _playniteApi = playniteApi;
            _libraryCachePersistence = libraryCachePersistence;
            _pluginId = pluginId;
        }

        public GameCache GetCacheById(int gameId)
        {
            var cache = _libraryCachePersistence.GetById(gameId);
            if (cache != null)
            {
                return cache;
            }

            return null;
        }

        public List<JastGameWrapper> GetLibraryGames()
        {
            return _playniteApi.Database.Games
                .Where(g => g.PluginId == _pluginId)
                .OrderBy(g => g.Name)
                .Select(game =>
                {
                    var cache = _libraryCachePersistence.GetById(Convert.ToInt32(game.GameId));
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
            _libraryCachePersistence.PersistData(gameCache);
        }

        public void RemoveCacheById(int id)
        {
            _libraryCachePersistence.RemoveById(id);
        }

        public List<GameCache> GetAllCache()
        {
            return _libraryCachePersistence.LoadPersistedData();
        }
    }
}