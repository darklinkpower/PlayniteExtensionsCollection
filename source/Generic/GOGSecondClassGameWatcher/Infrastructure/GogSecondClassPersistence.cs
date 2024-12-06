using GOGSecondClassGameWatcher.Domain.Interfaces;
using GOGSecondClassGameWatcher.Domain.ValueObjects;
using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
using ProtobufUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher.Infrastructure
{
    public class GogSecondClassPersistence : IGogSecondClassPersistence
    {
        private readonly ILogger _logger;
        private readonly string _storageFilePath;
        private GogSecondClassData _data;
        private bool _inTransaction = false;
        private int _transactionDepth = 0;
        private readonly object _lock = new object();
        private readonly object _transactionLock = new object();

        // In-memory caches for fast lookup by Title and Id
        private readonly Dictionary<string, GogSecondClassGame> _titleCache = new Dictionary<string, GogSecondClassGame>();
        private readonly Dictionary<string, GogSecondClassGame> _idCache = new Dictionary<string, GogSecondClassGame>();

        public GogSecondClassPersistence(string storageFileDirectory, ILogger logger)
        {
            _logger = logger;
            _storageFilePath = Path.Combine(storageFileDirectory, "GogSecondClassList.json");
            _data = LoadFromFile();
            BuildInMemoryCache();
        }

        private void BuildInMemoryCache()
        {
            _titleCache.Clear();
            _idCache.Clear();
            foreach (var item in _data.Items)
            {
                var titleKey = item.Title.Satinize();
                if (!_titleCache.ContainsKey(titleKey))
                {
                    _titleCache[titleKey] = item;
                }

                if (!item.Id.IsNullOrEmpty() && !_idCache.ContainsKey(item.Id))
                {
                    _idCache[item.Id] = item;
                }
            }
        }

        public void SaveItems(IEnumerable<GogSecondClassGame> items)
        {
            lock (_lock)
            {
                if (_inTransaction)
                {
                    foreach (var item in items)
                    {
                        AddOrUpdatePendingItem(item);
                    }
                }
                else
                {
                    foreach (var item in items)
                    {
                        AddOrUpdateItem(item);
                    }

                    SaveToFile();
                }
            }
        }

        public void RemoveItem(GogSecondClassGame item)
        {
            lock (_lock)
            {
                _data.Items.Remove(item);
                _titleCache.Remove(item.Title.Satinize());
                if (!item.Id.IsNullOrEmpty())
                {
                    _idCache.Remove(item.Id);
                }

                if (!_inTransaction)
                {
                    SaveToFile();
                }
            }
        }

        public void RemoveItems(IEnumerable<GogSecondClassGame> items)
        {
            lock (_lock)
            {
                foreach (var item in items)
                {
                    RemoveItem(item);
                }
            }
        }

        public void ClearItems()
        {
            lock (_lock)
            {
                _data.Items.Clear();
                _titleCache.Clear();
                _idCache.Clear();

                if (!_inTransaction)
                {
                    SaveToFile();
                }
            }
        }

        public GogSecondClassGame GetItemByTitle(string title)
        {
            lock (_lock)
            {
                _titleCache.TryGetValue(title.Satinize(), out var item);
                return item;
            }
        }

        public GogSecondClassGame GetItemById(string id)
        {
            lock (_lock)
            {
                _idCache.TryGetValue(id, out var item);
                return item;
            }
        }

        public List<GogSecondClassGame> GetAllItems()
        {
            lock (_lock)
            {
                return new List<GogSecondClassGame>(_data.Items);
            }
        }

        public DateTime? GetLastCheckTime()
        {
            lock (_lock)
            {
                return _data.LastCheckTime;
            }
        }

        public void SetLastCheckTime(DateTime time)
        {
            lock (_lock)
            {
                _data.LastCheckTime = time;
                if (!_inTransaction)
                {
                    SaveToFile();
                }
            }
        }

        public void BeginTransaction()
        {
            lock (_lock)
            {
                lock (_transactionLock)
                {
                    _transactionDepth++;
                }
                _inTransaction = true;
            }
        }

        public void CommitTransaction()
        {
            lock (_transactionLock)
            {
                _transactionDepth--;
            }

            if (_transactionDepth == 0)
            {
                SaveToFile();
                _inTransaction = false;
            }
        }

        public void RollbackTransaction()
        {
            lock (_transactionLock)
            {
                _transactionDepth--;
            }

            if (_transactionDepth == 0)
            {
                _data = LoadFromFile();
                BuildInMemoryCache();
                _inTransaction = false;
            }
        }

        public void ExecuteInTransaction(Action action)
        {
            BeginTransaction();
            try
            {
                action();
                CommitTransaction();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        private void AddOrUpdatePendingItem(GogSecondClassGame item)
        {
            AddOrUpdateItem(item);
        }

        private void AddOrUpdateItem(GogSecondClassGame item)
        {
            if (!item.Id.IsNullOrEmpty() && _idCache.TryGetValue(item.Id, out var gameById))
            {
                _data.Items.Remove(gameById);
            }

            if (_titleCache.TryGetValue(item.Title.Satinize(), out var gameByTitle))
            {
                _data.Items.Remove(gameByTitle);
            }

            _data.Items.Add(item);
            if (!item.Id.IsNullOrEmpty())
            {
                _idCache[item.Id] = item;
            }

            var nameKey = item.Title.Satinize();
            if (!nameKey.IsNullOrEmpty())
            {
                _titleCache[nameKey] = item;
            }
        }

        private void SaveToFile()
        {
            try
            {
                var serializedData = Serialization.ToJson(_data);
                FileSystem.WriteStringToFile(_storageFilePath, serializedData);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error saving file {_storageFilePath}");
                throw;
            }
        }

        private GogSecondClassData LoadFromFile()
        {
            if (FileSystem.FileExists(_storageFilePath))
            {
                try
                {
                    var data = Serialization.FromJsonFile<GogSecondClassData>(_storageFilePath);
                    return data ?? new GogSecondClassData();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error loading file {_storageFilePath}");
                    FileSystem.DeleteFileSafe(_storageFilePath);
                }
            }

            return new GogSecondClassData();
        }
    }

}
