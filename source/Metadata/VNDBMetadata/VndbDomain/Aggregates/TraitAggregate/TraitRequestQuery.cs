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

namespace VNDBMetadata.VndbDomain.Aggregates.TraitAggregate
{
    public class TraitRequestQuery : RequestQueryBase
    {
        [JsonIgnore]
        public TraitRequestFieldsFlags FieldsFlags;
        [JsonIgnore]
        public TraitRequestSortEnum Sort = TraitRequestSortEnum.SearchRank;

        public TraitRequestQuery(SimpleFilterBase<Trait> filter) : base(filter)
        {
            EnableAllFieldsFlags();
        }

        public TraitRequestQuery(ComplexFilterBase<Trait> filter) : base(filter)
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
            if (Filters is SimpleFilterBase<Trait> simpleFilter)
            {
                if (Sort == TraitRequestSortEnum.SearchRank)
                {
                    if (simpleFilter.Name != TraitFilterFactory.Search.FilterName)
                    {
                        return null;
                    }
                }
            }
            else if (Filters is ComplexFilterBase<Trait> complexFilter)
            {
                var simplePredicates = complexFilter.Filters.OfType<SimpleFilterBase<Trait>>();
                if (Sort == TraitRequestSortEnum.SearchRank)
                {
                    var searchPredicatesCount = simplePredicates.Count(x => x.Name == TraitFilterFactory.Search.FilterName);
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