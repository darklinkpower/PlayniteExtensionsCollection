using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericEntityJsonRepository
{
    public class EntityJsonRepository<TEntity, TId> : IEntityRepository<TEntity, TId> where TEntity : IEntity<TId>
    {
        private readonly ILogger _logger;
        private readonly string _jsonFilePath;
        private readonly Dictionary<TId, TEntity> _cache;

        public EntityJsonRepository(ILogger logger, string storageDirectory, string storageFilename)
        {
            _logger = logger;
            _jsonFilePath = Path.Combine(storageDirectory, $"{storageFilename}.json");
            _cache = new Dictionary<TId, TEntity>();

            if (FileSystem.FileExists(_jsonFilePath))
            {
                var entities = LoadPersistedData();
                foreach (var entity in entities)
                {
                    _cache[entity.Id] = entity;
                }
            }

            if (!FileSystem.DirectoryExists(storageDirectory))
            {
                FileSystem.CreateDirectory(storageDirectory);
            }
        }

        public void PersistData(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                _cache[entity.Id] = entity;
            }

            SaveData();
        }

        public void PersistData(TEntity entity)
        {
            _cache[entity.Id] = entity;
            SaveData();
        }

        public List<TEntity> LoadPersistedData()
        {
            if (FileSystem.FileExists(_jsonFilePath))
            {
                try
                {
                    return Serialization.FromJsonFile<List<TEntity>>(_jsonFilePath);
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Error while deserializing {_jsonFilePath}");
                    FileSystem.DeleteFileSafe(_jsonFilePath);
                }
            }

            return new List<TEntity>();
        }

        public bool ClearPersistedData()
        {
            _cache.Clear();
            SaveData();
            return true;
        }

        private void SaveData()
        {
            try
            {
                var serializedData = Serialization.ToJson(_cache.Values.ToList());
                FileSystem.WriteStringToFile(_jsonFilePath, serializedData, true);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error while storing serialized data to {_jsonFilePath}");
                throw;
            }
        }

        public TEntity GetById(TId id)
        {
            _cache.TryGetValue(id, out var entity);
            return entity;
        }

        public bool RemoveById(TId id)
        {
            if (_cache.Remove(id))
            {
                SaveData();
                return true;
            }

            return false;
        }
    }


}
