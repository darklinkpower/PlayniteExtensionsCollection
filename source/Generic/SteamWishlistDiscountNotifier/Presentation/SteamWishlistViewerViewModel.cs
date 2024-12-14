using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
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
using SteamWishlistDiscountNotifier.Domain.ValueObjects;
using SteamCommon;
using SteamWishlistDiscountNotifier.Domain.Enums;
using SteamWishlistDiscountNotifier.Application.Steam.Wishlist;
using SteamWishlistDiscountNotifier.Presentation.Filters;
using SteamWishlistDiscountNotifier.Application.Steam.Tags;

namespace SteamWishlistDiscountNotifier.Presentation
{
    internal class SteamWishlistViewerViewModel : ObservableObject, IDisposable
    {
        #region Fields
        private readonly IPlayniteAPI _playniteApi;
        private string _steamSessionId;
        private string _steamLoginSecure;

        private const string _steamStoreSubUrlMask = @"https://store.steampowered.com/app/{0}/";
        private const string _steamUriOpenUrlMask = @"steam://openurl/{0}";
        #endregion

        #region Properties
        private SteamAccountInfo _accountInfo;
        public SteamAccountInfo AccountInfo
        {
            get => _accountInfo;
            set => SetValue(ref _accountInfo, value);
        }

        public Dictionary<WishlistViewSorting, string> WishlistSortingTypes { get; }
        public Dictionary<ListSortDirection, string> WishlistSortingOrders { get; }
        public Dictionary<Ownership, string> OwnershipTypes { get; }

        private WishlistViewSorting _selectedSortingType = WishlistViewSorting.Rank;
        public WishlistViewSorting SelectedSortingType
        {
            get => _selectedSortingType;
            set
            {
                SetValue(ref _selectedSortingType, value);
                UpdateWishlistSorting();
            }
        }

        private ListSortDirection _selectedSortingDirection = ListSortDirection.Ascending;
        public ListSortDirection SelectedSortingDirection
        {
            get => _selectedSortingDirection;
            set
            {
                SetValue(ref _selectedSortingDirection, value);
                UpdateWishlistSorting();
            }
        }

        private Ownership _selectedOwnershipType = Ownership.Any;
        public Ownership SelectedOwnershipType
        {
            get => _selectedOwnershipType;
            set
            {
                SetValue(ref _selectedOwnershipType, value);
                RefreshWishlistView();
            }
        }

        private List<SteamWishlistViewItem> _wishlistItemsCollection;
        public List<SteamWishlistViewItem> WishlistItemsCollection
        {
            get => _wishlistItemsCollection;
            set => SetValue(ref _wishlistItemsCollection, value);
        }

        private ICollectionView _wishlistCollectionView;
        public ICollectionView WishlistCollectionView => _wishlistCollectionView;

        private string _searchString = string.Empty;
        public string SearchString
        {
            get => _searchString;
            set
            {
                SetValue(ref _searchString, value);
                RefreshWishlistView();
            }
        }

        private SteamWishlistViewItem _selectedWishlistItem;
        public SteamWishlistViewItem SelectedWishlistItem
        {
            get => _selectedWishlistItem;
            set => SetValue(ref _selectedWishlistItem, value);
        }

        private FilterGroup _tagFilters;
        public FilterGroup TagFilters
        {
            get => _tagFilters;
            set => SetValue(ref _tagFilters, value);
        }

        public Uri DefaultBannerUri { get; }

        // Filters
        private bool _filterOnlyDiscounted = true;
        public bool FilterOnlyDiscounted
        {
            get => _filterOnlyDiscounted;
            set
            {
                SetValue(ref _filterOnlyDiscounted, value);
                RefreshWishlistView();
            }
        }

        private int _filterMinimumDiscount = 0;
        public int FilterMinimumDiscount
        {
            get => _filterMinimumDiscount;
            set
            {
                SetValue(ref _filterMinimumDiscount, value);
                RefreshWishlistView();
            }
        }

        private double _filterMinimumPrice = 0;
        public double FilterMinimumPrice
        {
            get => _filterMinimumPrice;
            set
            {
                SetValue(ref _filterMinimumPrice, value);
                RefreshWishlistView();
            }
        }

        private double _filterMaximumPrice = 999999;
        public double FilterMaximumPrice
        {
            get => _filterMaximumPrice;
            set
            {
                SetValue(ref _filterMaximumPrice, value);
                RefreshWishlistView();
            }
        }

        private bool _filterItemTypeGame = true;
        public bool FilterItemTypeGame
        {
            get => _filterItemTypeGame;
            set
            {
                _filterItemTypeGame = value;
                OnPropertyChanged();
                RefreshWishlistView();
            }
        }

        private bool _filterItemTypeDlc = true;
        public bool FilterItemTypeDlc
        {
            get => _filterItemTypeDlc;
            set
            {
                _filterItemTypeDlc = value;
                OnPropertyChanged();
                RefreshWishlistView();
            }
        }

        private bool _filterItemTypeMusic = true;
        public bool FilterItemTypeMusic
        {
            get => _filterItemTypeMusic;
            set
            {
                _filterItemTypeMusic = value;
                OnPropertyChanged();
                RefreshWishlistView();
            }
        }

        private bool _filterItemTypeApplication = true;
        public bool FilterItemTypeApplication
        {
            get => _filterItemTypeApplication;
            set
            {
                _filterItemTypeApplication = value;
                OnPropertyChanged();
                RefreshWishlistView();
            }
        }

        private bool _filterItemTypeVideo = true;
        public bool FilterItemTypeVideo
        {
            get => _filterItemTypeVideo;
            set
            {
                _filterItemTypeVideo = value;
                OnPropertyChanged();
                RefreshWishlistView();
            }
        }

        private bool _filterItemTypeHardware = true;
        public bool FilterItemTypeHardware
        {
            get => _filterItemTypeHardware;
            set
            {
                _filterItemTypeHardware = value;
                OnPropertyChanged();
                RefreshWishlistView();
            }
        }

        private bool _filterIncludeReleased = true;
        public bool FilterIncludeReleased
        {
            get => _filterIncludeReleased;
            set
            {
                _filterIncludeReleased = value;
                OnPropertyChanged();
                RefreshWishlistView();
            }
        }

        private bool _filterIncludeNotReleased = true;
        public bool FilterIncludeNotReleased
        {
            get => _filterIncludeNotReleased;
            set
            {
                _filterIncludeNotReleased = value;
                OnPropertyChanged();
                RefreshWishlistView();
            }
        }

        public string FormattedBalance { get; }

        #endregion

        #region Commands
        public RelayCommand NavigateBackCommand { get; }
        public RelayCommand ResetSearchStringCommand { get; }
        public RelayCommand ResetFilterMinimumPriceCommand { get; }
        public RelayCommand ResetFilterMaximumPriceCommand { get; }
        public RelayCommand SetMaximumPriceToWalletCommand { get; }
        public RelayCommand<SteamWishlistViewItem> RemoveItemFromWishlistCommand { get; }
        public RelayCommand<SteamWishlistViewItem> OpenWishlistItemOnSteamCommand { get; }
        public RelayCommand<SteamWishlistViewItem> OpenWishlistItemOnWebCommand { get; }
        public RelayCommand<SteamWishlistViewItem> OpenWishlistItemOnSteamDbCommand { get; }

        #endregion

        #region Constructor

        public SteamWishlistViewerViewModel(
            IPlayniteAPI playniteApi,
            SteamWalletDetails walletDetails,
            List<CWishlistGetWishlistSortedFilteredResponseWishlistItem> wishlistResponseItems,
            List<Tag> tags,
            Dictionary<uint, string> bannersPathsMapper,
            string pluginInstallPath)
        {
            _playniteApi = playniteApi;
            FormattedBalance = walletDetails.FormattedBalance;
            WishlistItemsCollection = CreateViewItems(wishlistResponseItems, bannersPathsMapper, tags);
            DefaultBannerUri = new Uri(Path.Combine(pluginInstallPath, "Resources", "DefaultBanner.png"), UriKind.Absolute);

            NavigateBackCommand = new RelayCommand(() => _playniteApi.MainView.SwitchToLibraryView());
            ResetSearchStringCommand = new RelayCommand(() => SearchString = string.Empty);
            ResetFilterMinimumPriceCommand = new RelayCommand(() => FilterMinimumPrice = 0);
            ResetFilterMaximumPriceCommand = new RelayCommand(() => FilterMaximumPrice = 999999);
            SetMaximumPriceToWalletCommand = new RelayCommand(() => FilterMaximumPrice = (double)walletDetails.Balance / 100);
            RemoveItemFromWishlistCommand = new RelayCommand<SteamWishlistViewItem>((a) => RemoveWishlistItem(a));
            OpenWishlistItemOnSteamCommand = new RelayCommand<SteamWishlistViewItem>((a) => OpenWishlistItemInSteamClient(a));
            OpenWishlistItemOnWebCommand = new RelayCommand<SteamWishlistViewItem>((a) => OpenWishlistItemInBrowser(a));
            OpenWishlistItemOnSteamDbCommand = new RelayCommand<SteamWishlistViewItem>((a) => OpenWishlistItemInSteamDb(a));

            TagFilters = new FilterGroup(CreateFilterItems(WishlistItemsCollection));
            TagFilters.SettingsChanged += OnTagFiltersSettingsChanged;

            _wishlistCollectionView = InitializeWishlistCollectionView(WishlistItemsCollection);
            _wishlistCollectionView = CollectionViewSource.GetDefaultView(WishlistItemsCollection);
            _wishlistCollectionView.SortDescriptions.Add(new SortDescription(GetSortingPropertyName(), _selectedSortingDirection));

            UpdateSteamCookies();
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
        }

        public ObservableCollection<FilterItem> CreateFilterItems(List<SteamWishlistViewItem> wishlistItemsCollection)
        {
            var dict = new Dictionary<string, FilterItem>();
            foreach (var wishlistItem in wishlistItemsCollection)
            {
                foreach (var tag in wishlistItem.Tags)
                {
                    if (!dict.ContainsKey(tag))
                    {
                        dict[tag] = new FilterItem(false, tag);
                    }
                }
            }

            return dict.OrderBy(x => x.Key).Select(x => x.Value).ToObservable();
        }

        public List<string> GetWishlistItemTags(CWishlistGetWishlistSortedFilteredResponseWishlistItem wishlistItem, Dictionary<uint, string> tagsDictionary)
        {
            if (wishlistItem.StoreItem.Tags is null)
            {
                return new List<string>();
            }

            return wishlistItem.StoreItem.Tags
                .OrderByDescending(x => x.Weight)
                .Take(5)
                .Select(x => tagsDictionary.TryGetValue(x.Tagid, out var tagName) ? tagName : null)
                .Where(tagName => tagName != null)
                .ToList();
        }

        private List<SteamWishlistViewItem> CreateViewItems(
            List<CWishlistGetWishlistSortedFilteredResponseWishlistItem> wishlistResponseItems,
            Dictionary<uint, string> bannersPathsMapper,
            List<Tag> tags)
        {
            var otherSourcesOwnership = GetNonSteamOwnedItems();
            var items = new List<SteamWishlistViewItem>();
            var tagsDictionary = tags.ToDictionary(x => x.Tagid, x => x.Name);
            foreach (var wishlistItem in wishlistResponseItems)
            {
                var wishlistItemTags = GetWishlistItemTags(wishlistItem, tagsDictionary);
                if (wishlistItem.StoreItem.BestPurchaseOption != null)
                {
                    DateTime? activeDiscountEndDate = null;
                    var formattedActiveDiscountEndDate = string.Empty;
                    var activeDiscount = wishlistItem.StoreItem.BestPurchaseOption.ActiveDiscounts.FirstOrDefault();
                    if (activeDiscount != null)
                    {
                        activeDiscountEndDate = DateTimeOffset.FromUnixTimeSeconds(activeDiscount.DiscountEndDate).DateTime.ToLocalTime();
                        formattedActiveDiscountEndDate = activeDiscountEndDate.Value.ToString("yyyy/MM/dd");
                    }

                    var item = new SteamWishlistViewItem
                    (
                        wishlistItem.StoreItem.Name,
                        wishlistItem.Appid,
                        wishlistItem.Priority,
                        wishlistItemTags,
                        DateTimeOffset.FromUnixTimeSeconds(wishlistItem.DateAdded).DateTime.ToLocalTime().ToString("yyyy/M/d"),
                        wishlistItem.StoreItem.Release?.SteamReleaseDate != null ? DateTimeOffset.FromUnixTimeSeconds(wishlistItem.StoreItem.Release.SteamReleaseDate).DateTime.ToLocalTime().ToString("yyyy/MM/dd") : string.Empty,
                        wishlistItem.StoreItem.IsEarlyAccess,
                        wishlistItem.StoreItem.BestPurchaseOption.DiscountPct,
                        wishlistItem.StoreItem.BestPurchaseOption.FormattedFinalPrice,
                        wishlistItem.StoreItem.BestPurchaseOption.FormattedOriginalPrice,
                        wishlistItem.StoreItem.BestPurchaseOption.FinalPriceInCents / 100,
                        wishlistItem.StoreItem.BestPurchaseOption.OriginalPriceInCents / 100,
                        string.Join(", ", otherSourcesOwnership.ContainsKey(wishlistItem.StoreItem.Name.Satinize()) ? otherSourcesOwnership[wishlistItem.StoreItem.Name.Satinize()] : new List<string>()),
                        bannersPathsMapper.ContainsKey(wishlistItem.Appid) ? bannersPathsMapper[wishlistItem.Appid] : string.Empty,
                        (SteamStoreItemAppType)Enum.ToObject(typeof(SteamStoreItemAppType), wishlistItem.StoreItem.Type),
                        wishlistItem.StoreItem.Reviews?.SummaryFiltered.ReviewScoreLabel ?? string.Empty,
                        activeDiscountEndDate,
                        formattedActiveDiscountEndDate
                    );

                    items.Add(item);
                }
                else
                {
                    var item = new SteamWishlistViewItem
                    (
                        wishlistItem.StoreItem.Name,
                        wishlistItem.Appid,
                        wishlistItem.Priority,
                        wishlistItemTags,
                        DateTimeOffset.FromUnixTimeSeconds(wishlistItem.DateAdded).DateTime.ToLocalTime().ToString("yyyy/M/d"),
                        wishlistItem.StoreItem.Release?.SteamReleaseDate != null ? DateTimeOffset.FromUnixTimeSeconds(wishlistItem.StoreItem.Release.SteamReleaseDate).DateTime.ToLocalTime().ToString("yyyy/M/d") : string.Empty,
                        wishlistItem.StoreItem.IsEarlyAccess,
                        0,
                        string.Empty,
                        string.Empty,
                        0,
                        0,
                        string.Join(", ", otherSourcesOwnership.ContainsKey(wishlistItem.StoreItem.Name.Satinize()) ? otherSourcesOwnership[wishlistItem.StoreItem.Name.Satinize()] : new List<string>()),
                        bannersPathsMapper.ContainsKey(wishlistItem.Appid) ? bannersPathsMapper[wishlistItem.Appid] : string.Empty,
                        (SteamStoreItemAppType)Enum.ToObject(typeof(SteamStoreItemAppType), wishlistItem.StoreItem.ItemType),
                        wishlistItem.StoreItem.Reviews?.SummaryFiltered.ReviewScoreLabel ?? string.Empty,
                        null,
                        string.Empty
                    );

                    items.Add(item);
                }
            }

            return items;
        }

        private Dictionary<string, List<string>> GetNonSteamOwnedItems()
        {
            var defaultSource = "Playnite";
            return _playniteApi.Database.Games
                .AsParallel()
                .Where(game => !Steam.IsGameSteamGame(game))
                .GroupBy(game => game.Name.Satinize())
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(game => game.Source?.Name.IsNullOrEmpty() == false ? game.Source.Name : defaultSource).ToList()
                );
        }

        #endregion

        #region Private Methods

        // Event Handlers
        private void OnTagFiltersSettingsChanged(object sender, EventArgs e)
        {
            _wishlistCollectionView.Refresh();
        }

        // Wishlist Management
        private void RefreshWishlistView()
        {
            _wishlistCollectionView?.Refresh();
        }

        private void UpdateWishlistSorting()
        {
            using (_wishlistCollectionView.DeferRefresh())
            {
                _wishlistCollectionView.SortDescriptions.Clear();
                _wishlistCollectionView.SortDescriptions.Add(new SortDescription(GetSortingPropertyName(), _selectedSortingDirection));
            }
        }

        private void RemoveWishlistItem(SteamWishlistViewItem cacheItem)
        {
            var success = false;
            _playniteApi.Dialogs.ActivateGlobalProgress(async (a) =>
            {
                // TODO Move to Steam Wishlist service
                var request = HttpRequestFactory.GetHttpRequest()
                    .WithUrl("https://store.steampowered.com/api/removefromwishlist")
                    .WithCookies(new Dictionary<string, string> { { "sessionid", _steamSessionId }, { "steamLoginSecure", _steamLoginSecure } })
                    .WithPostHttpMethod()
                    .WithContent($"sessionid={_steamSessionId}&appid={cacheItem.Appid}", HttpContentTypes.FormUrlEncoded, Encoding.UTF8);

                var result = await request.DownloadStringAsync(a.CancelToken);
                if (!result.IsSuccess || !Serialization.TryFromJson<WishlistAddRemoveRequestResponseDto>(result.Content, out var response) || !response.Success)
                {
                    _playniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_RemoveFromWishlistFail"), "Steam Wishlist Discount Notifier");
                    return;
                }

                success = true;
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_RemovingFromWishlist")) { Cancelable = true });

            if (success)
            {
                WishlistItemsCollection.Remove(cacheItem);
                _wishlistCollectionView.Refresh();
            }
        }

        private void UpdateSteamCookies()
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
            var cookie = webView
                .GetCookies()
                .FirstOrDefault(x => x.Domain == domain && x.Name == cookieName);
            return cookie?.Value;
        }

        private void OpenWishlistItemInSteamClient(SteamWishlistViewItem wishlistItem)
        {
            var subIdSteamUrl = string.Format(_steamStoreSubUrlMask, wishlistItem.Appid);
            ProcessStarter.StartUrl(string.Format(_steamUriOpenUrlMask, subIdSteamUrl));
        }

        private void OpenWishlistItemInBrowser(SteamWishlistViewItem wishlistItem)
        {
            var subIdSteamUrl = string.Format(_steamStoreSubUrlMask, wishlistItem.Appid);
            ProcessStarter.StartUrl(subIdSteamUrl);
        }

        private void OpenWishlistItemInSteamDb(SteamWishlistViewItem wishlistItem)
        {
            var subIdSteamUrl = string.Format("https://steamdb.info/app/{0}/", wishlistItem.Appid);
            ProcessStarter.StartUrl(subIdSteamUrl);
        }

        private ICollectionView InitializeWishlistCollectionView(IEnumerable<SteamWishlistViewItem> wishlistItems)
        {
            var collectionView = CollectionViewSource.GetDefaultView(wishlistItems);
            collectionView.Filter = FilterWishlistItems;
            return collectionView;
        }

        private bool FilterWishlistItems(object item)
        {
            var wishlistItem = item as SteamWishlistViewItem;
            if (_filterOnlyDiscounted)
            {
                if (wishlistItem.DiscountPct == 0 || wishlistItem.DiscountPct < FilterMinimumDiscount)
                {
                    return false;
                }
            }

            if (wishlistItem.FinalPrice != 0)
            {
                if (!_filterIncludeReleased)
                {
                    return false;
                }
            }
            else if (!_filterIncludeNotReleased)
            {
                return false;
            }

            if (FilterMinimumPrice != 0)
            {
                if (wishlistItem.FinalPrice <= FilterMinimumPrice)
                {
                    return false;
                }
            }

            if (FilterMaximumPrice != 999999)
            {
                if (wishlistItem.FinalPrice > FilterMaximumPrice)
                {
                    return false;
                }
            }

            if (_selectedOwnershipType == Ownership.Owned && wishlistItem.FormattedOwnedSources.IsNullOrEmpty())
            {
                return false;
            }
            else if (_selectedOwnershipType == Ownership.NotOwned && !wishlistItem.FormattedOwnedSources.IsNullOrEmpty())
            {
                return false;
            }

            if (!DoesTextMatchSearchString(wishlistItem.Name))
            {
                return false;
            }

            if (!IsWishlistItemTypeFilterEnabled(wishlistItem.SteamStoreItemType))
            {
                return false;
            }

            if (TagFilters.EnabledFiltersNames.Any(x => !wishlistItem.Tags.Contains(x)))
            {
                return false;
            }

            return true;
        }

        private bool DoesTextMatchSearchString(string toMatch)
        {
            if (_searchString.IsNullOrWhiteSpace())
            {
                return true;
            }

            if (!_searchString.IsNullOrWhiteSpace() && toMatch.IsNullOrWhiteSpace())
            {
                return false;
            }

            if (_searchString.IsNullOrWhiteSpace() && toMatch.IsNullOrWhiteSpace())
            {
                return true;
            }

            if (_searchString.GetJaroWinklerSimilarityIgnoreCase(toMatch) >= 0.95)
            {
                return true;
            }

            if (!_searchString.MatchesAllWords(toMatch))
            {
                return false;
            }

            return true;
        }

        private bool IsWishlistItemTypeFilterEnabled(SteamStoreItemAppType itemType)
        {
            switch (itemType)
            {
                case SteamStoreItemAppType.Game:
                    return FilterItemTypeGame;
                case SteamStoreItemAppType.DLC:
                    return FilterItemTypeDlc;
                case SteamStoreItemAppType.Music:
                    return FilterItemTypeMusic;
                case SteamStoreItemAppType.Software:
                    return FilterItemTypeApplication;
                case SteamStoreItemAppType.Video:
                case SteamStoreItemAppType.Series:
                    return FilterItemTypeVideo;
                case SteamStoreItemAppType.Hardware:
                    return FilterItemTypeHardware;
                case SteamStoreItemAppType.Mod:
                case SteamStoreItemAppType.Demo:
                    return FilterItemTypeGame;
                default:
                    return true;
            }
        }

        private string GetSortingPropertyName()
        {
            switch (_selectedSortingType)
            {
                case WishlistViewSorting.Name:
                    return nameof(SteamWishlistViewItem.Name);
                case WishlistViewSorting.Rank:
                    return nameof(SteamWishlistViewItem.Priority);
                case WishlistViewSorting.Discount:
                    return nameof(SteamWishlistViewItem.DiscountPct);
                case WishlistViewSorting.Price:
                    return nameof(SteamWishlistViewItem.FinalPrice);
                case WishlistViewSorting.ReleaseDate:
                    return nameof(SteamWishlistViewItem.FormattedReleaseDate);
                case WishlistViewSorting.Added:
                    return nameof(SteamWishlistViewItem.FormattedDateAdded);
                default:
                    return nameof(SteamWishlistViewItem.Name);
            }
        }

        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            if (TagFilters != null)
            {
                TagFilters.SettingsChanged -= OnTagFiltersSettingsChanged;
                TagFilters.Dispose();
            }
        }
        #endregion
    }

}