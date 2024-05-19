using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Filters;
using VNDBMetadata.VndbDomain.Common.Flags;
using VNDBMetadata.VndbDomain.Common.Queries;
using VNDBMetadata.VndbDomain.Common.Utilities;

namespace VNDBMetadata.VndbDomain.Aggregates.TagAggregate
{
    public class TagRequestQuery : RequestQueryBase
    {
        [JsonIgnore]
        public TagRequestFieldsFlags FieldsFlags;
        [JsonIgnore]
        public TagRequestSortEnum Sort = TagRequestSortEnum.Id;

        public TagRequestQuery(TagFilter filter) : base(filter)
        {
            Initialize();
        }

        public TagRequestQuery(TagComplexFilter filter) : base(filter)
        {
            Initialize();
        }

        private void Initialize()
        {
            foreach (TagRequestFieldsFlags field in Enum.GetValues(typeof(TagRequestFieldsFlags)))
            {
                FieldsFlags |= field;
            }
        }

        public override List<string> GetEnabledFields()
        {
            return EnumUtils.GetStringRepresentations(FieldsFlags);
        }

        public override string GetSortString()
        {
            if (Filters is SimpleFilterBase simpleFilter)
            {
                if (Sort == TagRequestSortEnum.SearchRank)
                {
                    if (simpleFilter.Name != TagFilterFactory.Search.FilterName)
                    {
                        return null;
                    }
                }
            }
            else if (Filters is ComplexFilterBase complexFilter)
            {
                var simplePredicates = complexFilter.Filters.OfType<SimpleFilterBase>();
                if (Sort == TagRequestSortEnum.SearchRank)
                {
                    var searchPredicatesCount = simplePredicates.Count(x => x.Name == TagFilterFactory.Search.FilterName);
                    if (searchPredicatesCount != 1)
                    {
                        return null;
                    }
                }
            }

            return EnumUtils.GetStringRepresentation(Sort);
        }
    }
}