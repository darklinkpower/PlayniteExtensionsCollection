using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Enums
{
    public enum AuthStatus
    {
        Ok,
        Checking,
        AuthRequired,
        PrivateAccount,
        Failed,
        NoConnection
    }
}
