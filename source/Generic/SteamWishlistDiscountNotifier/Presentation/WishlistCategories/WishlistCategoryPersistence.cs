using Playnite.SDK.Data;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace SteamWishlistDiscountNotifier.Presentation.WishlistCategories
{
    public class WishlistCategoryPersistence
    {
        private readonly string _filePath;
        private readonly object _lock = new object();
        private WishlistCategoryStore _pendingStore;

        private CancellationTokenSource _saveCts;

        public WishlistCategoryPersistence(string persistenceDirectoryPath)
        {
            FileSystem.CreateDirectory(persistenceDirectoryPath);
            _filePath = Path.Combine(persistenceDirectoryPath, "wishlistCategories.json");
        }

        public WishlistCategoryStore Load()
        {
            lock (_lock)
            {
                try
                {
                    if (!FileSystem.FileExists(_filePath))
                    {
                        return new WishlistCategoryStore();
                    }

                    var json = File.ReadAllText(_filePath);
                    return Serialization.FromJson<WishlistCategoryStore>(json)
                        ?? new WishlistCategoryStore();
                }
                catch
                {
                    // Corrupted file fallback
                    return new WishlistCategoryStore();
                }
            }
        }

        public void ScheduleSave(WishlistCategoryStore store, int delayMs = 500)
        {
            CancellationTokenSource cts;

            lock (_lock)
            {
                _pendingStore = store;

                _saveCts?.Cancel();
                _saveCts?.Dispose();

                _saveCts = new CancellationTokenSource();
                cts = _saveCts;
            }

            var token = cts.Token;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delayMs, token);

                    WishlistCategoryStore storeToSave = null;

                    lock (_lock)
                    {
                        if (!token.IsCancellationRequested && ReferenceEquals(_saveCts, cts))
                        {
                            storeToSave = _pendingStore;
                            _pendingStore = null;

                            _saveCts.Dispose();
                            _saveCts = null;
                        }
                    }

                    if (storeToSave != null)
                    {
                        Save(storeToSave);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Expected: debounce cancellation
                }
            }, token);
        }

        public void Save(WishlistCategoryStore store)
        {
            lock (_lock)
            {
                try
                {
                    var tempFile = _filePath + ".tmp";
                    var json = Serialization.ToJson(store);
                    FileSystem.WriteStringToFile(tempFile, json, true);

                    FileSystem.CopyFile(tempFile, _filePath, true);
                    FileSystem.DeleteFile(tempFile);
                }
                catch
                {
                    // Intentionally swallow or log if you add logging later
                    // You don't want UI crashes due to disk issues
                }
            }
        }

        /// <summary>
        /// Forces any pending debounced save to complete immediately.
        /// Useful on shutdown.
        /// </summary>
        public void Flush()
        {
            lock (_lock)
            {
                _saveCts?.Cancel();
                _saveCts?.Dispose();
                _saveCts = null;

                if (_pendingStore != null)
                {
                    Save(_pendingStore);
                    _pendingStore = null;
                }
            }
        }
    }
}
