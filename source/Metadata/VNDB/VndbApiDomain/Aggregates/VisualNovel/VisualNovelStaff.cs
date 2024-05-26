using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.StaffAggregate;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.VisualNovelAggregate
{
    public class VisualNovelStaff : Staff
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