using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon;
using PluginsCommon.Web;
using SteamWishlistDiscountNotifier.Enums;
using SteamWishlistDiscountNotifier.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SteamWishlistDiscountNotifier
{
    public class SteamWishlistDiscountNotifier : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly int currentDatabaseVersion = 1;
        private static readonly Regex discBlockParseRegex = new Regex(@"discount_original_price"">(\S+) ([^<]+).+(?=discount_final_price)[^ ]+ ([^<]+)", RegexOptions.Compiled);
        private const string steamStoreSubUrlMask = @"https://store.steampowered.com/sub/{0}/";
        private const string steamUriOpenUrlMask = @"steam://openurl/{0}";
        private const string steamWishlistUrlMask = @"https://store.steampowered.com/wishlist/profiles/{0}/wishlistdata/?p={1}";
        private const string notLoggedInNotifId = @"Steam_Wishlist_Notif_AuthRequired";
        private string wishlistCachePath;
        public readonly DispatcherTimer wishlistCheckTimer;

        private SteamWishlistDiscountNotifierSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("d5825a82-42bf-426b-ac47-5bea5df7aede");

        public SteamWishlistDiscountNotifier(IPlayniteAPI api) : base(api)
        {
            settings = new SteamWishlistDiscountNotifierSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            wishlistCachePath = Path.Combine(GetPluginUserDataPath(), "WishlistCache.json");
            wishlistCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1),
            };
            wishlistCheckTimer.Tick += new EventHandler(WishlistCheckTimer_Tick);
        }

        private void WishlistCheckTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now >
                 settings.Settings.LastWishlistUpdate.AddMinutes(settings.Settings.WishlistAutoCheckIntervalMins))
            {
                StartWishlistCheck();
            }
        }

        private void StartWishlistCheck()
        {
            Task.Run(() =>
            {
                wishlistCheckTimer.Stop();
                using (var webView = PlayniteApi.WebViews.CreateOffscreenView())
                {
                    SteamLogin.GetLoggedInSteamId64(webView, out var status, out var steamId);
                    logger.Debug($"Started checking for wishlist. Status: {status}, steamId: {steamId}");
                    if (status == AuthStatus.NoConnection)
                    {
                        return;
                    }
                    else if (status == AuthStatus.Ok)
                    {
                        PlayniteApi.Notifications.Remove(notLoggedInNotifId);
                        UpdateAndNotifyWishlistDiscounts(steamId, webView);
                    }
                    else if (status == AuthStatus.AuthRequired)
                    {
                        PlayniteApi.Notifications.Add(new NotificationMessage(
                            notLoggedInNotifId,
                            ResourceProvider.GetString("LOCSteam_Wishlist_Notif_WishlistCheckNotLoggedIn"),
                            NotificationType.Info,
                            () => OpenSettingsView()
                        ));
                    }
                }

                logger.Debug($"Finished checking for wishlist");
                wishlistCheckTimer.Start();
            });
        }

        private int? UpdateAndNotifyWishlistDiscounts(string steamId, IWebView webView)
        {
            var wishlistDiscounts = GetWishlistDiscounts(steamId, webView);
            if (wishlistDiscounts == null)
            {
                return null;
            }

            var wishlistCache = GetWishlistCache();
            var cacheRemovals = 0;
            var cacheAdditions = 0;

            // Check for changes in existing cache
            foreach (var cachedDiscount in wishlistCache.ToList())
            {
                if (wishlistDiscounts.TryGetValue(cachedDiscount.Id, out var newDiscount))
                {
                    if (HasDiscountDataChanged(cachedDiscount, newDiscount))
                    {
                        wishlistCache.Remove(cachedDiscount);
                        cacheRemovals++;
                        wishlistCache.Add(newDiscount);
                        cacheAdditions++;

                        AddNotifyDiscount(newDiscount);
                    }
                }
                else
                {
                    cacheRemovals++;
                    wishlistCache.Remove(cachedDiscount);
                }
            }

            // Check new items in discount
            foreach (var wishlistDiscount in wishlistDiscounts)
            {
                if (!wishlistCache.Any(x => x.Id == wishlistDiscount.Key))
                {
                    wishlistCache.Add(wishlistDiscount.Value);
                    cacheAdditions++;
                    AddNotifyDiscount(wishlistDiscount.Value);
                }
            }

            if (cacheRemovals > 0 || cacheAdditions > 0)
            {
                FileSystem.WriteStringToFile(wishlistCachePath, Serialization.ToJson(wishlistCache));
            }

            settings.Settings.LastWishlistUpdate = DateTime.Now;
            SavePluginSettings(settings.Settings);

            return cacheRemovals + cacheAdditions;
        }

        private void AddNotifyDiscount(DiscountedItemCache newDiscount)
        {
            if (!GetShouldDisplayNotification(newDiscount))
            {
                return;
            }
            
            PlayniteApi.Notifications.Add(new NotificationMessage(
                Guid.NewGuid().ToString(),
                GetDiscountNotificationMessage(newDiscount),
                NotificationType.Info,
                () => OpenDiscountedItemUrl(newDiscount.Id)
            ));
        }

        private bool GetShouldDisplayNotification(DiscountedItemCache newDiscount)
        {
            if (settings.Settings.NotificationMinDiscount > newDiscount.DiscountPercent)
            {
                return false;
            }

            switch (newDiscount.WishlistItem.Type)
            {
                case TypeEnum.Game:
                    return settings.Settings.NotifyDiscountsTypeGame;
                case TypeEnum.Dlc:
                    return settings.Settings.NotifyDiscountsTypeDlc;
                case TypeEnum.Music:
                    return settings.Settings.NotifyDiscountsTypeMusic;
                case TypeEnum.Application:
                    return settings.Settings.NotifyDiscountsTypeApplication;
                case TypeEnum.Video:
                    return settings.Settings.NotifyDiscountsTypeVideo;
                case TypeEnum.Series:
                    return settings.Settings.NotifyDiscountsTypeVideo;
                case TypeEnum.Hardware:
                    return settings.Settings.NotifyDiscountsTypeHardware;
                case TypeEnum.Mod:
                    return settings.Settings.NotifyDiscountsTypeMod;
                default:
                    return true;
            }
        }

        private void OpenDiscountedItemUrl(double subId)
        {
            var subIdSteamUrl = string.Format(steamStoreSubUrlMask, subId);
            if (settings.Settings.OpenUrlsInBrowser)
            {
                ProcessStarter.StartUrl(subIdSteamUrl);
            }
            else
            {
                ProcessStarter.StartUrl(string.Format(steamUriOpenUrlMask, subIdSteamUrl));
            }
        }

        private string GetDiscountNotificationMessage(DiscountedItemCache newDiscount)
        {
            return string.Join("\n", new string[3]
            {
                string.Format(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_GameOnSaleLabel"), newDiscount.Name) + "\n",
                string.Format(ResourceProvider.GetString("LOCSteam_Wishlist_Notif_DiscountPercent"), newDiscount.DiscountPercent),
                string.Format("{0} {1} -> {0} {2}", newDiscount.Currency, newDiscount.PriceOriginal.ToString("0.00"), newDiscount.PriceFinal.ToString("0.00"))
            });
        }

        private bool HasDiscountDataChanged(DiscountedItemCache cachedDiscount, DiscountedItemCache newDiscount)
        {
            if (cachedDiscount.PriceFinal != newDiscount.PriceFinal ||
                cachedDiscount.Currency != newDiscount.Currency ||
                cachedDiscount.DiscountPercent != newDiscount.DiscountPercent)
            {
                return true;
            }

            return false;
        }

        private List<DiscountedItemCache> GetWishlistCache()
        {
            if (FileSystem.FileExists(wishlistCachePath))
            {
                return Serialization.FromJsonFile<List<DiscountedItemCache>>(wishlistCachePath);
            }

            return new List<DiscountedItemCache>();
        }

        private Dictionary<double, DiscountedItemCache> GetWishlistDiscounts(string steamId, IWebView webView)
        {
            var currentPage = 0;
            var currentDiscountedItems = new Dictionary<double, DiscountedItemCache>();
            while (true)
            {
                var url = string.Format(steamWishlistUrlMask, steamId, currentPage);
                webView.NavigateAndWait(url);
                var pageSource = webView.GetPageSource();
                pageSource = HttpUtility.HtmlDecode(pageSource);
                var startIndex = pageSource.IndexOf('{');
                var endIndex = pageSource.LastIndexOf('}'); ;
                if (startIndex == -1 || endIndex == -1)
                {
                    logger.Debug($"Wishlist check finished in {url}");
                    break;
                }

                pageSource = pageSource.Substring(startIndex, endIndex - startIndex + 1);
                if (pageSource.IsNullOrEmpty() || pageSource == "[]")
                {
                    break;
                }

                // Success 2 means that the logged account doesn't have permissions to check the
                // wishlist. Check in case logged account has changed in the period between obtaining
                // the steamId and getting the wishlist
                if (pageSource == @"{""success"":2}")
                {
                    logger.Debug($"Page {url}, Sucess 2");
                    return null;
                }

                var response = Serialization.FromJson<Dictionary<string, SteamWishlistResponse>>(pageSource);
                foreach (var wishlistItem in response.Values)
                {
                    foreach (var sub in wishlistItem.Subs)
                    {
                        if (sub.DiscountPct == 0)
                        {
                            continue;
                        }

                        var discountedItem = GetDiscountedItemFromSub(wishlistItem, sub);
                        if (discountedItem == null)
                        {
                            continue;
                        }

                        currentDiscountedItems[discountedItem.Id] = discountedItem;
                    }
                }

                currentPage++;
            }

            logger.Debug($"Wishlist check obtained {currentDiscountedItems.Count} items");
            return currentDiscountedItems;
        }

        private DiscountedItemCache GetDiscountedItemFromSub(SteamWishlistResponse wishlistItem, Sub sub)
        {
            var regexMatch = discBlockParseRegex.Match(sub.DiscountBlock);
            if (!regexMatch.Success)
            {
                logger.Warn($"Failed to parse sub discount block: {sub.DiscountBlock}");
                return null;
            }

            return new DiscountedItemCache
            {
                Name = HttpUtility.HtmlDecode(wishlistItem.Name),
                Id = sub.Id,
                PriceOriginal = GetParsedPrice(regexMatch.Groups[2].Value),
                PriceFinal = GetParsedPrice(regexMatch.Groups[3].Value),
                Currency = regexMatch.Groups[1].Value,
                DiscountPercent = sub.DiscountPct,
                WishlistItem = wishlistItem
            };
        }

        private double GetParsedPrice(string str)
        {
            var pointIndex = str.LastIndexOf('.');
            var commaIndex = str.LastIndexOf(',');
            
            if (commaIndex < pointIndex)
            {
                // Point is decimal separator
                return double.Parse(str, CultureInfo.InvariantCulture);
            }
            else
            {
                // Comma is decimal separator
                return double.Parse(str, CultureInfo.GetCultureInfo("es-ES"));
            }
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (settings.Settings.DatabaseVersion < currentDatabaseVersion)
            {
                if (FileSystem.FileExists(wishlistCachePath))
                {
                    FileSystem.DeleteFileSafe(wishlistCachePath);
                }

                settings.Settings.DatabaseVersion = currentDatabaseVersion;
                SavePluginSettings(settings.Settings);
            }
            
            if (!FileSystem.FileExists(wishlistCachePath) || DateTime.Now >
                settings.Settings.LastWishlistUpdate.AddMinutes(settings.Settings.WishlistAutoCheckIntervalMins))
            {
                StartWishlistCheck();
            }
            else
            {
                wishlistCheckTimer.Start();
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SteamWishlistDiscountNotifierSettingsView();
        }
    }
}