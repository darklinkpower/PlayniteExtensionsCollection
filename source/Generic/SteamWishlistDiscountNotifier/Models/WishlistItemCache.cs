using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Models
{
    public class WishlistItemCache : IEquatable<WishlistItemCache>
    {
        public string Name { get; set; }
        public string StoreId { get; set; }
        public double? SubId { get; set; }
        public double? PriceOriginal { get; set; }
        public double? PriceFinal { get; set; }
        public string Currency { get; set; }
        public double DiscountPercent { get; set; }
        public bool IsDiscounted { get; set; }
        public SteamWishlistItem WishlistItem { get; set; }
        public string BannerImagePath { get; set; } = null;

        public bool Equals(WishlistItemCache other)
        {
            if (other is null)
            {
                return false;
            }

            if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(StoreId, other.StoreId, StringComparison.Ordinal))
            {
                return false;
            }

            if (SubId != other.SubId)
            {
                return false;
            }

            if (PriceOriginal != other.PriceOriginal)
            {
                return false;
            }

            if (PriceFinal != other.PriceFinal)
            {
                return false;
            }

            if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
            {
                return false;
            }

            if (DiscountPercent != other.DiscountPercent)
            {
                return false;
            }

            if (IsDiscounted != other.IsDiscounted)
            {
                return false;
            }

            return true;
        }
    }
}