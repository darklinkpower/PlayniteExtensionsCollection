using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImporterforAnilist.Models
{
    public partial class AnilistUser
    {
        [SerializationPropertyName("data")]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [SerializationPropertyName("Viewer")]
        public Viewer Viewer { get; set; }
    }

    public partial class Viewer
    {
        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("id")]
        public long Id { get; set; }

        [SerializationPropertyName("options")]
        public Options Options { get; set; }

        [SerializationPropertyName("mediaListOptions")]
        public MediaListOptions MediaListOptions { get; set; }
    }

    public partial class MediaListOptions
    {
        [SerializationPropertyName("scoreFormat")]
        public string ScoreFormat { get; set; }
    }

    public partial class Options
    {
        [SerializationPropertyName("displayAdultContent")]
        public bool DisplayAdultContent { get; set; }
    }
}