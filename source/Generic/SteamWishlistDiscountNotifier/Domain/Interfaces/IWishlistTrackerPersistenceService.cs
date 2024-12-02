using SteamWishlistDiscountNotifier.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Domain.Interfaces
{
    /// <summary>
    /// Interface for persistence of wishlist tracking items. 
    /// Provides methods to save, remove, and retrieve items, with support for transactional operations.
    /// </summary>
    public interface IWishlistTrackerPersistenceService
    {
        /// <summary>
        /// Retrieves the last time the wishlist tracking check was performed.
        /// </summary>
        /// <returns>A <see cref="DateTime"/> representing the time of the last check, or <c>null</c> if no check has been performed.</returns>
        DateTime? GetLastCheckTime();

        /// <summary>
        /// Sets the time of the last wishlist tracking check.
        /// </summary>
        /// <param name="time">The <see cref="DateTime"/> to store as the last check time.</param>
        void SetLastCheckTime(DateTime time);


        /// <summary>
        /// Saves a single wishlist item to the persistence storage.
        /// If an item with the same AppId already exists, it will be updated.
        /// </summary>
        /// <param name="item">The wishlist item to save or update.</param>
        void SaveWishlistItem(WishlistItemTrackingInfo item);

        /// <summary>
        /// Saves a collection of wishlist items to the persistence storage.
        /// If any items with the same AppId already exist, they will be updated.
        /// </summary>
        /// <param name="items">The collection of wishlist items to save or update.</param>
        void SaveWishlistItems(IEnumerable<WishlistItemTrackingInfo> items);

        /// <summary>
        /// Removes a single wishlist item from the persistence storage.
        /// </summary>
        /// <param name="item">The wishlist item to remove.</param>
        void RemoveWishlistItem(WishlistItemTrackingInfo item);

        /// <summary>
        /// Removes a collection of wishlist items from the persistence storage.
        /// </summary>
        /// <param name="items">The collection of wishlist items to remove.</param>
        void RemoveWishlistItems(IEnumerable<WishlistItemTrackingInfo> items);

        /// <summary>
        /// Clears all wishlist items from the persistence storage.
        /// </summary>
        void ClearItems();

        /// <summary>
        /// Retrieves a wishlist item by its AppId.
        /// </summary>
        /// <param name="appId">The AppId of the wishlist item to retrieve.</param>
        /// <returns>The wishlist item with the specified AppId, or null if not found.</returns>
        WishlistItemTrackingInfo GetWishlistItemByAppId(uint appId);

        /// <summary>
        /// Retrieves all wishlist items from the persistence storage.
        /// </summary>
        /// <returns>A list of all tracked wishlist items.</returns>
        List<WishlistItemTrackingInfo> GetAllWishlistItems();

        // Transaction-related methods

        /// <summary>
        /// Begins a transaction, allowing changes to be deferred until the transaction is committed.
        /// Supports nested transactions.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Commits the current transaction, persisting all changes made during the transaction.
        /// Only persists changes when the outermost transaction is committed.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Rolls back the current transaction, discarding any changes made during the transaction.
        /// Only applies to the outermost transaction if nested.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Executes an action within a transaction, ensuring that changes are only persisted 
        /// if the action completes successfully. Rolls back the transaction if an exception is thrown.
        /// </summary>
        /// <param name="action">The action to execute within the transaction.</param>
        void ExecuteInTransaction(Action action);
    }

}