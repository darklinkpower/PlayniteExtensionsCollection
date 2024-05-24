using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApi.Infrastructure.SharedKernel.Responses
{
    public class VndbDatabaseQueryReponse<T>
    {
        [JsonProperty("more")]
        public bool More { get; set; }

        [JsonProperty("results")]
        public List<T> Results { get; set; }
    }
}