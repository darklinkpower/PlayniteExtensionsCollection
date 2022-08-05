using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseDateImporter.Models
{
    public class EpicGetOrderHistoryResponse
    {
        [SerializationPropertyName("orders")]
        public List<EpicOrder> Orders { get; set; }

        [SerializationPropertyName("count")]
        public long Count { get; set; }

        [SerializationPropertyName("start")]
        public long Start { get; set; }

        [SerializationPropertyName("total")]
        public long Total { get; set; }
    }

    public class EpicOrder
    {
        [SerializationPropertyName("orderStatus")]
        public string OrderStatus { get; set; }

        [SerializationPropertyName("canSendReceipt")]
        public bool CanSendReceipt { get; set; }

        [SerializationPropertyName("receiptId")]
        public string ReceiptId { get; set; }

        [SerializationPropertyName("orderId")]
        public string OrderId { get; set; }

        [SerializationPropertyName("createdAtMillis")]
        public long CreatedAtMillis { get; set; }

        [SerializationPropertyName("updatedAtMillis")]
        public long UpdatedAtMillis { get; set; }

        [SerializationPropertyName("parentOrderId")]
        public object ParentOrderId { get; set; }

        [SerializationPropertyName("currency")]
        public string Currency { get; set; }

        [SerializationPropertyName("price")]
        public long Price { get; set; }

        [SerializationPropertyName("presentmentAmount")]
        public string PresentmentAmount { get; set; }

        [SerializationPropertyName("items")]
        public EpicOrderItem[] Items { get; set; }

        [SerializationPropertyName("merchantGroup")]
        public string MerchantGroup { get; set; }

        [SerializationPropertyName("total")]
        public long Total { get; set; }

        [SerializationPropertyName("convenienceFee")]
        public long ConvenienceFee { get; set; }

        [SerializationPropertyName("tax")]
        public long Tax { get; set; }

        [SerializationPropertyName("presentmentTotal")]
        public string PresentmentTotal { get; set; }

        [SerializationPropertyName("presentmentConvenienceFee")]
        public string PresentmentConvenienceFee { get; set; }

        [SerializationPropertyName("presentmentTax")]
        public string PresentmentTax { get; set; }
    }

    public class EpicOrderItem
    {
        [SerializationPropertyName("description")]
        public string Description { get; set; }

        [SerializationPropertyName("quantity")]
        public long Quantity { get; set; }

        [SerializationPropertyName("sellerName")]
        public string SellerName { get; set; }

        [SerializationPropertyName("amount")]
        public string Amount { get; set; }

        [SerializationPropertyName("price")]
        public long Price { get; set; }

        [SerializationPropertyName("offerId")]
        public string OfferId { get; set; }

        [SerializationPropertyName("namespace")]
        public string Namespace { get; set; }

        [SerializationPropertyName("status")]
        public string Status { get; set; }
    }
}