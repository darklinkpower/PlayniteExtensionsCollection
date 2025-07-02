using LiteDB;
using Playnite.SDK;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace DatabaseCommon
{
    public class LiteDbRepository<T> : IRepository<T>, IDisposable where T : IDatabaseItem<T>
    {
        private readonly ILogger _logger;
        private readonly LiteDatabase _db;
        private readonly LiteCollection<T> _collection;
        private readonly List<T> _deleteBuffer = new List<T>();
        private readonly List<Guid> _deleteIdsBuffer = new List<Guid>();
        private readonly Timer _autoProcessTimer;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly double _bufferProcessTime = 300;

        public LiteDbRepository(string databasePath, ILogger logger)
        {
            _logger = logger;
            _db = OpenOrRecreate(databasePath);
            _collection = _db.GetCollection<T>(typeof(T).Name + "_collection");
            _collection.EnsureIndex(x => x.Id, true);
            _autoProcessTimer = new Timer(AutoProcessBuffer, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public LiteCollection<T> GetRawCollection() => _collection;

        private void StartAutoProcessTimer() =>
            _autoProcessTimer.Change(TimeSpan.FromMilliseconds(_bufferProcessTime), TimeSpan.FromMilliseconds(_bufferProcessTime));

        private void StopAutoProcessTimer() =>
            _autoProcessTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        private LiteDatabase OpenOrRecreate(string path)
        {
            LiteDatabase db = null;
            try
            {
                db = new LiteDatabase($"Filename={path};Mode=Exclusive");

                // Try an insert + delete in each collection to trigger hidden corruption
                foreach (var name in db.GetCollectionNames())
                {
                    try
                    {
                        var collection = db.GetCollection(name);
                        var dummy = new BsonDocument
                        {
                            ["_id"] = ObjectId.NewObjectId(),
                            ["__healthcheck"] = true
                        };

                        collection.Insert(dummy);
                        collection.Delete(dummy["_id"]);
                    }
                    catch (Exception innerEx)
                    {
                        throw new InvalidOperationException($"Health check failed on collection '{name}'", innerEx);
                    }
                }

                return db;
            }
            catch (Exception ex) when (ex is LiteException || ex is InvalidCastException || ex is InvalidOperationException)
            {
                _logger.Error(ex, $"LiteDB corruption detected for database in {path}");
                db?.Dispose(); // Otherwise I/O operations are not possible
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ssZ");
                var backupPath = Path.ChangeExtension(path, $"{timestamp}.corrupt.bak");
                if (FileSystem.FileExists(backupPath))
                {
                    FileSystem.DeleteFile(backupPath);
                }

                FileSystem.MoveFile(path, backupPath);
                _logger.Info($"Backed up corrupt DB to {backupPath}. Creating new DB...");
                var newDb = new LiteDatabase($"Filename={path};Mode=Exclusive");
                TryMigrateFromBackup(backupPath, newDb);
                return newDb;
            }
        }

        private void TryMigrateFromBackup(string backupPath, LiteDatabase targetDb)
        {
            using (var corruptDb = new LiteDatabase($"Filename={backupPath};Mode=ReadOnly"))
            {
                foreach (var name in corruptDb.GetCollectionNames())
                {
                    try
                    {
                        var source = corruptDb.GetCollection(name);
                        var docs = source.FindAll().ToList();
                        var target = targetDb.GetCollection(name);
                        target.InsertBulk(docs);

                        _logger.Info($"Recovered {docs.Count} documents from collection '{name}'");
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, $"Skipping corrupted collection '{name}' during recovery.");
                    }
                }
            }
        }

        private void AutoProcessBuffer(object state)
        {
            _lock.EnterWriteLock();
            try
            {
                StopAutoProcessTimer();

                if (_deleteBuffer.Count > 0)
                {
                    foreach (var item in _deleteBuffer)
                    {
                        _collection.Delete(item.Id);
                    }

                    _deleteBuffer.Clear();
                }

                if (_deleteIdsBuffer.Count > 0)
                {
                    foreach (var id in _deleteIdsBuffer)
                    {
                        _collection.Delete(id);
                    }

                    _deleteIdsBuffer.Clear();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Insert(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                _collection.Insert(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public T GetById(BsonValue id)
        {
            _lock.EnterReadLock();
            try
            {
                return _collection.FindById(id);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public T GetOrCreateById(BsonValue id)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                var existingItem = _collection.FindById(id);
                if (existingItem != null)
                {
                    return existingItem;
                }

                _lock.EnterWriteLock();
                try
                {
                    var newItem = Activator.CreateInstance<T>();
                    newItem.Id = id;
                    _collection.Insert(newItem);
                    return newItem;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public IEnumerable<T> GetAll()
        {
            _lock.EnterReadLock();
            try
            {
                return _collection.FindAll().ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _collection.Find(predicate).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _collection.Find(predicate).FirstOrDefault();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Any(Expression<Func<T, bool>> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _collection.Exists(predicate);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _collection.Count(predicate);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public int CountAll()
        {
            _lock.EnterReadLock();
            try
            {
                return _collection.Count();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Update(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _collection.Update(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Delete(BsonValue id)
        {
            _lock.EnterWriteLock();
            try
            {
                return _collection.Delete(id);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Delete(T item) => Delete(item.Id);

        public int Delete(Expression<Func<T, bool>> predicate)
        {
            _lock.EnterWriteLock();
            try
            {
                return _collection.Delete(predicate);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void AddToDeleteBuffer(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                _deleteBuffer.Add(item);
                StartAutoProcessTimer();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void AddToDeleteBuffer(BsonValue id)
        {
            _lock.EnterWriteLock();
            try
            {
                _deleteIdsBuffer.Add(id);
                StartAutoProcessTimer();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void DeleteAll()
        {
            _lock.EnterWriteLock();
            try
            {
                StopAutoProcessTimer();
                _deleteBuffer.Clear();
                _deleteIdsBuffer.Clear();
                _collection.Delete(Query.All());
                _db.Shrink();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void ShrinkDatabase()
        {
            _lock.EnterWriteLock();
            try
            {
                _db.Shrink();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to shrink LiteDB database.");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            _db.Dispose();
            _autoProcessTimer.Dispose();
            _lock.Dispose();
        }
    }

}