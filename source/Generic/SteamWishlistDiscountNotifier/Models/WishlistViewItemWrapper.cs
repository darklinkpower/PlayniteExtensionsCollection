using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Models
{
    public class WishlistCacheItemViewWrapper
    {
        public WishlistItemCache Data { get; private set; }
        public string BannerImagePath { get; private set; }
        public List<string> OwnedSources { get; private set; } = new List<string>();

        public WishlistCacheItemViewWrapper(WishlistItemCache wishlistItem, string bannerImagePath)
        {
            Data = wishlistItem;
            BannerImagePath = bannerImagePath;
        }

        public WishlistCacheItemViewWrapper(WishlistItemCache wishlistItem, string bannerImagePath, List<string> ownedSources)
        {
            Data = wishlistItem;
            BannerImagePath = bannerImagePath;
            OwnedSources = ownedSources;
        }
    }
}