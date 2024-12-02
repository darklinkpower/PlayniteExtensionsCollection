using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using PluginsCommon;
using FlowHttp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using SteamCommon;
using SteamWishlistDiscountNotifier.Application.Steam.Wishlist;
using SteamWishlistDiscountNotifier.Application.Steam.JwtToken;
using SteamWishlistDiscountNotifier.Application.Steam.UserAccount;
using SteamWishlistDiscountNotifier.Domain.ValueObjects;
using SteamWishlistDiscountNotifier.Application.WishlistTracker;
using SteamWishlistDiscountNotifier.Infrastructure.WishlistTracker;
using SteamWishlistDiscountNotifier.Domain.Events;
using SteamWishlistDiscountNotifier.Domain.Enums;
using SteamWishlistDiscountNotifier.Presentation;
using SteamWishlistDiscountNotifier.Application.Steam.Login;

namespace SteamWishlistDiscountNotifier
{
    public class SteamWishlistDiscountNotifier : GenericPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly int currentDatabaseVersion = 2;
        private const string _steamStoreUrlMask = @"https://store.steampowered.com/app/{0}/";
        private const string _steamUriOpenUrlMask = @"steam://openurl/{0}";
        private const string _notLoggedInNotifId = @"Steam_Wishlist_Notif_AuthRequired";
        private readonly SteamLoginService _steamLoginService;
        private readonly SteamJwtTokenService _steamJwtTokenService;
        private readonly SteamUserAccountService _steamUserAccountService;
        private readonly SteamWishlistService _steamWishlistService;
        private readonly WishlistTrackerService _wishlistTrackerService;
        private SteamWishlistViewerViewModel _wishlistViewDataContext = null;

        private readonly SteamWishlistDiscountNotifierSettingsViewModel _settings;

        public override Guid Id { get; } = Guid.Parse("d5825a82-42bf-426b-ac47-5bea5df7aede");

        public SteamWishlistDiscountNotifier(IPlayniteAPI api) : base(api)
        {
            _steamLoginService = new SteamLoginService(PlayniteApi, _logger);
            _steamJwtTokenService = new SteamJwtTokenService(PlayniteApi, _logger);
            _steamUserAccountService = new SteamUserAccountService(_steamJwtTokenService, PlayniteApi, _logger);
            _steamWishlistService = new SteamWishlistService(_steamJwtTokenService, PlayniteApi, _logger);
            _wishlistTrackerService = new WishlistTrackerService
            (
                _logger,
                new WishlistTrackerPersistenceService(Path.Combine(GetPluginUserDataPath(), "WishlistTrackerPersistence"), _logger),
                _steamUserAccountService,
                _steamWishlistService
            );

            _settings = new SteamWishlistDiscountNotifierSettingsViewModel(this, _steamLoginService);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            _wishlistTrackerService.WishlistTrackedItemAdded += WishlistTrackerService_WishlistTrackedItemAdded;
            _wishlistTrackerService.WishlistTrackedItemRemoved += WishlistTrackerService_WishlistTrackedItemRemoved;
            _wishlistTrackerService.WishlistTrackedItemChanged += WishlistTrackerService_WishlistTrackedItemChanged;
            _settings.SettingsChanged += Settings_SettingsChanged;
            _steamJwtTokenService.AddUserLoggedInCallback(() =>
            {
                PlayniteApi.Notifications.Remove(_notLoggedInNotifId);
            });

            _steamJwtTokenService.AddUserNotLoggedInCallback(() =>
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    _notLoggedInNotifId,
                    ResourceProvider.GetString("LOCSteam_Wishlist_Notif_WishlistCheckNotLoggedIn"),
                    NotificationType.Info,
                    () => OpenSettingsView()
                ));
            });

            UpdateTracketServiceConfig(_settings.Settings);
        }

        private void Settings_SettingsChanged(object sender, SettingsChangedEventArgs e)
        {
            UpdateTracketServiceConfig(e.NewSettings);
        }

        private void UpdateTracketServiceConfig(SteamWishlistDiscountNotifierSettings settings)
        {
            _wishlistTrackerService.SetBackgroundServiceTrackingDelay(TimeSpan.FromMinutes(settings.WishlistAutoCheckIntervalMins));
            if (settings.EnableWishlistNotifications)
            {
                _wishlistTrackerService.EnableBackgroundServiceTracker();
            }
            else
            {
                _wishlistTrackerService.DisableBackgroundServiceTracker();
            }
        }

        private void WishlistTrackerService_WishlistTrackedItemAdded(object sender, WishlistTrackedItemAddedEventArgs e)
        {
            var newWishlistItem = e.Item;
            if (newWishlistItem.DiscountPct == 0 || !GetShouldDisplayNotification(newWishlistItem))
            {
                return;
            }

            var notificationLines = new List<string>
            {
                string.Format(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_GameOnSaleLabel"), newWishlistItem.Name) + "\n",
                string.Format(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_DiscountPercent"), newWishlistItem.DiscountPct ?? 0),
                string.Format("{0} -> {1}",
                              newWishlistItem.FormattedOriginalPrice ?? "?", 
                              newWishlistItem.FormattedFinalPrice ?? "?")
            };

            var finalNotificationMessageText = string.Join("\n", notificationLines);
            var notificationMessage = new NotificationMessage(
                e.Id.ToString(),
                finalNotificationMessageText,
                NotificationType.Info,
                () => OpenDiscountedItemUrl(newWishlistItem.AppId)
            );

            PlayniteApi.Notifications.Add(notificationMessage);
        }

        private void WishlistTrackerService_WishlistTrackedItemRemoved(object sender, WishlistTrackedItemRemovedEventArgs e)
        {

        }

        private void WishlistTrackerService_WishlistTrackedItemChanged(object sender, WishlistTrackedItemChangedEventArgs e)
        {
            if (!GetShouldDisplayNotification(e.NewItem))
            {
                return;
            }

            var oldItem = e.OldItem;
            var newItem = e.NewItem;
            if (!oldItem.FinalPriceInCents.HasValue && newItem.FinalPriceInCents.HasValue && newItem.FinalPriceInCents > 0)
            {
                // New item in the store
                if (!_settings.Settings.EnableNewReleasesNotifications && newItem.DiscountPct == 0)
                {
                    return;
                }

                var notificationLines = new List<string>
                {
                    string.Format(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_GameNewAvailableLabel"), newItem.Name) + "\n"
                };

                if (newItem.DiscountPct.HasValue && newItem.DiscountPct > 0)
                {
                    // New release with discount
                    notificationLines.Add(string.Format(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_DiscountPercent"), newItem.DiscountPct));
                    notificationLines.Add(string.Format("{0} -> {0} ", newItem.FormattedOriginalPrice, newItem.FormattedFinalPrice));
                }
                else
                {
                    // New release without discount
                    notificationLines.Add(newItem.FormattedFinalPrice);
                }

                PlayniteApi.Notifications.Add(new NotificationMessage(
                    e.Id.ToString(),
                    string.Join("\n", notificationLines),
                    NotificationType.Info,
                    () => OpenDiscountedItemUrl(newItem.AppId)
                ));
            }
            else if (newItem.DiscountPct.HasValue && newItem.DiscountPct > 0)
            {
                // Item now on discount
                var notificationLines = new List<string>
                {
                    string.Format(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_GameOnSaleLabel"), newItem.Name) + "\n",
                    string.Format(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_DiscountPercent"), newItem.DiscountPct),
                    string.Format("{0} -> {1}", oldItem.FormattedFinalPrice, newItem.FormattedFinalPrice)
                };

                PlayniteApi.Notifications.Add(new NotificationMessage(
                    e.Id.ToString(),
                    string.Join("\n", notificationLines),
                    NotificationType.Info,
                    () => OpenDiscountedItemUrl(newItem.AppId)
                ));
            }
            else if (_settings.Settings.EnablePriceChangesNotifications && oldItem.FinalPriceInCents != newItem.FinalPriceInCents)
            {
                // Price has changed
                var notificationLines = new List<string>
                {
                    string.Format(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_GamePriceChangedLabel"), newItem.Name) + "\n",
                    string.Format("{0} -> {1}", oldItem.FormattedOriginalPrice, newItem.FormattedOriginalPrice)
                };

                PlayniteApi.Notifications.Add(new NotificationMessage(
                    e.Id.ToString(),
                    string.Join("\n", notificationLines),
                    NotificationType.Info,
                    () => OpenDiscountedItemUrl(newItem.AppId)
                ));
            }
        }

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            yield return new SidebarItem
            {
                Title = ResourceProvider.GetString("LOCSteam_Wishlist_Notif_SidebarTitle"),
                Type = SiderbarItemType.View,
                Icon = new TextBlock
                {
                    Text = "\u0041",
                    FontFamily = new FontFamily(new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "IconsFont.ttf")), "./#IconsFont")
                },
                Opened = () => {
                    return GetSteamWishlistViewerSidebarView();
                },
                Closed = () => {
                    _wishlistViewDataContext?.Dispose();
                    _wishlistViewDataContext = null;
                },
            };
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSteam_Wishlist_Notif_MenuItemStartWishlistDiscountCheckDescription"),
                    MenuSection = "@Steam Wishlist Discount Notifier",
                    Action = x => {
                        PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
                        {
                            _wishlistTrackerService.CheckForChangesInTrackingItems(a.CancelToken);
                        }, new GlobalProgressOptions(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_ObtainingWishlistMessage"), true));
                    }
                }
            };
        }

        private SteamWishlistViewerView GetSteamWishlistViewerSidebarView()
        {
            _wishlistViewDataContext?.Dispose();
            _wishlistViewDataContext = null;

            var wishlistItems = new List<CWishlistGetWishlistSortedFilteredResponseWishlistItem>();
            SteamWalletDetails walletDetails = null;
            Dictionary<uint, string> bannersPathsMapper = null;
            var tokenWasCancelled = false;
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                walletDetails =_steamUserAccountService.GetClientWalletDetails();
                if (walletDetails != null)
                {
                    wishlistItems = _steamWishlistService.GetWishlist(walletDetails.UserCountryCode, cancellationToken: a.CancelToken);
                    if (wishlistItems.HasItems())
                    {
                        bannersPathsMapper = GetBannerPaths(wishlistItems, a);
                    }
                }

                tokenWasCancelled = a.CancelToken.IsCancellationRequested;
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_ObtainingWishlistMessage"), true));

            if (!wishlistItems.HasItems() || tokenWasCancelled)
            {
                return null;
            }

            _wishlistViewDataContext = new SteamWishlistViewerViewModel(PlayniteApi, walletDetails, wishlistItems, bannersPathsMapper, GetPluginUserDataPath());
            return new SteamWishlistViewerView { DataContext = _wishlistViewDataContext };
        }

        private Dictionary<uint, string> GetBannerPaths(List<CWishlistGetWishlistSortedFilteredResponseWishlistItem> wishlistItems, GlobalProgressActionArgs a)
        {
            var dict = new Dictionary<uint, string>();
            if (wishlistItems is null)
            {
                return dict;
            }

            var bannerImagesCachePath = Path.Combine(GetPluginUserDataPath(), "BannerImages");
            foreach (var wishlistItem in wishlistItems)
            {
                if (a.CancelToken.IsCancellationRequested)
                {
                    return dict;
                }

                var localBannerPath = Path.Combine(bannerImagesCachePath, wishlistItem.Appid + ".jpg");
                if (FileSystem.FileExists(localBannerPath))
                {
                    dict[wishlistItem.Appid] = localBannerPath;
                }
                else
                {
                    if (wishlistItem.StoreItem.Assets is null
                        || wishlistItem.StoreItem.Assets.AssetUrlFormat.IsNullOrEmpty()
                        || wishlistItem.StoreItem.Assets.Header.IsNullOrEmpty())
                    {
                        continue;
                    }

                    // https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/2725260/header.jpg?t=1725930041
                    // asseturlformat = steam/apps/24242424/${FILENAME}?T=1725930041
                    // header = header.jpg
                    var assetPath = wishlistItem.StoreItem.Assets.AssetUrlFormat.Replace("${FILENAME}", wishlistItem.StoreItem.Assets.Header);
                    var headerUrl = $"https://shared.cloudflare.steamstatic.com/store_item_assets/{assetPath}";
                    var request = HttpRequestFactory.GetHttpFileRequest()
                        .WithUrl(headerUrl)
                        .WithDownloadTo(localBannerPath);

                    var result = request.DownloadFile(a.CancelToken);
                    if (result.IsSuccess)
                    {
                        dict[wishlistItem.Appid] = localBannerPath;
                    }
                }
            }

            return dict;
        }

        private Dictionary<string, List<string>> GetNonSteamOwnedItems()
        {
            var defaultSource = "Playnite";
            return PlayniteApi.Database.Games
                .AsParallel()
                .Where(game => !Steam.IsGameSteamGame(game))
                .GroupBy(game => game.Name.Satinize())
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(game => game.Source?.Name.IsNullOrEmpty() == false ? game.Source.Name : defaultSource).ToList()
                );
        }

        private bool GetShouldDisplayNotification(WishlistItemTrackingInfo item)
        {
            switch (item.ItemType)
            {
                case SteamStoreItemAppType.Game:
                    return _settings.Settings.NotifyDiscountsTypeGame;
                case SteamStoreItemAppType.DLC:
                    return _settings.Settings.NotifyDiscountsTypeDlc;
                case SteamStoreItemAppType.Music:
                    return _settings.Settings.NotifyDiscountsTypeMusic;
                case SteamStoreItemAppType.Software:
                    return _settings.Settings.NotifyDiscountsTypeApplication;
                case SteamStoreItemAppType.Video:
                    return _settings.Settings.NotifyDiscountsTypeVideo;
                case SteamStoreItemAppType.Series:
                    return _settings.Settings.NotifyDiscountsTypeVideo;
                case SteamStoreItemAppType.Hardware:
                    return _settings.Settings.NotifyDiscountsTypeHardware;
                case SteamStoreItemAppType.Mod:
                    return _settings.Settings.NotifyDiscountsTypeMod;
                default:
                    return true;
            }
        }

        private void OpenDiscountedItemUrl(uint steamStoreId)
        {
            var subIdSteamUrl = string.Format(_steamStoreUrlMask, steamStoreId);
            if (_settings.Settings.OpenUrlsInBrowser)
            {
                ProcessStarter.StartUrl(subIdSteamUrl);
            }
            else
            {
                ProcessStarter.StartUrl(string.Format(_steamUriOpenUrlMask, subIdSteamUrl));
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return _settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SteamWishlistDiscountNotifierSettingsView();
        }
    }
}