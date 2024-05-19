using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.VndbDomain.Common.Models
{
    public class VndbDatabaseQueryReponse<T>
    {
        [JsonProperty("more")]
        public bool More { get; set; }

        [JsonProperty("results")]
        public List<T> Results { get; set; }
    }
}
