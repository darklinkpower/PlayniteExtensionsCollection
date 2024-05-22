using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Converters;

namespace VNDBFuze.VndbDomain.Aggregates.VnAggregate
{
    public class VnRelation : Vn
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