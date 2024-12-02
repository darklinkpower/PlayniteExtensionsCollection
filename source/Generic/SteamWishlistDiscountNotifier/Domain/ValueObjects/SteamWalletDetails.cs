using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Domain.ValueObjects
{
    public class SteamWalletDetails
    {
        public bool HasWallet { get; }
        public string UserCountryCode { get; }
        public string WalletCountryCode { get; }
        public string WalletState { get; }
        public long Balance { get; }
        public long DelayedBalance { get; }
        public long CurrencyCode { get; }
        public long TimeMostRecentTxn { get; }
        public string MostRecentTxnid { get; }
        public bool HasWalletInOtherRegions { get; }
        public string FormattedBalance { get; }
        public string FormattedDelayedBalance { get; }

        public SteamWalletDetails(
            bool hasWallet,
            string userCountryCode,
            string walletCountryCode,
            string walletState,
            long balance,
            long delayedBalance,
            long currencyCode,
            long timeMostRecentTxn,
            string mostRecentTxnid,
            bool hasWalletInOtherRegions,
            string formattedBalance,
            string formattedDelayedBalance)
        {
            HasWallet = hasWallet;
            UserCountryCode = userCountryCode;
            WalletCountryCode = walletCountryCode;
            WalletState = walletState;
            Balance = balance;
            DelayedBalance = delayedBalance;
            CurrencyCode = currencyCode;
            TimeMostRecentTxn = timeMostRecentTxn;
            MostRecentTxnid = mostRecentTxnid;
            HasWalletInOtherRegions = hasWalletInOtherRegions;
            FormattedBalance = formattedBalance;
            FormattedDelayedBalance = formattedDelayedBalance;
        }
    }
}