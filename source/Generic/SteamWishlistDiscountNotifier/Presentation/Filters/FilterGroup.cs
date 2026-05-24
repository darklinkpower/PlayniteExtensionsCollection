using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Presentation.Filters
{
    public class FilterGroup : ObservableObject, IDisposable
    {
        public event EventHandler SettingsChanged;

        public IReadOnlyList<FilterItem> ActiveItems =>
            Sources?.Where(x => x.Enabled).ToList();

        public string SelectionText =>
            string.Join(", ", Sources.Where(x => x.Enabled).Select(x => x.Name));

        public ObservableCollection<FilterItem> Sources { get; }

        public HashSet<Guid> ActiveIds { get; private set; } = new HashSet<Guid>();

        private HashSet<string> enabledFiltersNames = new HashSet<string>();
        public HashSet<string> EnabledFiltersNames
        {
            get => enabledFiltersNames;
            set
            {
                enabledFiltersNames = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<FilterItem> MoveFilterUpCommand { get; }
        public RelayCommand<FilterItem> MoveFilterDownCommand { get; }
        public RelayCommand ClearEnabledCommand { get; }

        public FilterGroup(IEnumerable<FilterItem> sources = null)
        {
            Sources = new ObservableCollection<FilterItem>(
                sources?.OrderBy(x => x.Name) ?? Enumerable.Empty<FilterItem>());

            Sources.CollectionChanged += Sources_CollectionChanged;
            foreach (var item in Sources)
            {
                Subscribe(item);
            }

            MoveFilterUpCommand = new RelayCommand<FilterItem>(MoveUp);
            MoveFilterDownCommand = new RelayCommand<FilterItem>(MoveDown);
            ClearEnabledCommand = new RelayCommand(ClearEnabled);

            Recompute();
        }

        private void Sources_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (FilterItem item in e.OldItems)
                {
                    Unsubscribe(item);
                }
            }

            if (e.NewItems != null)
            {
                foreach (FilterItem item in e.NewItems)
                {
                    Subscribe(item);
                }
            }

            Recompute();
        }

        private void Subscribe(FilterItem item)
        {
            item.PropertyChanged += Item_PropertyChanged;
        }

        private void Unsubscribe(FilterItem item)
        {
            item.PropertyChanged -= Item_PropertyChanged;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterItem.Enabled))
            {
                Recompute();
            }
        }

        private void Recompute()
        {
            ActiveIds = Sources
                .Where(x => x.Enabled)
                .Select(x => x.Id)
                .ToHashSet();

            OnPropertyChanged(nameof(ActiveItems));
            OnPropertyChanged(nameof(ActiveIds));
            OnPropertyChanged(nameof(SelectionText));

            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MoveUp(FilterItem item)
        {
            var index = Sources.IndexOf(item);
            if (index > 0)
            {
                Sources.Move(index, index - 1);
            }
        }

        private void MoveDown(FilterItem item)
        {
            var index = Sources.IndexOf(item);
            if (index >= 0 && index < Sources.Count - 1)
            {
                Sources.Move(index, index + 1);
            }
        }

        private void ClearEnabled()
        {
            foreach (var item in Sources.Where(x => x.Enabled))
            {
                item.Enabled = false;
            }
        }

        internal void UpdateFilters(List<FilterItem> items)
        {
            Sources.CollectionChanged -= Sources_CollectionChanged;
            foreach (var item in Sources)
            {
                Unsubscribe(item);
            }

            Sources.Clear();
            foreach (var item in items.OrderBy(x => x.Name))
            {
                Sources.Add(item);
                Subscribe(item);
            }

            Sources.CollectionChanged += Sources_CollectionChanged;
            Recompute();
        }

        public void Dispose()
        {
            Sources.CollectionChanged -= Sources_CollectionChanged;
            foreach (var item in Sources)
            {
                Unsubscribe(item);
            }
        }
    }
}