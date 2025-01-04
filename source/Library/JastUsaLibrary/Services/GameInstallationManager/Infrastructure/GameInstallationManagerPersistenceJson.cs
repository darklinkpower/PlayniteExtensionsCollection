using GenericEntityJsonRepository;
using JastUsaLibrary.Services.GameInstallationManager.Application;
using JastUsaLibrary.Services.GameInstallationManager.Entities;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.GameInstallationManager.Infrastructure
{
    public class GameInstallationManagerPersistenceJson : IGameInstallationManagerPersistence
    {
        private readonly IEntityRepository<GameInstallCache, Guid> _repository;
        public GameInstallationManagerPersistenceJson(string storagePath, ILogger logger)
        {
            _repository = new EntityJsonRepository<GameInstallCache, Guid>(logger, storagePath, "gameInstallations");
        }

        public bool ClearPersistedData()
        {
            return _repository.ClearPersistedData();
        }

        public List<GameInstallCache> LoadPersistedData()
        {
            return _repository.LoadPersistedData();
        }

        public void PersistData(IEnumerable<GameInstallCache> entities)
        {
            _repository.PersistData(entities);
        }

        public void PersistData(GameInstallCache entity)
        {
            _repository.PersistData(entity);
        }

        public GameInstallCache GetById(Guid id)
        {
            return _repository.GetById(id);
        }

        public void RemoveById(Guid id)
        {
            _repository.RemoveById(id);
        }
    }
}