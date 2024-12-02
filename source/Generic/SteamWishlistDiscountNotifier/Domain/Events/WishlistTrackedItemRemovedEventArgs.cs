using SteamWishlistDiscountNotifier.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Domain.Events
{
    public class WishlistTrackedItemRemovedEventArgs : EventArgs
    {
        public Guid Id { get; }
        public DateTime CreatedAtUtc { get; }
        public WishlistItemTrackingInfo Item { get; }
        public WishlistTrackedItemRemovedEventArgs(WishlistItemTrackingInfo item)
        {
            Id = Guid.NewGuid();
            CreatedAtUtc = DateTime.UtcNow;
            Item = item;
        }
    }
}