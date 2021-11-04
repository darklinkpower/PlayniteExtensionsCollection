using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.Models
{
    public class GoogleImage
    {
        [JsonProperty("ow")]
        public uint Width { get; set; }

        [JsonProperty("oh")]
        public uint Height { get; set; }

        [JsonProperty("ou")]
        public string ImageUrl { get; set; }

        [JsonProperty("tu")]
        public string ThumbUrl { get; set; }

        public string Size => $"{Width}x{Height}";
    }
}
