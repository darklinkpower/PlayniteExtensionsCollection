using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImporterforAnilist.Models
{
    public partial class MalSyncSiteItem
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("page")]
        public string Page { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("malId")]
        public int? MalId { get; set; }

        [JsonProperty("aniId")]
        public int? AniId { get; set; }
    }
}
