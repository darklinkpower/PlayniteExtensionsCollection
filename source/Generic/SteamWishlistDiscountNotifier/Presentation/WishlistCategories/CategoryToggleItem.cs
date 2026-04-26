using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Presentation.WishlistCategories
{
    public class CategoryToggleItem : ObservableObject
    {
        public Guid Id { get; }
        public string Name { get; }

        private bool? _state;
        public bool? State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged();
                }
            }
        }

        public CategoryToggleItem(Guid id, string name, bool? state)
        {
            Id = id;
            Name = name;
            _state = state;
        }
    }
}
