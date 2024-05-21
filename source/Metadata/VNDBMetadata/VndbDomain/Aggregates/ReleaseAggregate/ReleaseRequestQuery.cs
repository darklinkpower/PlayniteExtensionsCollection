using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.ProducerAggregate;
using VNDBMetadata.VndbDomain.Aggregates.VnAggregate;
using VNDBMetadata.VndbDomain.Common.Filters;
using VNDBMetadata.VndbDomain.Common.Queries;
using VNDBMetadata.VndbDomain.Common.Utilities;

namespace VNDBMetadata.VndbDomain.Aggregates.ReleaseAggregate
{
    public class ReleaseRequestQuery : RequestQueryBase
    {
        [JsonIgnore]
        public ReleaseRequestFieldsFlags FieldsFlags;
        [JsonIgnore]
        public ProducerRequestFieldsFlags ProducerRequestFieldsFlags;

        [JsonIgnore]
        public VnRequestFieldsFlags VnRequestFieldsFlags;

        [JsonIgnore]
        public ReleaseRequestSortEnum Sort = ReleaseRequestSortEnum.SearchRank;

        public ReleaseRequestQuery(SimpleFilterBase<Release> filter) : base(filter)
        {
            EnableAllFieldsFlags();
        }

        public ReleaseRequestQuery(ComplexFilterBase<Release> filter) : base(filter)
        {
            EnableAllFieldsFlags();
        }

        public override void EnableAllFieldsFlags()
        {
            EnumUtilities.SetAllEnumFlags(ref FieldsFlags);
            EnumUtilities.SetAllEnumFlags(ref VnRequestFieldsFlags);
            EnumUtilities.SetAllEnumFlags(ref ProducerRequestFieldsFlags);
        }

        public override void ResetAllFieldsFlags()
        {
            FieldsFlags = default;
            VnRequestFieldsFlags = default;
            ProducerRequestFieldsFlags = default;
        }

        protected override List<string> GetEnabledFields()
        {
            var results = new List<List<string>>
            {
                EnumUtilities.GetStringRepresentations(FieldsFlags),
                EnumUtilities.GetStringRepresentations(VnRequestFieldsFlags, ReleaseConstants.Fields.VnsAll),
                EnumUtilities.GetStringRepresentations(ProducerRequestFieldsFlags, ReleaseConstants.Fields.ProducersAll)
            };

            return results.SelectMany(x => x).ToList();
        }

        protected override string GetSortString()
        {
            if (Filters is SimpleFilterBase<Release> simpleFilter)
            {
                if (Sort == ReleaseRequestSortEnum.SearchRank)
                {
                    if (simpleFilter.Name != ReleaseFilterFactory.Search.FilterName)
                    {
                        return null;
                    }
                }
            }
            else if (Filters is ComplexFilterBase<Release> complexFilter)
            {
                var simplePredicates = complexFilter.Filters.OfType<SimpleFilterBase<Release>>();
                if (Sort == ReleaseRequestSortEnum.SearchRank)
                {
                    var searchPredicatesCount = simplePredicates.Count(x => x.Name == ReleaseFilterFactory.Search.FilterName);
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