using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Application.Steam.Wishlist
{
    public class WishlistAddRemoveRequestResponseDto
    {
        [SerializationPropertyName("success")]
        public bool Success { get; set; }

        [SerializationPropertyName("wishlistCount")]
        public int WishlistCount { get; set; }
    }
}