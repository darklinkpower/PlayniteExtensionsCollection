using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Aggregates.Product
{
    public class BonusPoints
    {
        [SerializationPropertyName("value")]
        public long Value { get; set; }

        [SerializationPropertyName("amount")]
        public long Amount { get; set; }

        [SerializationPropertyName("currencyCode")]
        public string CurrencyCode { get; set; }
    }
}