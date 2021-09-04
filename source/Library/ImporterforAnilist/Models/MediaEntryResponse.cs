using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ImporterforAnilist.Models
{
    public partial class MediaEntryData
    {
        [JsonProperty("data")]
        public MediaEntryMedia Data { get; set; }
    }

    public partial class MediaEntryMedia
    {
        [JsonProperty("Media")]
        public Media Media { get; set; }
    }
}
