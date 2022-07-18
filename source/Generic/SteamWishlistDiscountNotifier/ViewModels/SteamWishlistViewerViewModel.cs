using Playnite.SDK;
using PluginsCommon;
using SteamWishlistDiscountNotifier.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SteamWishlistDiscountNotifier.ViewModels
{
    class SteamWishlistViewerViewModel : ObservableObject
    {
        private readonly IPlayniteAPI playniteApi;
        private static readonly ILogger logger = LogManager.GetLogger();
        private const string steamStoreSubUrlMask = @"https://store.steampowered.com/sub/{0}/";
        private const string steamUriOpenUrlMask = @"steam://openurl/{0}";
        private List<WishlistItemCache> wishlistItemsCollection;
        public List<WishlistItemCache> WishlistItemsCollection
        {
            get
            {
                return wishlistItemsCollection;
            }
            set
            {
                wishlistItemsCollection = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView wishlistCollectionView;
        public ICollectionView WishlistCollectionView
        {
            get { return wishlistCollectionView; }
        }

        private string searchString = string.Empty;
        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value;
                OnPropertyChanged();
                wishlistCollectionView.Refresh();
            }
        }

        private WishlistItemCache selectedWishlistItem;
        public WishlistItemCache SelectedWishlistItem
        {
            get { return selectedWishlistItem; }
            set
            {
                selectedWishlistItem = value;
                OnPropertyChanged();
            }
        }

        private bool filterOnlyDiscounted { get; set; } = true;
        public bool FilterOnlyDiscounted
        {
            get => filterOnlyDiscounted;
            set
            {
                filterOnlyDiscounted = value;
                OnPropertyChanged();
                wishlistCollectionView.Refresh();
            }
        }

        public SteamWishlistViewerViewModel(IPlayniteAPI playniteApi, List<WishlistItemCache> wishlistItems)
        {
            this.playniteApi = playniteApi;
            WishlistItemsCollection = wishlistItems;
            wishlistCollectionView = CollectionViewSource.GetDefaultView(WishlistItemsCollection);
            wishlistCollectionView.Filter = FilterSkProfilesCollection;
        }

        bool FilterSkProfilesCollection(object item)
        {
            var wishlistItem = item as WishlistItemCache;

            if (filterOnlyDiscounted && !wishlistItem.IsDiscounted)
            {
                return false;
            }

            if (!SearchString.IsNullOrEmpty() && !wishlistItem.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public RelayCommand<WishlistItemCache> OpenWishlistItemOnSteamCommand
        {
            get => new RelayCommand<WishlistItemCache>((a) =>
            {
                OpenWishlistItemOnSteam(a);
            });
        }

        public RelayCommand<WishlistItemCache> OpenWishlistItemOnWebCommand
        {
            get => new RelayCommand<WishlistItemCache>((a) =>
            {
                OpenWishlistItemOnWeb(a);
            });
        }

        private void OpenWishlistItemOnSteam(WishlistItemCache wishlistItem)
        {
            var subIdSteamUrl = string.Format(steamStoreSubUrlMask, wishlistItem.Id);
            ProcessStarter.StartUrl(string.Format(steamUriOpenUrlMask, subIdSteamUrl));
        }

        private void OpenWishlistItemOnWeb(WishlistItemCache wishlistItem)
        {
            var subIdSteamUrl = string.Format(steamStoreSubUrlMask, wishlistItem.Id);
           ProcessStarter.StartUrl(subIdSteamUrl);
        }
    }
}