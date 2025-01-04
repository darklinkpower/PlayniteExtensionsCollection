using JastUsaLibrary.Services.GameInstallationManager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.GameInstallationManager.Application
{
    public interface IGameInstallationManagerPersistence
    {
        void PersistData(IEnumerable<GameInstallCache> entities);
        void PersistData(GameInstallCache entity);
        List<GameInstallCache> LoadPersistedData();
        bool ClearPersistedData();
        GameInstallCache GetById(Guid id);
        void RemoveById(Guid id);
    }
}