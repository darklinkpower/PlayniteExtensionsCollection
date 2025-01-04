using GenericEntityJsonRepository;
using JastUsaLibrary.JastLibraryCacheService.Application;
using JastUsaLibrary.Services.JastLibraryCacheService.Entities;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.JastLibraryCacheService.Infrastructure
{
    public class LibraryCachePersistenceJson : ILibraryCachePersistence
    {
        private readonly IEntityRepository<GameCache, int> _repository;
        public LibraryCachePersistenceJson(string storagePath, ILogger logger)
        {
            _repository = new EntityJsonRepository<GameCache, int>(logger, storagePath, "jastUsaLibraryCache");
        }

        public bool ClearPersistedData()
        {
            return _repository.ClearPersistedData();
        }

        public List<GameCache> LoadPersistedData()
        {
            return _repository.LoadPersistedData();
        }

        public void PersistData(IEnumerable<GameCache> entities)
        {
            _repository.PersistData(entities);
        }

        public void PersistData(GameCache entity)
        {
            _repository.PersistData(entity);
        }

        public GameCache GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void RemoveById(int id)
        {
            _repository.RemoveById(id);
        }
    }


}