using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.Fields;
using VNDBMetadata.Filters;
using VNDBMetadata.Interfaces;
using VNDBMetadata.Models;
using VNDBMetadata.Sort;
using VNDBMetadata.VNDB.Enums;

namespace VNDBMetadata.Queries
{
    public class PostProducerQuery : PostQueryBase
    {
        [JsonIgnore]
        public ProducerFields Fields;
        [JsonIgnore]
        public ProducerSortEnum Sort = ProducerSortEnum.SearchRank;

        public PostProducerQuery(ProducerFilter filter) : base(filter)
        {
            Initialize();
        }

        public PostProducerQuery(ProducerComplexFilter filter) : base(filter)
        {
            Initialize();
        }

        private void Initialize()
        {
            foreach (ProducerFields field in Enum.GetValues(typeof(ProducerFields)))
            {
                Fields |= field;
            }
        }

        public override List<string> GetEnabledFields()
        {
            return EnumUtils.GetStringRepresentations(Fields);
        }

        public override string GetSortString()
        {
            if (Filters is SimpleFilterBase simpleFilter)
            {
                if (Sort == ProducerSortEnum.SearchRank)
                {
                    if (simpleFilter.Name != ProducerFilterFactory.Search.FilterName)
                    {
                        return null;
                    }
                }
            }
            else if (Filters is ComplexFilterBase complexFilter)
            {
                var simplePredicates = complexFilter.Filters.OfType<SimpleFilterBase>();
                if (Sort == ProducerSortEnum.SearchRank)
                {
                    var searchPredicatesCount = simplePredicates.Count(x => x.Name == ProducerFilterFactory.Search.FilterName);
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