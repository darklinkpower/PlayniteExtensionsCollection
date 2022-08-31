using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Models
{
    public class FilterGroup : ObservableObject, IDisposable
    {
        public RelayCommand<FilterItem> MoveFilterUpCommand
        {
            get => new RelayCommand<FilterItem>((a) =>
            {
                var index = Sources.IndexOf(a);
                if (Sources.Count > 1 && (index - 1) >= 0)
                {
                    Sources.Remove(a);
                    Sources.Insert(index - 1, a);
                }
            });
        }

        public RelayCommand<FilterItem> MoveFilterDownCommand
        {
            get => new RelayCommand<FilterItem>((a) =>
            {
                var index = Sources.IndexOf(a);
                if (Sources.Count > 1 && (index + 1) < Sources.Count)
                {
                    Sources.Remove(a);
                    Sources.Insert(index + 1, a);
                }
            });
        }

        public RelayCommand<FilterItem> ClearEnabledCommand
        {
            get => new RelayCommand<FilterItem>((a) =>
            {
                foreach (var item in Sources)
                {
                    if (item.Enabled)
                    {
                        item.Enabled = false;
                    }
                }
            });
        }

        public ObservableCollection<FilterItem> Sources
        {
            get; set;
        }

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

        public string SelectionText
        {
            get => string.Join(", ", EnabledFiltersNames);
        }

        public event EventHandler SettingsChanged;

        public FilterGroup(ObservableCollection<FilterItem> sources)
        {
            Sources = sources.OrderBy(x => x.Name).ToObservable();
            Sources.CollectionChanged += (s, e) =>
            {
                OnSettingsChanged();
            };

            foreach (var source in Sources)
            {
                source.PropertyChanged += (s, e) =>
                {
                    OnSettingsChanged();
                };
            }
        }

        private void OnSettingsChanged()
        {
            var newEnabledFiltersNames = new HashSet<string>();
            foreach (var sourceItem in Sources)
            {
                if (sourceItem.Enabled)
                {
                    newEnabledFiltersNames.Add(sourceItem.Name);
                }
            }

            EnabledFiltersNames = newEnabledFiltersNames;
            OnPropertyChanged(nameof(SelectionText));
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Sources.CollectionChanged -= (s, e) =>
            {
                OnSettingsChanged();
            };

            foreach (var source in Sources)
            {
                source.PropertyChanged -= (s, e) =>
                {
                    OnSettingsChanged();
                };
            }
        }
    }
}