using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Aggregates.StaffAggregate;
using VNDBFuze.VndbDomain.Common.Converters;

namespace VNDBFuze.VndbDomain.Aggregates.VnAggregate
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