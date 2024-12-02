using ProtoBuf;
using SteamWishlistDiscountNotifier.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Infrastructure.WishlistTracker
{
    [ProtoContract]
    public class WishlistTrackerData
    {
        [ProtoMember(1)]
        public DateTime? LastCheckTime { get; set; }

        [ProtoMember(2)]
        public Dictionary<uint, WishlistItemTrackingInfo> WishlistItems { get; set; } = new Dictionary<uint, WishlistItemTrackingInfo>();
    }
}