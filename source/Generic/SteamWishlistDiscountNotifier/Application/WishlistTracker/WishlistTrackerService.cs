using Playnite.SDK;
using SteamWishlistDiscountNotifier.Application.Steam.UserAccount;
using SteamWishlistDiscountNotifier.Application.Steam.Wishlist;
using SteamWishlistDiscountNotifier.Domain.Enums;
using SteamWishlistDiscountNotifier.Domain.Events;
using SteamWishlistDiscountNotifier.Domain.Interfaces;
using SteamWishlistDiscountNotifier.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Application.WishlistTracker
{
    public class WishlistTrackerService
    {
        private readonly ILogger _logger;
        private readonly IWishlistTrackerPersistenceService _persistenceService;
        private readonly SteamUserAccountService _steamUserAccountService;
        private readonly SteamWishlistService _steamWishlistService;
        private TimeSpan _backgroundServiceDelay = TimeSpan.FromMinutes(15);
        private bool _isBackgroundServiceRunning = false;
        private bool _isBackgroundServiceEnabled = false;
        private bool _isDetectingWishlistChanges = false;
        private int _backgroundChecksWithoutSuccess = 0;
        private object _checkForChangesInTrackingItemsLock = new object();

        public event EventHandler<WishlistTrackedItemAddedEventArgs> WishlistTrackedItemAdded;
        public event EventHandler<WishlistTrackedItemRemovedEventArgs> WishlistTrackedItemRemoved;
        public event EventHandler<WishlistTrackedItemChangedEventArgs> WishlistTrackedItemChanged;

        public WishlistTrackerService(
            ILogger logger,
            IWishlistTrackerPersistenceService persistenceService,
            SteamUserAccountService steamUserAccountService,
            SteamWishlistService steamWishlistService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _persistenceService = persistenceService ?? throw new ArgumentNullException(nameof(persistenceService));
            _steamUserAccountService = steamUserAccountService ?? throw new ArgumentNullException(nameof(steamUserAccountService));
            _steamWishlistService = steamWishlistService ?? throw new ArgumentNullException(nameof(steamWishlistService));
        }

        public void SetBackgroundServiceTrackingDelay(TimeSpan delay)
        {
            _backgroundServiceDelay = delay;
        }

        public void EnableBackgroundServiceTracker()
        {
            _isBackgroundServiceEnabled = true;
            StartBackgroundServiceStatusCheckAsync();
        }

        public void DisableBackgroundServiceTracker()
        {
            _isBackgroundServiceEnabled = false;
        }

        private async void StartBackgroundServiceStatusCheckAsync()
        {
            if (_isBackgroundServiceRunning)
            {
                return;
            }

            _isBackgroundServiceRunning = true;
            await Task.Run(async () =>
            {
                while (true)
                {
                    await WaitUntilNextCheckTimeAsync();
                    if (!_isBackgroundServiceEnabled)
                    {
                        break;
                    }

                    var backgroundCheckSuccess = CheckForChangesInTrackingItemsInternal();
                    if (backgroundCheckSuccess)
                    {
                        _backgroundChecksWithoutSuccess = 0;
                        _logger.Info("Wishlist tracking check successful.");
                    }
                    else
                    {
                        _backgroundChecksWithoutSuccess++;
                        _logger.Warn($"Wishlist tracking failed, attempt {_backgroundChecksWithoutSuccess}.");
                    }
                }
            });

            _isBackgroundServiceRunning = false;
        }

        private async Task WaitUntilNextCheckTimeAsync()
        {
            var checkInterval = TimeSpan.FromSeconds(30);
            while (true)
            {
                var lastCheckTime = _persistenceService.GetLastCheckTime();
                if (!lastCheckTime.HasValue)
                {
                    break;
                }

                var timeSinceLastCheck = DateTime.UtcNow - lastCheckTime.Value;
                var delayUntilNextCheck = _backgroundServiceDelay - timeSinceLastCheck;
                if (delayUntilNextCheck > TimeSpan.Zero)
                {
                    await Task.Delay(checkInterval);
                }
                else
                {
                    break;
                }
            }

            if (_backgroundChecksWithoutSuccess >= 5)
            {
                var backoffDelay = TimeSpan.FromMinutes(Math.Min(Math.Pow(2, _backgroundChecksWithoutSuccess - 5), 20));
                _logger.Warn($"Background check failed {_backgroundChecksWithoutSuccess} times consecutively. Applying exponential backoff: waiting {backoffDelay.TotalMinutes} minute(s) before next check.");
                await Task.Delay(backoffDelay);
            }
        }

        public void ClearTrackingItems()
        {
            _persistenceService.ClearItems();
        }

        public bool CheckForChangesInTrackingItems(CancellationToken cancellationToken = default)
        {
            return CheckForChangesInTrackingItemsInternal(cancellationToken);
        }

        private bool CheckForChangesInTrackingItemsInternal(CancellationToken cancellationToken = default)
        {
            lock (_checkForChangesInTrackingItemsLock)
            {
                try
                {
                    _isDetectingWishlistChanges = true;

                    var walletDetails = _steamUserAccountService.GetClientWalletDetails();
                    if (walletDetails is null)
                    {
                        return false;
                    }

                    var wishlistItems = _steamWishlistService.GetWishlist(walletDetails.UserCountryCode, cancellationToken: cancellationToken);
                    if (wishlistItems is null || !wishlistItems.Any() || cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    var obtainedItemsAppIds = wishlistItems.Select(x => x.Appid).ToHashSet();
                    var trackedWishlistItems = GetAllTrackedWishlistItems();
                    var itemsToRemove = trackedWishlistItems
                        .Where(x => !obtainedItemsAppIds.Contains(x.AppId))
                        .ToList();

                    _persistenceService.BeginTransaction();
                    foreach (var item in itemsToRemove)
                    {
                        _persistenceService.RemoveWishlistItem(item);
                        OnWishlistItemRemoved(item);
                    }

                    var trackedWishlistItemsDictionary = trackedWishlistItems
                        .Where(x => obtainedItemsAppIds.Contains(x.AppId))
                        .ToDictionary(x => x.AppId);

                    foreach (var wishlistItem in wishlistItems)
                    {
                        if (wishlistItem.StoreItem.Unlisted || wishlistItem.StoreItem.IsFree)
                        {
                            continue;
                        }

                        var newItem = new WishlistItemTrackingInfo
                        {
                            AppId = wishlistItem.Appid,
                            Name = wishlistItem.StoreItem.Name,

                            PriceInCents = wishlistItem.StoreItem.BestPurchaseOption?.FinalPriceInCents,
                            FinalPriceInCents = wishlistItem.StoreItem.BestPurchaseOption?.FinalPriceInCents,
                            OriginalPriceInCents = wishlistItem.StoreItem.BestPurchaseOption?.OriginalPriceInCents,
                            DiscountPct = wishlistItem.StoreItem.BestPurchaseOption?.DiscountPct,
                            BundleDiscountPct = wishlistItem.StoreItem.BestPurchaseOption?.BundleDiscountPct,
                            UserDiscountPct = wishlistItem.StoreItem.BestPurchaseOption?.UserDiscountPct,
                            UserFinalPriceInCents = wishlistItem.StoreItem.BestPurchaseOption?.UserFinalPriceInCents,
                            FormattedOriginalPrice = wishlistItem.StoreItem.BestPurchaseOption?.FormattedOriginalPrice,
                            FormattedFinalPrice = wishlistItem.StoreItem.BestPurchaseOption?.FormattedFinalPrice,

                            ItemType = (SteamStoreItemAppType)Enum.ToObject(typeof(SteamStoreItemAppType), wishlistItem.StoreItem.Type)
                        };

                        if (trackedWishlistItemsDictionary.TryGetValue(wishlistItem.Appid, out var existingItem))
                        {
                            if (HasItemChanged(existingItem, newItem, out var changeFlags))
                            {
                                _persistenceService.SaveWishlistItem(newItem);
                                OnWishlistItemChanged(existingItem, newItem, changeFlags);
                            }
                        }
                        else
                        {
                            _persistenceService.SaveWishlistItem(newItem);
                            OnWishlistItemAdded(newItem);
                        }
                    }

                    _persistenceService.SetLastCheckTime(DateTime.UtcNow);
                    _persistenceService.CommitTransaction();
                    return true;
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error during CheckForChangesInTrackingItems");
                    _persistenceService.RollbackTransaction();
                }
                finally
                {
                    _isDetectingWishlistChanges = false;
                }

                return false;
            }
        }

        public List<WishlistItemTrackingInfo> GetAllTrackedWishlistItems()
        {
            return _persistenceService.GetAllWishlistItems();
        }

        private bool HasItemChanged(WishlistItemTrackingInfo oldItem, WishlistItemTrackingInfo newItem, out WishlistTrackedItemChanges changeFlags)
        {
            changeFlags = new WishlistTrackedItemChanges
            {
                PriceChanged = oldItem.PriceInCents != newItem.PriceInCents,
                FinalPriceChanged = oldItem.FinalPriceInCents != newItem.FinalPriceInCents,
                OriginalPriceChanged = oldItem.OriginalPriceInCents != newItem.OriginalPriceInCents,
                DiscountChanged = oldItem.DiscountPct != newItem.DiscountPct,
                BundleDiscountChanged = oldItem.BundleDiscountPct != newItem.BundleDiscountPct,
                UserDiscountChanged = oldItem.UserDiscountPct != newItem.UserDiscountPct,
                UserFinalPriceChanged = oldItem.UserFinalPriceInCents != newItem.UserFinalPriceInCents
            };

            return changeFlags.HasAnyChanges();
        }

        private void OnWishlistItemAdded(WishlistItemTrackingInfo item)
        {
            WishlistTrackedItemAdded?.Invoke(this, new WishlistTrackedItemAddedEventArgs(item));
        }

        private void OnWishlistItemRemoved(WishlistItemTrackingInfo item)
        {
            WishlistTrackedItemRemoved?.Invoke(this, new WishlistTrackedItemRemovedEventArgs(item));
        }

        private void OnWishlistItemChanged(WishlistItemTrackingInfo oldItem, WishlistItemTrackingInfo newItem, WishlistTrackedItemChanges changeFlags)
        {
            WishlistTrackedItemChanged?.Invoke(this, new WishlistTrackedItemChangedEventArgs(oldItem, newItem, changeFlags));
        }
    }


}