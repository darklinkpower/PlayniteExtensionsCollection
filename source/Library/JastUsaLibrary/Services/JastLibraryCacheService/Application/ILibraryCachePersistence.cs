using JastUsaLibrary.Services.JastLibraryCacheService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastLibraryCacheService.Application
{
    public interface ILibraryCachePersistence
    {
        void PersistData(IEnumerable<GameCache> entities);
        void PersistData(GameCache entity);
        List<GameCache> LoadPersistedData();
        bool ClearPersistedData();
        GameCache GetById(int gameId);
        void RemoveById(int id);
    }
}