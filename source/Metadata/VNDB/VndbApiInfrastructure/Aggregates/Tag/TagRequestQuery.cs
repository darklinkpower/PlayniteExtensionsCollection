using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.TagAggregate;
using VndbApiInfrastructure.SharedKernel;
using VndbApiInfrastructure.SharedKernel.Filters;
using VndbApiInfrastructure.SharedKernel.Requests;

namespace VndbApiInfrastructure.TagAggregate
{
    public class TagRequestFields : RequestFieldAbstractBase, IRequestFields
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

        public TagRequestQuery(SimpleFilterBase<Tag> filter) : base(filter)
        {

        }

        public TagRequestQuery(ComplexFilterBase<Tag> filter) : base(filter)
        {

        }

        protected override List<string> GetEnabledFields()
        {
            return Fields.GetFlagsStringRepresentations();
        }

        protected override string GetSortString()
        {
            if (Filters is SimpleFilterBase<Tag> simpleFilter)
            {
                if (Sort == TagRequestSortEnum.SearchRank)
                {
                    if (simpleFilter.Name != TagFilterFactory.Search.FilterName)
                    {
                        return null;
                    }
                }
            }
            else if (Filters is ComplexFilterBase<Tag> complexFilter)
            {
                var simplePredicates = complexFilter.Filters.OfType<SimpleFilterBase<Tag>>();
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