using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.ProducerAggregate;

namespace VndbApiDomain.ReleaseAggregate
{
    public class ReleaseProducer : Producer
    {
        [JsonProperty("developer")]
        public bool IsDeveloper { get; set; }

        [JsonProperty("publisher")]
        public bool IsPublisher { get; set; }
    }
}