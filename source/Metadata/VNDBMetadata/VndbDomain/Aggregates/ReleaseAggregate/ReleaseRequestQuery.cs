using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.ProducerAggregate;
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
        public ReleaseRequestSortEnum Sort = ReleaseRequestSortEnum.Id;
        // Missing vns.*

        public ReleaseRequestQuery(SimpleFilterBase<Release> filter) : base(filter)
        {
            Initialize();
        }

        public ReleaseRequestQuery(ComplexFilterBase<Release> filter) : base(filter)
        {
            Initialize();
        }

        private void Initialize()
        {
            foreach (ReleaseRequestFieldsFlags field in Enum.GetValues(typeof(ReleaseRequestFieldsFlags)))
            {
                FieldsFlags |= field;
            }

            foreach (ProducerRequestFieldsFlags field in Enum.GetValues(typeof(ProducerRequestFieldsFlags)))
            {
                ProducerRequestFieldsFlags |= field;
            }
        }

        public override List<string> GetEnabledFields()
        {
            return EnumUtilities.GetStringRepresentations(FieldsFlags)
                .Concat(EnumUtilities.GetStringRepresentations(ProducerRequestFieldsFlags, "producers."))
                .ToList();
        }

        public override string GetSortString()
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