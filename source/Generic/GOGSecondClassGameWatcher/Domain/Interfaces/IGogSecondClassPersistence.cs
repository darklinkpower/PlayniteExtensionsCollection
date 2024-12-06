using GOGSecondClassGameWatcher.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher.Domain.Interfaces
{
    public interface IGogSecondClassPersistence
    {
        void SaveItems(IEnumerable<GogSecondClassGame> items);
        void RemoveItem(GogSecondClassGame item);
        void RemoveItems(IEnumerable<GogSecondClassGame> items);
        void ClearItems();
        GogSecondClassGame GetItemByTitle(string title);
        GogSecondClassGame GetItemById(string id);
        List<GogSecondClassGame> GetAllItems();
        DateTime? GetLastCheckTime();
        void SetLastCheckTime(DateTime time);
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        void ExecuteInTransaction(Action action);
    }
}