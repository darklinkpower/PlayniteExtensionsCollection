using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace ExtraMetadataLoader.Models
{
    public partial class SteamGridDbGameSearchResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public List<SgdbData> Data { get; set; }
    }

    public partial class SgdbData
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