using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Presentation.WishlistCategories
{
    public class WishlistCategoryManagerViewModel : ObservableObject
    {
        private readonly WishlistCategoryService _service;

        public ObservableCollection<WishlistCategory> Categories { get; }

        private string _newCategoryName;
        public string NewCategoryName
        {
            get => _newCategoryName;
            set
            {
                _newCategoryName = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand CreateCategoryCommand { get; }
        public RelayCommand<WishlistCategory> DeleteCategoryCommand { get; }

        public WishlistCategoryManagerViewModel(WishlistCategoryService service)
        {
            _service = service;

            Categories = new ObservableCollection<WishlistCategory>(_service.Categories);

            CreateCategoryCommand = new RelayCommand(CreateCategory);
            DeleteCategoryCommand = new RelayCommand<WishlistCategory>(DeleteCategory);
        }

        private void CreateCategory()
        {
            if (NewCategoryName.IsNullOrWhiteSpace())
            {
                return;
            }

            var created = _service.Create(NewCategoryName.Trim());

            Categories.Add(created);
            NewCategoryName = string.Empty;
        }

        private void DeleteCategory(WishlistCategory category)
        {
            if (category is null)
            {
                return;
            }

            _service.Delete(category.Id);
            Categories.Remove(category);
        }
    }
}
