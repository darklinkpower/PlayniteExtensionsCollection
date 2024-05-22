using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Filters;
using VNDBMetadata.VndbDomain.Common.Flags;
using VNDBMetadata.VndbDomain.Common.Interfaces;
using VNDBMetadata.VndbDomain.Common.Models;
using VNDBMetadata.VndbDomain.Common.Queries;
using VNDBMetadata.VndbDomain.Common.Utilities;

namespace VNDBMetadata.VndbDomain.Aggregates.TagAggregate
{
    public class TagRequestFields : RequestFieldAbstractBase, IVndbRequestFields
    {
        public TagRequestFieldsFlags Flags = TagRequestFieldsFlags.Id | TagRequestFieldsFlags.Name | TagRequestFieldsFlags.Category;

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

    public class TagRequestQuery : RequestQueryBase
    {
        [JsonIgnore]
        public TagRequestFields Fields = new TagRequestFields();
        [JsonIgnore]
        public TagRequestSortEnum Sort = TagRequestSortEnum.SearchRank;

        public TagRequestQuery(SimpleFilterBase<VndbTag> filter) : base(filter)
        {

        }

        public TagRequestQuery(ComplexFilterBase<VndbTag> filter) : base(filter)
        {

        }

        protected override List<string> GetEnabledFields()
        {
            return Fields.GetFlagsStringRepresentations();
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