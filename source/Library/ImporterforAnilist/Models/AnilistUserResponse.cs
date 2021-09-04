using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImporterforAnilist.Models
{
    public partial class AnilistUser
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("Viewer")]
        public Viewer Viewer { get; set; }
    }

    public partial class Viewer
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("options")]
        public Options Options { get; set; }

        [JsonProperty("mediaListOptions")]
        public MediaListOptions MediaListOptions { get; set; }
    }

    public partial class MediaListOptions
    {
        [JsonProperty("scoreFormat")]
        public string ScoreFormat { get; set; }
    }

    public partial class Options
    {
        [JsonProperty("displayAdultContent")]
        public bool DisplayAdultContent { get; set; }
    }
}
