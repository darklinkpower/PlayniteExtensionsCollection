using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.VndbDomain.Aggregates.VnAggregate
{
    public class Vn
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}