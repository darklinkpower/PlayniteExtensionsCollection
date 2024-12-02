using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Domain.ValueObjects
{
    public struct WishlistTrackedItemChanges
    {
        public bool PriceChanged { get; set; }
        public bool FinalPriceChanged { get; set; }
        public bool OriginalPriceChanged { get; set; }
        public bool DiscountChanged { get; set; }
        public bool BundleDiscountChanged { get; set; }
        public bool UserDiscountChanged { get; set; }
        public bool UserFinalPriceChanged { get; set; }

        public bool HasAnyChanges() =>
            PriceChanged || FinalPriceChanged || OriginalPriceChanged || DiscountChanged ||
            BundleDiscountChanged || UserDiscountChanged || UserFinalPriceChanged;
    }
}
