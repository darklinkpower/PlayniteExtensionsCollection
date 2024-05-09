using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
using SteamWishlistDiscountNotifier.Enums;
using SteamWishlistDiscountNotifier.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using FlowHttp;
using FlowHttp.Constants;

namespace SteamWishlistDiscountNotifier.ViewModels
{
    class SteamWishlistViewerViewModel : ObservableObject, IDisposable
    {
        private readonly IPlayniteAPI _playniteApi;
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly char[] textMatchSplitter = new char[] { ' ' };
        private const string steamStoreSubUrlMask = @"https://store.steampowered.com/app/{0}/";
        private const string steamUriOpenUrlMask = @"steam://openurl/{0}";
        
        private SteamAccountInfo accountInfo;
        public SteamAccountInfo AccountInfo
        {
            get { return accountInfo; }
            set
            {
                accountInfo = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<WishlistViewSorting, string> WishlistSortingTypes { get; }
        public Dictionary<ListSortDirection, string> WishlistSortingOrders { get; }
        public Dictionary<Ownership, string> OwnershipTypes { get; }
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

        private Ownership selectedOwnershipType = Ownership.Any;
        public Ownership SelectedOwnershipType
        {
            get { return selectedOwnershipType; }
            set
            {

                selectedOwnershipType = value;
                OnPropertyChanged();
                UpdateWishlistSorting();
            }
        }

        private List<WishlistCacheItemViewWrapper> wishlistItemsCollection;
        public List<WishlistCacheItemViewWrapper> WishlistItemsCollection
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

        private WishlistCacheItemViewWrapper selectedWishlistItem;
        public WishlistCacheItemViewWrapper SelectedWishlistItem
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

        private double filterMinimumPrice = 0;
        public double FilterMinimumPrice
        {
            get { return filterMinimumPrice; }
            set
            {
                filterMinimumPrice = value;
                OnPropertyChanged();
                wishlistCollectionView.Refresh();
            }
        }

        private double filterMaximumPrice = 999999;
        public double FilterMaximumPrice
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

        private bool filterIncludeReleased { get; set; } = true;
        public bool FilterIncludeReleased
        {
            get => filterIncludeReleased;
            set
            {
                filterIncludeReleased = value;
                OnPropertyChanged();
                wishlistCollectionView.Refresh();
            }
        }

        private bool filterIncludeNotReleased { get; set; } = true;
        public bool FilterIncludeNotReleased
        {
            get => filterIncludeNotReleased;
            set
            {
                filterIncludeNotReleased = value;
                OnPropertyChanged();
                wishlistCollectionView.Refresh();
            }
        }

        private FilterGroup tagFilters;
        private string _steamSessionId;
        private string _steamLoginSecure;

        public FilterGroup TagFilters
        {
            get => tagFilters;
            set
            {
                tagFilters = value;
                OnPropertyChanged();
            }
        }

        public SteamWishlistViewerViewModel(IPlayniteAPI playniteApi, SteamAccountInfo accountInfo, List<WishlistCacheItemViewWrapper> wishlistItems, string pluginInstallPath)
        {
            this._playniteApi = playniteApi;
            AccountInfo = accountInfo;
            WishlistItemsCollection = wishlistItems;
            DefaultBannerUri = new Uri(Path.Combine(pluginInstallPath, "Resources", "DefaultBanner.png"), UriKind.Absolute);

            var tags = new HashSet<string>();
            var tagsFiltersSource = new ObservableCollection<FilterItem>();
            foreach (var item in wishlistItems)
            {
                foreach (var tag in item.Data.WishlistItem.Tags)
                {
                    if (tags.Contains(tag))
                    {
                        continue;
                    }

                    tagsFiltersSource.Add(new FilterItem(false, tag));
                    tags.Add(tag);
                }
            }

            UpdateSteamCookiesValues();

            TagFilters = new FilterGroup(tagsFiltersSource);
            TagFilters.SettingsChanged += TagFilters_SettingsChanged;

            wishlistCollectionView = CollectionViewSource.GetDefaultView(WishlistItemsCollection);
            wishlistCollectionView.Filter = FilterWishlistCollection;
            WishlistSortingTypes = new Dictionary<WishlistViewSorting, string>
            {
                [WishlistViewSorting.Name] = ResourceProvider.GetString("LOCSteam_Wishlist_Notif_WishlistViewSortingTypeName"),
                [WishlistViewSorting.Rank] = ResourceProvider.GetString("LOCSteam_Wishlist_Notif_WishlistViewSortingTypeRank"),
                [WishlistViewSorting.Discount] = ResourceProvider.GetString("LOCSteam_Wishlist_Notif_WishlistViewSortingTypeDiscount"),
                [WishlistViewSorting.Price] = ResourceProvider.GetString("LOCSteam_Wishlist_Notif_WishlistViewSortingTypePrice"),
                [WishlistViewSorting.ReleaseDate] = ResourceProvider.GetString("LOCSteam_Wishlist_Notif_WishlistViewSortingTypeReleaseDate"),
                [WishlistViewSorting.Added] = ResourceProvider.GetString("LOCSteam_Wishlist_Notif_WishlistViewSortingTypeAdded")
            };

            WishlistSortingOrders = new Dictionary<ListSortDirection, string>
            {
                [ListSortDirection.Ascending] = ResourceProvider.GetString("LOCSteam_Wishlist_Notif_WishlistViewSortingDirectionAscending"),
                [ListSortDirection.Descending] = ResourceProvider.GetString("LOCSteam_Wishlist_Notif_WishlistViewSortingDirectionDescending"),
            };

            OwnershipTypes = new Dictionary<Ownership, string>
            {
                [Ownership.Any] = ResourceProvider.GetString("LOCSteam_Wishlist_Notif_GameOwnershipAny"),
                [Ownership.Owned] = ResourceProvider.GetString("LOCSteam_Wishlist_Notif_GameOwnershipOwned"),
                [Ownership.NotOwned] = ResourceProvider.GetString("LOCSteam_Wishlist_Notif_GameOwnershipNotOwned")
            };

            wishlistCollectionView.SortDescriptions.Add(new SortDescription(GetSortingDescription(), selectedSortingDirection));
        }

        private void TagFilters_SettingsChanged(object sender, EventArgs e)
        {
            wishlistCollectionView.Refresh();
        }

        private void UpdateSteamCookiesValues()
        {
            using (var webView = _playniteApi.WebViews.CreateOffscreenView())
            {
                webView.NavigateAndWait(@"https://store.steampowered.com/cart");

                _steamSessionId = GetCookieValue(webView, "store.steampowered.com", "sessionid");
                _steamLoginSecure = GetCookieValue(webView, "store.steampowered.com", "steamLoginSecure");
            }
        }

        private string GetCookieValue(IWebView webView, string domain, string cookieName)
        {
            var cookie = webView.GetCookies()
                .FirstOrDefault(x => x.Domain == domain && x.Name == cookieName);

            return cookie?.Value;
        }

        bool FilterWishlistCollection(object item)
        {
            var wishlistItemWrapper = item as WishlistCacheItemViewWrapper;
            var wishlistItem = wishlistItemWrapper.Data;
            if (filterOnlyDiscounted)
            {
                if (!wishlistItem.IsDiscounted || wishlistItem.DiscountPercent < FilterMinimumDiscount)
                {
                    return false;
                }
            }

            if (wishlistItem.PriceFinal.HasValue)
            {
                if (!filterIncludeReleased)
                {
                    return false;
                }
            }
            else if (!filterIncludeNotReleased)
            {
                return false;
            }

            if (FilterMinimumPrice != 0)
            {
                if (!wishlistItem.PriceFinal.HasValue || wishlistItem.PriceFinal <= FilterMinimumPrice)
                {
                    return false;
                }
            }

            if (FilterMaximumPrice != 999999)
            {
                if (!wishlistItem.PriceFinal.HasValue || wishlistItem.PriceFinal > FilterMaximumPrice)
                {
                    return false;
                }
            }

            if (selectedOwnershipType == Ownership.Owned && !wishlistItemWrapper.OwnedSources.HasItems())
            {
                return false;
            }
            else if (selectedOwnershipType == Ownership.NotOwned && wishlistItemWrapper.OwnedSources.HasItems())
            {
                return false;
            }

            if (!MatchTextFilter(wishlistItem.Name))
            {
                return false;
            }

            if (!IsItemCacheTypeFilterEnabled(wishlistItem))
            {
                return false;
            }

            if (TagFilters.EnabledFiltersNames.Any(x => !wishlistItem.WishlistItem.Tags.Contains(x)))
            {
                return false;
            }

            return true;
        }

        // Based on https://github.com/JosefNemec/Playnite
        public bool MatchTextFilter(string toMatch)
        {
            if (searchString.IsNullOrWhiteSpace())
            {
                return true;
            }

            if (!searchString.IsNullOrWhiteSpace() && toMatch.IsNullOrWhiteSpace())
            {
                return false;
            }

            if (searchString.IsNullOrWhiteSpace() && toMatch.IsNullOrWhiteSpace())
            {
                return true;
            }

            if (searchString.GetJaroWinklerSimilarityIgnoreCase(toMatch) >= 0.95)
            {
                return true;
            }

            if (!searchString.MatchesAllWords(toMatch))
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
                    return "Data.Name";
                case WishlistViewSorting.Rank:
                    return "Data.WishlistItem.Priority";
                case WishlistViewSorting.Discount:
                    return "Data.DiscountPercent";
                case WishlistViewSorting.Price:
                    return "Data.PriceFinal";
                case WishlistViewSorting.ReleaseDate:
                    return "Data.WishlistItem.ReleaseDate";
                case WishlistViewSorting.Added:
                    return "Data.WishlistItem.Added";
                default:
                    return "Data.Name";
            }
        }

        private void RemoveItemFromWishlist(WishlistCacheItemViewWrapper cacheItem)
        {
            var success = false;
            _playniteApi.Dialogs.ActivateGlobalProgress(async (a) =>
            {
                var request = HttpRequestFactory.GetFlowHttpRequest()
                    .WithUrl("https://store.steampowered.com/api/removefromwishlist")
                    .WithCookies(new Dictionary<string, string> { { "sessionid", _steamSessionId }, { "steamLoginSecure", _steamLoginSecure } })
                    .WithPostHttpMethod()
                    .WithContent($"sessionid={_steamSessionId}&appid={cacheItem.Data.StoreId}", HttpContentTypes.FormUrlEncoded, Encoding.UTF8);
                
                var result = await request.DownloadStringAsync(a.CancelToken);
                if (!result.IsSuccess || !Serialization.TryFromJson<WishlistAddRemoveRequestResponse>(result.Content, out var response) || !response.Success)
                {
                    _playniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_RemoveFromWishlistFail"), "Steam Wishlist Discount Notifier");
                    return;
                }

                success = true;
                
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_RemovingFromWishlist")) { Cancelable = true });

            if (success)
            {
                WishlistItemsCollection.Remove(cacheItem);
                wishlistCollectionView.Refresh();
            }
        }

        public RelayCommand<WishlistCacheItemViewWrapper> RemoveItemFromWishlistCommand
        {
            get => new RelayCommand<WishlistCacheItemViewWrapper>((a) =>
            {
                RemoveItemFromWishlist(a);
            });
        }

        public RelayCommand<WishlistCacheItemViewWrapper> OpenWishlistItemOnSteamCommand
        {
            get => new RelayCommand<WishlistCacheItemViewWrapper>((a) =>
            {
                OpenWishlistItemOnSteam(a);
            });
        }

        public RelayCommand<WishlistCacheItemViewWrapper> OpenWishlistItemOnWebCommand
        {
            get => new RelayCommand<WishlistCacheItemViewWrapper>((a) =>
            {
                OpenWishlistItemOnWeb(a);
            });
        }

        public RelayCommand ResetSearchStringCommand
        {
            get => new RelayCommand(() =>
            {
                if (SearchString != string.Empty)
                {
                    SearchString = string.Empty;
                }
            });
        }

        public RelayCommand ResetFilterMinimumPriceCommand
        {
            get => new RelayCommand(() =>
            {
                if (FilterMinimumPrice != 0)
                {
                    FilterMinimumPrice = 0;
                }
            });
        }

        public RelayCommand ResetFilterMaximumPriceCommand
        {
            get => new RelayCommand(() =>
            {
                if (FilterMaximumPrice != 999999)
                {
                    FilterMaximumPrice = 999999;
                }
            });
        }

        public RelayCommand SetMaximumPriceToWalletCommand
        {
            get => new RelayCommand(() =>
            {
                if (AccountInfo.WalletAmount != 0 && FilterMaximumPrice != AccountInfo.WalletAmount)
                {
                    FilterMaximumPrice = AccountInfo.WalletAmount;
                }
            });
        }

        public RelayCommand NavigateBackCommand
        {
            get => new RelayCommand(() =>
            {
                _playniteApi.MainView.SwitchToLibraryView();
            });
        }

        public Uri DefaultBannerUri { get; }

        private void OpenWishlistItemOnSteam(WishlistCacheItemViewWrapper wishlistItem)
        {
            var subIdSteamUrl = string.Format(steamStoreSubUrlMask, wishlistItem.Data.StoreId);
            ProcessStarter.StartUrl(string.Format(steamUriOpenUrlMask, subIdSteamUrl));
        }

        private void OpenWishlistItemOnWeb(WishlistCacheItemViewWrapper wishlistItem)
        {
            var subIdSteamUrl = string.Format(steamStoreSubUrlMask, wishlistItem.Data.StoreId);
            ProcessStarter.StartUrl(subIdSteamUrl);
        }

        public void Dispose()
        {
            TagFilters.SettingsChanged -= TagFilters_SettingsChanged;
            TagFilters?.Dispose();
        }
    }
}