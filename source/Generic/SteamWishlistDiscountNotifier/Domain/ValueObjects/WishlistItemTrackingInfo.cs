using ProtoBuf;
using SteamWishlistDiscountNotifier.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Domain.ValueObjects
{
    [ProtoContract]
    public class WishlistItemTrackingInfo
    {
        [ProtoMember(1)]
        public uint AppId { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public long? PriceInCents { get; set; }

        [ProtoMember(4)]
        public long? FinalPriceInCents { get; set; }

        [ProtoMember(5)]
        public long? OriginalPriceInCents { get; set; }

        [ProtoMember(6)]
        public long? DiscountPct { get; set; }

        [ProtoMember(7)]
        public long? BundleDiscountPct { get; set; }

        [ProtoMember(8)]
        public long? UserDiscountPct { get; set; }

        [ProtoMember(9)]
        public long? UserFinalPriceInCents { get; set; }

        [ProtoMember(10)]
        public string FormattedOriginalPrice { get; set; }

        [ProtoMember(11)]
        public string FormattedFinalPrice { get; set; }

        [ProtoMember(12)]
        public SteamStoreItemAppType ItemType { get; set; }

        public override string ToString()
        {
            return $"{Name} ({AppId})";
        }
    }
}
