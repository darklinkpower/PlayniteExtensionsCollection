using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Presentation.WishlistCategories
{
    public class WishlistBatchCategoryEditorViewModel : ObservableObject
    {
        private readonly WishlistCategoryService _service;
        private readonly List<SteamWishlistViewItem> _items;

        public ObservableCollection<CategoryToggleItem> Categories { get; }

        public RelayCommand ApplyCommand { get; }
        public RelayCommand ClearAllCommand { get; }
        public RelayCommand CloseCommand { get; }

        public WishlistBatchCategoryEditorViewModel(
            List<SteamWishlistViewItem> items,
            WishlistCategoryService service,
            Action closeAction)
        {
            _items = items;
            _service = service;

            Categories = new ObservableCollection<CategoryToggleItem>(
                _service.Categories.Select(cat =>
                {
                    var states = _items
                        .Select(i => i.CategoryIds.Contains(cat.Id))
                        .Distinct()
                        .ToList();

                    bool? state = states.Count == 1 ? states[0] : (bool?)null;
                    return new CategoryToggleItem(cat.Id, cat.Name, state);
                }));

            ApplyCommand = new RelayCommand(Apply);
            ClearAllCommand = new RelayCommand(ClearAll);
            CloseCommand = new RelayCommand(closeAction);
        }

        private void Apply()
        {
            foreach (var cat in Categories)
            {
                if (cat.State is null)
                {
                    continue;
                }

                foreach (var item in _items)
                {
                    if (cat.State == true)
                    {
                        _service.Assign(item.Appid, cat.Id);
                        item.CategoryIds.Add(cat.Id);
                    }
                    else
                    {
                        _service.Unassign(item.Appid, cat.Id);
                        item.CategoryIds.Remove(cat.Id);
                    }
                }
            }

            CloseCommand?.Execute(null);
        }

        private void ClearAll()
        {
            foreach (var cat in Categories)
            {
                cat.State = false;
            }
        }


    }
}
