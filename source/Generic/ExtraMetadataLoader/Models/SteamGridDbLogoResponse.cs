using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.Models
{
    class SteamGridDbLogoResponse
    {
        public partial class Response
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("data")]
            public List<Data> Data { get; set; }
        }

        public partial class Data
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("score")]
            public long Score { get; set; }

            [JsonProperty("style")]
            public string Style { get; set; }

            [JsonProperty("width")]
            public long Width { get; set; }

            [JsonProperty("height")]
            public long Height { get; set; }

            [JsonProperty("nsfw")]
            public bool Nsfw { get; set; }

            [JsonProperty("humor")]
            public bool Humor { get; set; }

            [JsonProperty("notes")]
            public object Notes { get; set; }

            [JsonProperty("mime")]
            public string Mime { get; set; }

            [JsonProperty("language")]
            public string Language { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("thumb")]
            public string Thumb { get; set; }

            [JsonProperty("lock")]
            public bool Lock { get; set; }

            [JsonProperty("epilepsy")]
            public bool Epilepsy { get; set; }

            [JsonProperty("upvotes")]
            public long Upvotes { get; set; }

            [JsonProperty("downvotes")]
            public long Downvotes { get; set; }

            [JsonProperty("author")]
            public Author Author { get; set; }
        }

        public partial class Author
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("steam64")]
            public string Steam64 { get; set; }

            [JsonProperty("avatar")]
            public Uri Avatar { get; set; }
        }
    }
}
