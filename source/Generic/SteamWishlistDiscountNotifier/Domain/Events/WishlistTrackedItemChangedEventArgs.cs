using SteamWishlistDiscountNotifier.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Domain.Events
{
    public class WishlistTrackedItemChangedEventArgs : EventArgs
    {
        public Guid Id { get; }
        public DateTime CreatedAtUtc { get; }
        public WishlistItemTrackingInfo OldItem { get; }
        public WishlistItemTrackingInfo NewItem { get; }
        public bool PriceChanged { get; set; }
        public bool DiscountChanged { get; set; }
        public bool FinalPriceChanged { get; set; }
        public bool OriginalPriceChanged { get; set; }
        public bool BundleDiscountChanged { get; set; }
        public bool UserDiscountChanged { get; set; }
        public bool UserFinalPriceChanged { get; set; }

        public WishlistTrackedItemChangedEventArgs(
            WishlistItemTrackingInfo oldItem, WishlistItemTrackingInfo newItem, WishlistTrackedItemChanges changeFlags)
        {
            Id = Guid.NewGuid();
            CreatedAtUtc = DateTime.UtcNow;
            OldItem = oldItem;
            NewItem = newItem;
            PriceChanged = changeFlags.PriceChanged;
            DiscountChanged = changeFlags.DiscountChanged;
            FinalPriceChanged = changeFlags.FinalPriceChanged;
            OriginalPriceChanged = changeFlags.OriginalPriceChanged;
            BundleDiscountChanged = changeFlags.BundleDiscountChanged;
            UserDiscountChanged = changeFlags.UserDiscountChanged;
            UserFinalPriceChanged = changeFlags.UserFinalPriceChanged;
        }
    }
}