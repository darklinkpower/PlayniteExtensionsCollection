using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Interfaces;

namespace VNDBMetadata.VndbDomain.Common.Queries
{
    public abstract class RequestQueryBase
    {
        [JsonProperty("fields")]
        public string FieldsString => string.Join(",", GetEnabledFields());
        [JsonProperty("sort")]
        public string SortString => GetSortString();

        [JsonProperty("filters")]
        [JsonConverter(typeof(PredicateConverter))]
        public readonly IFilter Filters;

        [JsonProperty("reverse")]
        public bool Reverse = false;

        [JsonProperty("results")]
        public uint Results = 10;

        [JsonProperty("page")]
        public uint Page = 1;

        [JsonProperty("user")]
        public string User = null;

        [JsonProperty("count")]
        public bool Count = false;

        [JsonProperty("compact_filters")]
        public bool CompactFilters = false;

        [JsonProperty("normalized_filters")]
        public bool NormalizedFilters = false;

        public RequestQueryBase(IFilter filter)
        {
            Filters = filter;
        }

        protected abstract List<string> GetEnabledFields();

        protected abstract string GetSortString();

        public class PredicateConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(IFilter).IsAssignableFrom(objectType);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value is IFilter predicate)
                {
                    writer.WriteRawValue(predicate.ToJsonString());
                }
                else
                {
                    throw new JsonSerializationException("Expected IPredicate object value");
                }
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Deserialization not supported");
            }
        }
    }
}