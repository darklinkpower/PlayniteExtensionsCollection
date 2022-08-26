using SteamWishlistDiscountNotifier.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Models
{
    public class SteamAccountInfo : ObservableObject
    {
        private string username;
        public string Username { get => username; private set => SetValue(ref username, value); }

        private string steamId;
        public string SteamId { get => steamId; private set => SetValue(ref steamId, value); }

        private AuthStatus authStatus;
        public AuthStatus AuthStatus { get => authStatus; private set => SetValue(ref authStatus, value); }

        private string walletString;
        public string WalletString { get => walletString; private set => SetValue(ref walletString, value); }

        private double walletAmount = 0;
        public double WalletAmount { get => walletAmount; private set => SetValue(ref walletAmount, value); }

        public SteamAccountInfo(string username, string steamId, AuthStatus authStatus, string walletString)
        {
            Username = username;
            SteamId = steamId;
            AuthStatus = authStatus;
            WalletString = walletString;
            PriceStringParser.GetPriceValues(walletString, out var _, out var parsedAmount);
            if (parsedAmount.HasValue)
            {
                WalletAmount = (double)parsedAmount;
            }
        }
    }
}