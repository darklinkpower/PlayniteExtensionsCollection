using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Application.Steam.UserAccount
{
    public class GetClientWalletDetailsResponseDto
    {
        [SerializationPropertyName("response")]
        public SteamWalletDetailsDto Response { get; set; }
    }

    public class SteamWalletDetailsDto
    {
        [SerializationPropertyName("has_wallet")]
        public bool HasWallet { get; set; }

        [SerializationPropertyName("user_country_code")]
        public string UserCountryCode { get; set; }

        [SerializationPropertyName("wallet_country_code")]
        public string WalletCountryCode { get; set; }

        [SerializationPropertyName("wallet_state")]
        public string WalletState { get; set; }

        [SerializationPropertyName("balance")]
        public long Balance { get; set; }

        [SerializationPropertyName("delayed_balance")]
        public long DelayedBalance { get; set; }

        [SerializationPropertyName("currency_code")]
        public long CurrencyCode { get; set; }

        [SerializationPropertyName("time_most_recent_txn")]
        public long TimeMostRecentTxn { get; set; }

        [SerializationPropertyName("most_recent_txnid")]
        public string MostRecentTxnid { get; set; }

        [SerializationPropertyName("has_wallet_in_other_regions")]
        public bool HasWalletInOtherRegions { get; set; }

        [SerializationPropertyName("formatted_balance")]
        public string FormattedBalance { get; set; }

        [SerializationPropertyName("formatted_delayed_balance")]
        public string FormattedDelayedBalance { get; set; }
    }
}
