using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseDateImporter.Models
{
    public partial class GogOrderResponse
    {
        [SerializationPropertyName("orders")]
        public List<GogOrder> Orders { get; set; }

        [SerializationPropertyName("totalPages")]
        public long TotalPages { get; set; }
    }

    public class GogOrder
    {
        [SerializationPropertyName("publicId")]
        public string PublicId { get; set; }

        [SerializationPropertyName("distributor")]
        public object Distributor { get; set; }

        [SerializationPropertyName("date")]
        public long Date { get; set; }

        [SerializationPropertyName("moneybackGuarantee")]
        public bool MoneybackGuarantee { get; set; }

        [SerializationPropertyName("status")]
        public string Status { get; set; }

        [SerializationPropertyName("paymentMethod")]
        public string PaymentMethod { get; set; }

        [SerializationPropertyName("validUntil")]
        public object ValidUntil { get; set; }

        [SerializationPropertyName("checkoutLink")]
        public string CheckoutLink { get; set; }

        [SerializationPropertyName("receiptLink")]
        public string ReceiptLink { get; set; }

        [SerializationPropertyName("total")]
        public StoreCreditUsed Total { get; set; }

        [SerializationPropertyName("storeCreditUsed")]
        public StoreCreditUsed StoreCreditUsed { get; set; }

        [SerializationPropertyName("giftRecipient")]
        public object GiftRecipient { get; set; }

        [SerializationPropertyName("giftSender")]
        public object GiftSender { get; set; }

        [SerializationPropertyName("products")]
        public Product[] Products { get; set; }

        [SerializationPropertyName("giftCode")]
        public object GiftCode { get; set; }

        [SerializationPropertyName("isResendable")]
        public bool IsResendable { get; set; }

        [SerializationPropertyName("statusPageUrl")]
        public string StatusPageUrl { get; set; }

        [SerializationPropertyName("source")]
        public object Source { get; set; }
    }

    public class Product
    {
        [SerializationPropertyName("status")]
        public object Status { get; set; }

        [SerializationPropertyName("relatedAccount")]
        public object RelatedAccount { get; set; }

        [SerializationPropertyName("price")]
        public Price Price { get; set; }

        [SerializationPropertyName("image")]
        public string Image { get; set; }

        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("id")]
        public string Id { get; set; }

        [SerializationPropertyName("isRefunded")]
        public bool IsRefunded { get; set; }

        [SerializationPropertyName("cashValue")]
        public StoreCreditUsed CashValue { get; set; }

        [SerializationPropertyName("walletValue")]
        public StoreCreditUsed WalletValue { get; set; }

        [SerializationPropertyName("isPreorder")]
        public bool IsPreorder { get; set; }

        [SerializationPropertyName("displayAutomaticRefundLink")]
        public bool DisplayAutomaticRefundLink { get; set; }

        [SerializationPropertyName("refundDate")]
        public object RefundDate { get; set; }

        [SerializationPropertyName("extraInfo")]
        public object[] ExtraInfo { get; set; }
    }

    public partial class StoreCreditUsed
    {
        [SerializationPropertyName("amount")]
        public string Amount { get; set; }

        [SerializationPropertyName("symbol")]
        public string Symbol { get; set; }

        [SerializationPropertyName("code")]
        public string Code { get; set; }

        [SerializationPropertyName("isZero")]
        public bool IsZero { get; set; }

        [SerializationPropertyName("rawAmount")]
        public long RawAmount { get; set; }

        [SerializationPropertyName("formattedAmount")]
        public string FormattedAmount { get; set; }

        [SerializationPropertyName("full")]
        public string Full { get; set; }

        [SerializationPropertyName("for_email")]
        public string ForEmail { get; set; }
    }

    public class Price
    {
        [SerializationPropertyName("baseAmount")]
        public string BaseAmount { get; set; }

        [SerializationPropertyName("amount")]
        public string Amount { get; set; }

        [SerializationPropertyName("isFree")]
        public bool IsFree { get; set; }

        [SerializationPropertyName("isDiscounted")]
        public bool IsDiscounted { get; set; }

        [SerializationPropertyName("symbol")]
        public string Symbol { get; set; }
    }

}