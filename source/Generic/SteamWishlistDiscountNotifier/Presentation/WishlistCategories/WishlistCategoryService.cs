using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Presentation.WishlistCategories
{
    public class WishlistCategoryService
    {
        public event Action CategoriesChanged;

        private readonly WishlistCategoryStore _store;
        private readonly WishlistCategoryPersistence _persistence;

        public WishlistCategoryService(
            WishlistCategoryStore store,
            WishlistCategoryPersistence persistence)
        {
            _store = store;
            _persistence = persistence;
        }

        public IReadOnlyList<WishlistCategory> Categories => _store.Categories;

        public void Assign(uint appId, Guid categoryId)
        {
            _store.Assign(appId, categoryId);
            _persistence.ScheduleSave(_store);
            CategoriesChanged?.Invoke();
        }

        public void Unassign(uint appId, Guid categoryId)
        {
            _store.Unassign(appId, categoryId);
            _persistence.ScheduleSave(_store);
            CategoriesChanged?.Invoke();
        }

        public WishlistCategory Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var normalized = name.Trim();
            var existing = _store.Categories
                .FirstOrDefault(c =>
                    c.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                return existing;
            }

            var cat = new WishlistCategory
            {
                Id = Guid.NewGuid(),
                Name = normalized
            };

            _store.Categories.Add(cat);
            _persistence.ScheduleSave(_store);

            CategoriesChanged?.Invoke();
            return cat;
        }

        public void Delete(Guid categoryId)
        {
            _store.RemoveCategory(categoryId);
            _persistence.ScheduleSave(_store);
            CategoriesChanged?.Invoke();
        }

        public IReadOnlyCollection<Guid> GetCategories(uint appId)
        {
            return _store.GetCategoryIds(appId);
        }
    }
}
