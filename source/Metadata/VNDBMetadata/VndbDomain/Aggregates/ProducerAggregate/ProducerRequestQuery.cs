using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Filters;
using VNDBMetadata.VndbDomain.Common.Queries;
using VNDBMetadata.VndbDomain.Common.Utilities;

namespace VNDBMetadata.VndbDomain.Aggregates.ProducerAggregate
{
    public class ProducerRequestQuery : RequestQueryBase
    {
        [JsonIgnore]
        public ProducerRequestFieldsFlags FieldsFlags;
        [JsonIgnore]
        public ProducerRequestSortEnum Sort = ProducerRequestSortEnum.SearchRank;

        public ProducerRequestQuery(SimpleFilterBase<Producer> filter) : base(filter)
        {
            EnableAllFieldsFlags();
        }

        public ProducerRequestQuery(ComplexFilterBase<Producer> filter) : base(filter)
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
            var results = new List<List<string>>
            {
                EnumUtilities.GetStringRepresentations(FieldsFlags)
            };

            return results.SelectMany(x => x).ToList();
        }

        protected override string GetSortString()
        {
            if (Filters is SimpleFilterBase<Producer> simpleFilter)
            {
                if (Sort == ProducerRequestSortEnum.SearchRank)
                {
                    if (simpleFilter.Name != ProducerFilterFactory.Search.FilterName)
                    {
                        return null;
                    }
                }
            }
            else if (Filters is ComplexFilterBase<Producer> complexFilter)
            {
                var simplePredicates = complexFilter.Filters.OfType<SimpleFilterBase<Producer>>();
                if (Sort == ProducerRequestSortEnum.SearchRank)
                {
                    var searchPredicatesCount = simplePredicates.Count(x => x.Name == ProducerFilterFactory.Search.FilterName);
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