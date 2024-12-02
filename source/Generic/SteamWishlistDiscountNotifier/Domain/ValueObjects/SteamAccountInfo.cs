using SteamWishlistDiscountNotifier.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Domain.ValueObjects
{
    public class SteamAccountInfo : ObservableObject
    {
        public string Username { get; }
        public string SteamId { get; }
        public AuthStatus AuthStatus { get; }
        public string WalletString { get; }
        public double WalletAmount { get; }

        public SteamAccountInfo(string username, string steamId, AuthStatus authStatus, string walletString)
        {
            Username = username;
            SteamId = steamId;
            AuthStatus = authStatus;
            WalletString = walletString;
            WalletAmount = 0;
        }
    }
}