﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.TagAggregate;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.VisualNovelAggregate
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