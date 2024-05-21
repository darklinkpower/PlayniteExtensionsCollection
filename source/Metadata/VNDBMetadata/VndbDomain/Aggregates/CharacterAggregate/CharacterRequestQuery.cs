using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.ImageAggregate;
using VNDBMetadata.VndbDomain.Aggregates.ReleaseAggregate;
using VNDBMetadata.VndbDomain.Aggregates.TraitAggregate;
using VNDBMetadata.VndbDomain.Aggregates.VnAggregate;
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
        public VnRequestFieldsFlags VnRequestFieldsFlags;
        [JsonIgnore]
        public ReleaseRequestFieldsFlags VnReleaseRequestFieldsFlags;
        [JsonIgnore]
        public ImageRequestFieldsFlags ImageRequestFieldsFlags;
        [JsonIgnore]
        public CharacterRequestSortEnum Sort = CharacterRequestSortEnum.SearchRank;

        public CharacterRequestQuery(SimpleFilterBase<Character> filter) : base(filter)
        {
            EnableAllFieldsFlags();
        }

        public CharacterRequestQuery(ComplexFilterBase<Character> filter) : base(filter)
        {
            EnableAllFieldsFlags();
        }

        public override void EnableAllFieldsFlags()
        {
            EnumUtilities.SetAllEnumFlags(ref FieldsFlags);
            EnumUtilities.SetAllEnumFlags(ref TraitRequestFieldsFlags);
            EnumUtilities.SetAllEnumFlags(ref VnRequestFieldsFlags);
            EnumUtilities.SetAllEnumFlags(ref VnReleaseRequestFieldsFlags);
            EnumUtilities.SetAllEnumFlags(ref ImageRequestFieldsFlags);
        }

        public override void ResetAllFieldsFlags()
        {
            FieldsFlags = default;
            TraitRequestFieldsFlags = default;

            ImageRequestFieldsFlags = default;
            VnRequestFieldsFlags = default;
            VnReleaseRequestFieldsFlags = default;

            ImageRequestFieldsFlags = default;
        }

        protected override List<string> GetEnabledFields()
        {
            var results = new List<List<string>>
            {
                EnumUtilities.GetStringRepresentations(FieldsFlags),

                EnumUtilities.GetStringRepresentations(TraitRequestFieldsFlags, CharacterConstants.Fields.TraitsAllFields),
                EnumUtilities.GetStringRepresentations(VnRequestFieldsFlags, CharacterConstants.Fields.VnsAllFields),
                EnumUtilities.GetStringRepresentations(VnReleaseRequestFieldsFlags, CharacterConstants.Fields.VnsReleaseAllFields),

                // thumbnail and thumbnail_dims not available because character images are currently always limited to 256x300px
                EnumUtilities.GetStringRepresentations(ImageRequestFieldsFlags, CharacterConstants.Fields.ImageAllFields)
                    .Where(s => !s.EndsWith("thumbnail") && !s.EndsWith("thumbnail_dims")).ToList()
            };

            return results.SelectMany(x => x).ToList();
        }

        protected override string GetSortString()
        {
            if (Filters is SimpleFilterBase<Character> simpleFilter)
            {
                if (Sort == CharacterRequestSortEnum.SearchRank)
                {
                    if (simpleFilter.Name != CharacterFilterFactory.Search.FilterName)
                    {
                        return null;
                    }
                }
            }
            else if (Filters is ComplexFilterBase<Character> complexFilter)
            {
                var simplePredicates = complexFilter.Filters.OfType<SimpleFilterBase<Character>>();
                if (Sort == CharacterRequestSortEnum.SearchRank)
                {
                    var searchPredicatesCount = simplePredicates.Count(x => x.Name == CharacterFilterFactory.Search.FilterName);
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