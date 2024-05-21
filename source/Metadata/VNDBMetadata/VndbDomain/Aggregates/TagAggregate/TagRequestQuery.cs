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
        public TagRequestSortEnum Sort = TagRequestSortEnum.SearchRank;

        public TagRequestQuery(SimpleFilterBase<VndbTag> filter) : base(filter)
        {
            EnableAllFieldsFlags();
        }

        public TagRequestQuery(ComplexFilterBase<VndbTag> filter) : base(filter)
        {
            EnableAllFieldsFlags();
        }

        public override void EnableAllFieldsFlags()
        {
            EnumUtilities.SetAllEnumFlags(ref FieldsFlags);
        }

        public override void ResetAllFieldsFlags()
        {
            FieldsFlags = default;
        }

        protected override List<string> GetEnabledFields()
        {
            return EnumUtilities.GetStringRepresentations(FieldsFlags);
        }

        protected override string GetSortString()
        {
            if (Filters is SimpleFilterBase<VndbTag> simpleFilter)
            {
                if (Sort == TagRequestSortEnum.SearchRank)
                {
                    if (simpleFilter.Name != TagFilterFactory.Search.FilterName)
                    {
                        return null;
                    }
                }
            }
            else if (Filters is ComplexFilterBase<VndbTag> complexFilter)
            {
                var simplePredicates = complexFilter.Filters.OfType<SimpleFilterBase<VndbTag>>();
                if (Sort == TagRequestSortEnum.SearchRank)
                {
                    var searchPredicatesCount = simplePredicates.Count(x => x.Name == TagFilterFactory.Search.FilterName);
                    if (searchPredicatesCount != 1)
                    {
                        return null;
                    }
                }
            }

            return EnumUtilities.GetEnumStringRepresentation(Sort);
        }
    }
}