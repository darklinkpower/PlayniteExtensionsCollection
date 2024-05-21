using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.StaffAggregate;
using VNDBMetadata.VndbDomain.Common.Converters;

namespace VNDBMetadata.VndbDomain.Aggregates.VnAggregate
{
    public class VnStaff : Staff
    {
        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("eid")]
        public long? Eid { get; set; }

        [JsonProperty("role")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<StaffRoleEnum>))]
        public StaffRoleEnum Role { get; set; }
    }
}