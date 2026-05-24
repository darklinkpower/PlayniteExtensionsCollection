using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Presentation.Filters
{
    public class FilterItem : ObservableObject
    {
        private bool enabled;

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value)
                {
                    return;
                }
                
                enabled = value;
                OnPropertyChanged();
            }
        }

        public string Name { get; }
        public Guid Id { get; }


        public FilterItem(Guid id, string name, bool enabled = false)
        {
            Id = id == Guid.Empty ? throw new ArgumentNullException(nameof(id)) : id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Enabled = enabled;
        }
    }
}