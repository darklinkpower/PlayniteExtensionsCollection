using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Application.Steam.JwtToken
{
    public enum TokenRetrievalStatus
    {
        Success,
        NoInternet,
        NotLoggedIn,
        UnexpectedNavigation,
        ParseFailure
    }

}
