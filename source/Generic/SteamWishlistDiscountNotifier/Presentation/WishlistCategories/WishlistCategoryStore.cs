using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Presentation.WishlistCategories
{
    public class WishlistCategoryStore
    {
        public List<WishlistCategory> Categories { get; set; } = new List<WishlistCategory>();

        public Dictionary<uint, HashSet<Guid>> ItemCategoryMap { get; set; } = new Dictionary<uint, HashSet<Guid>>();

        /// <summary>
        /// Removes orphaned references (category IDs that no longer exist).
        /// Call after load or category deletion.
        /// </summary>
        public void CleanupOrphans()
        {
            var validCategoryIds = Categories
                .Select(c => c.Id)
                .ToHashSet();

            var emptyKeys = new List<uint>();

            foreach (var kvp in ItemCategoryMap)
            {
                kvp.Value.RemoveWhere(id => !validCategoryIds.Contains(id));

                if (kvp.Value.Count == 0)
                {
                    emptyKeys.Add(kvp.Key);
                }
            }

            foreach (var key in emptyKeys)
            {
                ItemCategoryMap.Remove(key);
            }
        }

        /// <summary>
        /// Gets categories assigned to an item (safe lookup).
        /// </summary>
        public IReadOnlyCollection<Guid> GetCategoryIds(uint appId)
        {
            if (ItemCategoryMap.TryGetValue(appId, out var set))
            {
                return set;
            }

            return Array.Empty<Guid>();
        }

        /// <summary>
        /// Assigns a category to an item.
        /// </summary>
        public void Assign(uint appId, Guid categoryId)
        {
            if (!ItemCategoryMap.TryGetValue(appId, out var set))
            {
                set = new HashSet<Guid>();
                ItemCategoryMap[appId] = set;
            }

            set.Add(categoryId);
        }

        /// <summary>
        /// Removes a category from an item.
        /// </summary>
        public void Unassign(uint appId, Guid categoryId)
        {
            if (ItemCategoryMap.TryGetValue(appId, out var set))
            {
                set.Remove(categoryId);

                if (set.Count == 0)
                {
                    ItemCategoryMap.Remove(appId);
                }
            }
        }

        /// <summary>
        /// Removes a category globally (and all its assignments).
        /// </summary>
        public void RemoveCategory(Guid categoryId)
        {
            Categories.RemoveAll(c => c.Id == categoryId);

            foreach (var kvp in ItemCategoryMap)
            {
                kvp.Value.Remove(categoryId);
            }

            CleanupOrphans();
        }
    }
}
