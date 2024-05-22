using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.VnAggregate;
using VNDBMetadata.VndbDomain.Common.Filters;
using VNDBMetadata.VndbDomain.Common.Flags;
using VNDBMetadata.VndbDomain.Common.Interfaces;
using VNDBMetadata.VndbDomain.Common.Models;
using VNDBMetadata.VndbDomain.Common.Queries;
using VNDBMetadata.VndbDomain.Common.Utilities;

namespace VNDBMetadata.VndbDomain.Aggregates.TraitAggregate
{

    public class TraitRequestFields : RequestFieldAbstractBase, IVndbRequestFields
    {
        public TraitRequestFieldsFlags Flags = TraitRequestFieldsFlags.Id | TraitRequestFieldsFlags.Name;

        public void EnableAllFlags(bool enableSubfields)
        {
            EnumUtilities.SetAllEnumFlags(ref Flags);
        }

        public void DisableAllFlags(bool disableSubfields)
        {
            Flags = default;
        }

        public override List<string> GetFlagsStringRepresentations(params string[] prefixParts)
        {
            var prefix = GetFullPrefixString(prefixParts);
            return EnumUtilities.GetStringRepresentations(Flags, prefix);
        }
    }

    public class TraitRequestQuery : RequestQueryBase
    {
        [JsonIgnore]
        public TraitRequestFields Fields = new TraitRequestFields();
        [JsonIgnore]
        public TraitRequestSortEnum Sort = TraitRequestSortEnum.SearchRank;

        public TraitRequestQuery(SimpleFilterBase<Trait> filter) : base(filter)
        {

        }

        public TraitRequestQuery(ComplexFilterBase<Trait> filter) : base(filter)
        {

        }

        protected override List<string> GetEnabledFields()
        {
            return Fields.GetFlagsStringRepresentations();
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