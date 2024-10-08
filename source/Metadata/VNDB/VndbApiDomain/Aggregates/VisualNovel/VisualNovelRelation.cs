﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.VisualNovelAggregate
{
    public class VisualNovelRelation : VisualNovel
    {
        [JsonProperty("relation")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<VnRelationTypeEnum>))]
        public VnRelationTypeEnum Relation { get; set; }

        [JsonProperty("relation_official")]
        public bool RelationOfficial { get; set; }

        public override string ToString()
        {
            return $"Visual novel: {base.ToString()}, Relation: {Relation}, Official: {RelationOfficial}";
        }

    }
}