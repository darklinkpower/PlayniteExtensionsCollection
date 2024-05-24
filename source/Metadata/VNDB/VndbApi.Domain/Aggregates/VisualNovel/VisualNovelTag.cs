using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.TagAggregate;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Domain.VisualNovelAggregate
{
    public class VisualNovelTag : Tag
    {
        [JsonProperty("lie")]
        public bool Lie { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("spoiler")]
        [JsonConverter(typeof(IntRepresentationEnumConverter<SpoilerLevelEnum>))]
        public SpoilerLevelEnum Spoiler { get; set; }
    }
}