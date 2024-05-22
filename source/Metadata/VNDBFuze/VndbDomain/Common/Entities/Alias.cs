using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBFuze.VndbDomain.Common.Entities
{
    public class Alias<T>
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("latin")]
        public string Latin { get; set; }

        [JsonProperty("aid")]
        public long Aid { get; set; }

        [JsonProperty("ismain")]
        public bool Ismain { get; set; }
    }
}
