using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Aggregates.Product
{
    public class ChannelPricing
    {
        [SerializationPropertyName("@type")]
        public string Type { get; set; }

        [SerializationPropertyName("channelCode")]
        public string ChannelCode { get; set; }

        [SerializationPropertyName("price")]
        public int Price { get; set; }

        [SerializationPropertyName("originalPrice")]
        public int? OriginalPrice { get; set; }
    }
}