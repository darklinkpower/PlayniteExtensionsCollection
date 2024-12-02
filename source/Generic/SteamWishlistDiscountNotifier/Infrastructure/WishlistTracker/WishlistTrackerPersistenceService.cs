using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
using SteamWishlistDiscountNotifier.Domain.Interfaces;
using SteamWishlistDiscountNotifier.Domain.ValueObjects;
using SteamWishlistDiscountNotifier.SharedKernel.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Infrastructure.WishlistTracker
{
    public class WishlistTrackerPersistenceService : IWishlistTrackerPersistenceService
    {
        private readonly ILogger _logger;
        private readonly string _storageFilePath;
        private WishlistTrackerData _wishlistTrackerData;
        private bool _inTransaction = false;
        private int _transactionDepth = 0;
        private readonly object _lock = new object();
        private readonly object _transactionLock = new object();

        public WishlistTrackerPersistenceService(string storageFileDirectory, ILogger logger)
        {
            _logger = logger;
            _storageFilePath = Path.Combine(storageFileDirectory, "wishlist_tracker_data.bin");
            _wishlistTrackerData = LoadFromFile();
        }

        public void SaveWishlistItem(WishlistItemTrackingInfo item)
        {
            lock (_lock)
            {
                if (_inTransaction)
                {
                    AddOrUpdatePendingItem(item);
                }
                else
                {
                    AddOrUpdateItem(_wishlistTrackerData.WishlistItems, item);
                    SaveToFile();
                }
            }
        }

        public void SaveWishlistItems(IEnumerable<WishlistItemTrackingInfo> items)
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
                        AddOrUpdateItem(_wishlistTrackerData.WishlistItems, item);
                    }

                    SaveToFile();
                }
            }
        }

        public void RemoveWishlistItem(WishlistItemTrackingInfo item)
        {
            lock (_lock)
            {
                if (_inTransaction)
                {
                    _wishlistTrackerData.WishlistItems.Remove(item.AppId);
                }
                else
                {
                    _wishlistTrackerData.WishlistItems.Remove(item.AppId);
                    SaveToFile();
                }
            }
        }

        public void RemoveWishlistItems(IEnumerable<WishlistItemTrackingInfo> items)
        {
            lock (_lock)
            {
                foreach (var item in items)
                {
                    if (_inTransaction)
                    {
                        _wishlistTrackerData.WishlistItems.Remove(item.AppId);
                    }
                    else
                    {
                        _wishlistTrackerData.WishlistItems.Remove(item.AppId);
                        SaveToFile();
                    }
                }
            }
        }

        public void ClearItems()
        {
            lock (_lock)
            {
                if (_inTransaction)
                {
                    _wishlistTrackerData.WishlistItems.Clear();
                }
                else
                {
                    _wishlistTrackerData.WishlistItems.Clear();
                    SaveToFile();
                }
            }
        }

        public WishlistItemTrackingInfo GetWishlistItemByAppId(uint appId)
        {
            lock (_lock)
            {
                _wishlistTrackerData.WishlistItems.TryGetValue(appId, out var item);
                return item;
            }
        }

        public List<WishlistItemTrackingInfo> GetAllWishlistItems()
        {
            lock (_lock)
            {
                return _wishlistTrackerData.WishlistItems.Values.ToList();
            }
        }

        public DateTime? GetLastCheckTime()
        {
            lock (_lock)
            {
                return _wishlistTrackerData.LastCheckTime;
            }
        }

        public void SetLastCheckTime(DateTime time)
        {
            lock (_lock)
            {
                _wishlistTrackerData.LastCheckTime = time;
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
                _wishlistTrackerData = LoadFromFile();
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

        private void AddOrUpdatePendingItem(WishlistItemTrackingInfo item)
        {
            AddOrUpdateItem(_wishlistTrackerData.WishlistItems, item);
        }

        private void AddOrUpdateItem(Dictionary<uint, WishlistItemTrackingInfo> dictionary, WishlistItemTrackingInfo item)
        {
            dictionary[item.AppId] = item;
        }

        private void SaveToFile()
        {
            byte[] serializedData = ProtobufUtilities.SerializeRequest(_wishlistTrackerData);
            FileSystem.WriteBytesToFile(_storageFilePath, serializedData);
        }

        private WishlistTrackerData LoadFromFile()
        {
            if (FileSystem.FileExists(_storageFilePath))
            {
                try
                {
                    byte[] fileContent = FileSystem.ReadBytesFromFile(_storageFilePath);
                    return ProtobufUtilities.DeserializeResponse<WishlistTrackerData>(fileContent);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error loading file {_storageFilePath}");
                    FileSystem.DeleteFileSafe(_storageFilePath);
                }
            }

            return new WishlistTrackerData();
        }
    }



}