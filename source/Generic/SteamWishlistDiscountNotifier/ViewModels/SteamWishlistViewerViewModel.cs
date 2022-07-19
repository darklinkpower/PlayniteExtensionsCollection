using Playnite.SDK;
using PluginsCommon;
using SteamWishlistDiscountNotifier.Enums;
using SteamWishlistDiscountNotifier.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SteamWishlistDiscountNotifier.ViewModels
{
    class SteamWishlistViewerViewModel : ObservableObject
    {
        private readonly IPlayniteAPI playniteApi;
        private static readonly ILogger logger = LogManager.GetLogger();
        private const string steamStoreSubUrlMask = @"https://store.steampowered.com/app/{0}/";
        private const string steamUriOpenUrlMask = @"steam://openurl/{0}";
        public Dictionary<WishlistViewSorting, string> WishlistSortingTypes { get; }
        public Dictionary<ListSortDirection, string> WishlistSortingOrders { get; }
        private WishlistViewSorting selectedSortingType = WishlistViewSorting.Rank;
        public WishlistViewSorting SelectedSortingType
        {
            get { return selectedSortingType; }
            set
            {
                selectedSortingType = value;
                OnPropertyChanged();
                UpdateWishlistSorting();
            }
        }

        private ListSortDirection selectedSortingDirection = ListSortDirection.Ascending;
        public ListSortDirection SelectedSortingDirection
        {
            get { return selectedSortingDirection; }
            set
            {
                
                selectedSortingDirection = value;
                OnPropertyChanged();
                UpdateWishlistSorting();
            }
        }

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

        private int filterMinimumDiscount = 0;
        public int FilterMinimumDiscount
        {
            get { return filterMinimumDiscount; }
            set
            {
                filterMinimumDiscount = value;
                OnPropertyChanged();
                wishlistCollectionView.Refresh();
            }
        }

        private int filterMaximumPrice = 999999;
        public int FilterMaximumPrice
        {
            get { return filterMaximumPrice; }
            set
            {
                filterMaximumPrice = value;
                OnPropertyChanged();
                wishlistCollectionView.Refresh();
            }
        }

        private bool filterItemTypeGame { get; set; } = true;
        public bool FilterItemTypeGame
        {
            get => filterItemTypeGame;
            set
            {
                filterItemTypeGame = value;
                OnPropertyChanged();
                wishlistCollectionView.Refresh();
            }
        }

        private bool filterItemTypeDlc { get; set; } = true;
        public bool FilterItemTypeDlc
        {
            get => filterItemTypeDlc;
            set
            {
                filterItemTypeDlc = value;
                OnPropertyChanged();
                wishlistCollectionView.Refresh();
            }
        }

        private bool filterItemTypeMusic { get; set; } = true;
        public bool FilterItemTypeMusic
        {
            get => filterItemTypeMusic;
            set
            {
                filterItemTypeMusic = value;
                OnPropertyChanged();
                wishlistCollectionView.Refresh();
            }
        }

        private bool filterItemTypeApplication { get; set; } = true;
        public bool FilterItemTypeApplication
        {
            get => filterItemTypeApplication;
            set
            {
                filterItemTypeApplication = value;
                OnPropertyChanged();
                wishlistCollectionView.Refresh();
            }
        }

        private bool filterItemTypeVideo { get; set; } = true;
        public bool FilterItemTypeVideo
        {
            get => filterItemTypeVideo;
            set
            {
                filterItemTypeVideo = value;
                OnPropertyChanged();
                wishlistCollectionView.Refresh();
            }
        }

        private bool filterItemTypeHardware { get; set; } = true;
        public bool FilterItemTypeHardware
        {
            get => filterItemTypeHardware;
            set
            {
                filterItemTypeHardware = value;
                OnPropertyChanged();
                wishlistCollectionView.Refresh();
            }
        }

        public SteamWishlistViewerViewModel(IPlayniteAPI playniteApi, List<WishlistItemCache> wishlistItems, string pluginInstallPath)
        {
            this.playniteApi = playniteApi;
            WishlistItemsCollection = wishlistItems;
            wishlistCollectionView = CollectionViewSource.GetDefaultView(WishlistItemsCollection);
            wishlistCollectionView.Filter = FilterWishlistCollection;
            WishlistSortingTypes = new Dictionary<WishlistViewSorting, string>
            {
                [WishlistViewSorting.Name] = "Name",
                [WishlistViewSorting.Rank] = "Rank",
                [WishlistViewSorting.Discount] = "Discount",
                [WishlistViewSorting.Price] = "Price",
                [WishlistViewSorting.ReleaseDate] = "Release Date"
            };

            WishlistSortingOrders = new Dictionary<ListSortDirection, string>
            {
                [ListSortDirection.Ascending] = "Ascending",
                [ListSortDirection.Descending] = "Descending",
            };

            wishlistCollectionView.SortDescriptions.Add(new SortDescription(GetSortingDescription(), selectedSortingDirection));

            DefaultBannerUri = new Uri(Path.Combine(pluginInstallPath, "Resources", "DefaultBanner.png"), UriKind.Absolute);
        }

        bool FilterWishlistCollection(object item)
        {
            var wishlistItem = item as WishlistItemCache;

            if (filterOnlyDiscounted)
            {
                if (!wishlistItem.IsDiscounted || wishlistItem.DiscountPercent < FilterMinimumDiscount)
                {
                    return false;
                }
            }

            if (wishlistItem.PriceFinal > FilterMaximumPrice)
            {
                return false;
            }

            if (!SearchString.IsNullOrEmpty() && !wishlistItem.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!IsItemCacheTypeFilterEnabled(wishlistItem))
            {
                return false;
            }

            return true;
        }

        private bool IsItemCacheTypeFilterEnabled(WishlistItemCache wishlistItemCache)
        {
            switch (wishlistItemCache.WishlistItem.Type)
            {
                case StoreItemType.Game:
                    return FilterItemTypeGame;
                case StoreItemType.Dlc:
                    return FilterItemTypeDlc;
                case StoreItemType.Music:
                    return FilterItemTypeMusic;
                case StoreItemType.Application:
                    return FilterItemTypeApplication;
                case StoreItemType.Video:
                    return FilterItemTypeVideo;
                case StoreItemType.Series:
                    return FilterItemTypeVideo;
                case StoreItemType.Hardware:
                    return FilterItemTypeHardware;
                case StoreItemType.Mod:
                    return FilterItemTypeGame;
                case StoreItemType.Demo:
                    return FilterItemTypeGame;
                default:
                    return true;
            }
        }

        private void UpdateWishlistSorting()
        {
            using (wishlistCollectionView.DeferRefresh())
            {
                wishlistCollectionView.SortDescriptions.Clear();
                wishlistCollectionView.SortDescriptions.Add(new SortDescription(GetSortingDescription(), selectedSortingDirection));
            }
        }

        private string GetSortingDescription()
        {
            switch (selectedSortingType)
            {
                case WishlistViewSorting.Name:
                    return "Name";
                case WishlistViewSorting.Rank:
                    return "WishlistItem.Priority";
                case WishlistViewSorting.Discount:
                    return "DiscountPercent";
                case WishlistViewSorting.Price:
                    return "PriceFinal";
                case WishlistViewSorting.ReleaseDate:
                    return "WishlistItem.ReleaseDate";
                default:
                    return "Name";
            }
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

        public Uri DefaultBannerUri { get; }

        //private BitmapImage defaultBannerImage;
        //public BitmapImage DefaultBannerImage
        //{
        //    get { return defaultBannerImage; }
        //    set
        //    {
        //        defaultBannerImage = value;
        //        OnPropertyChanged();
        //    }
        //}

        private void OpenWishlistItemOnSteam(WishlistItemCache wishlistItem)
        {
            var subIdSteamUrl = string.Format(steamStoreSubUrlMask, wishlistItem.StoreId);
            ProcessStarter.StartUrl(string.Format(steamUriOpenUrlMask, subIdSteamUrl));
        }

        private void OpenWishlistItemOnWeb(WishlistItemCache wishlistItem)
        {
            var subIdSteamUrl = string.Format(steamStoreSubUrlMask, wishlistItem.StoreId);
            ProcessStarter.StartUrl(subIdSteamUrl);
        }
    }
}