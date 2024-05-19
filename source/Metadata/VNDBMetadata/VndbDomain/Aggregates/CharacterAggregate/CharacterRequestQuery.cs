using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.TraitAggregate;
using VNDBMetadata.VndbDomain.Common.Filters;
using VNDBMetadata.VndbDomain.Common.Flags;
using VNDBMetadata.VndbDomain.Common.Queries;
using VNDBMetadata.VndbDomain.Common.Utilities;

namespace VNDBMetadata.VndbDomain.Aggregates.CharacterAggregate
{
    public class CharacterRequestQuery : RequestQueryBase
    {
        [JsonIgnore]
        public CharacterRequestFieldsFlags FieldsFlags;
        [JsonIgnore]
        public TraitRequestFieldsFlags TraitRequestFieldsFlags;
        [JsonIgnore]
        public CharacterRequestSortEnum Sort = CharacterRequestSortEnum.Id;

        // vns.* All visual novel fields are available here.
        // vns.release.* Object, usually null, specific release that this character appears in. All release fields are available here.

        public CharacterRequestQuery(CharacterFilter filter) : base(filter)
        {
            Initialize();
        }

        public CharacterRequestQuery(CharacterComplexFilter filter) : base(filter)
        {
            Initialize();
        }

        private void Initialize()
        {
            foreach (CharacterRequestFieldsFlags field in Enum.GetValues(typeof(CharacterRequestFieldsFlags)))
            {
                FieldsFlags |= field;
            }

            foreach (TraitRequestFieldsFlags field in Enum.GetValues(typeof(TraitRequestFieldsFlags)))
            {
                TraitRequestFieldsFlags |= field;
            }
        }

        public override List<string> GetEnabledFields()
        {
            return EnumUtils.GetStringRepresentations(FieldsFlags)
                .Concat(EnumUtils.GetStringRepresentations(TraitRequestFieldsFlags, "traits."))
                .ToList();
        }

        public override string GetSortString()
        {
            if (Filters is SimpleFilterBase simpleFilter)
            {
                if (Sort == CharacterRequestSortEnum.SearchRank)
                {
                    if (simpleFilter.Name != CharacterFilterFactory.Search.FilterName)
                    {
                        return null;
                    }
                }
            }
            else if (Filters is ComplexFilterBase complexFilter)
            {
                var simplePredicates = complexFilter.Filters.OfType<SimpleFilterBase>();
                if (Sort == CharacterRequestSortEnum.SearchRank)
                {
                    var searchPredicatesCount = simplePredicates.Count(x => x.Name == CharacterFilterFactory.Search.FilterName);
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