using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastLibraryCacheService.Interfaces
{
    public interface ILibraryCachePersistence
    {
        GameCache GetCacheById(string gameId);
        bool SaveCache(GameCache cache);
        void RemoveCacheById(string id);
        List<GameCache> GetAllCache();
    }
}