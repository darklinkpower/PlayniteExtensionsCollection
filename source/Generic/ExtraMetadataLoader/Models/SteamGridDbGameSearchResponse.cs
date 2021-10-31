using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace ExtraMetadataLoader.Models
{
    class SteamGridDbGameSearchResponse
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
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("release_date", NullValueHandling = NullValueHandling.Ignore)]
            public long? ReleaseDate { get; set; }

            [JsonProperty("verified")]
            public bool Verified { get; set; }

            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("types")]
            public string[] Types { get; set; }
        }
    }
}