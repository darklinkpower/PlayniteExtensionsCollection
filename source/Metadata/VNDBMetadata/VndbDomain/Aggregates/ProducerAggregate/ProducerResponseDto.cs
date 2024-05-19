using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.VndbDomain.Aggregates.ProducerAggregate
{
    public class ProducerResponseDto
    {
        [JsonProperty("more")]
        public bool More { get; set; }

        [JsonProperty("results")]
        public Producer[] Results { get; set; }
    }
}