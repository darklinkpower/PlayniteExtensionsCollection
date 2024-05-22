using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Aggregates.ImageAggregate;
using VNDBFuze.VndbDomain.Aggregates.ReleaseAggregate;
using VNDBFuze.VndbDomain.Aggregates.TraitAggregate;
using VNDBFuze.VndbDomain.Aggregates.VnAggregate;
using VNDBFuze.VndbDomain.Common.Filters;
using VNDBFuze.VndbDomain.Common.Flags;
using VNDBFuze.VndbDomain.Common.Interfaces;
using VNDBFuze.VndbDomain.Common.Models;
using VNDBFuze.VndbDomain.Common.Queries;
using VNDBFuze.VndbDomain.Common.Utilities;

namespace VNDBFuze.VndbDomain.Aggregates.CharacterAggregate
{
    public class CharacterRequestSubfields : RequestFieldAbstractBase, IVndbRequestSubfields
    {
        public TraitRequestFields Traits = new TraitRequestFields();
        public ReleaseRequestFields VisualNovelRelease = new ReleaseRequestFields();
        public ImageRequestFields Image = new ImageRequestFields();
        public VnRequestFieldsFlags VisualNovelFlags = VnRequestFieldsFlags.Id | VnRequestFieldsFlags.Title;

        public void EnableAllFlags()
        {
            EnumUtilities.SetAllEnumFlags(ref VisualNovelFlags);
            Image.EnableAllFlags();
            Traits.EnableAllFlags(true);
            VisualNovelRelease.EnableAllFlags(true);
        }

        public void DisableAllFlags()
        {
            VisualNovelFlags = default;
            Image.DisableAllFlags();
            Traits.DisableAllFlags(true);
            VisualNovelRelease.DisableAllFlags(true);
        }

        public override List<string> GetFlagsStringRepresentations(params string[] prefixParts)
        {
            var prefix = GetFullPrefixString(prefixParts);
            var results = new List<List<string>>
            {
                Traits.GetFlagsStringRepresentations(prefix, CharacterConstants.Fields.TraitsAllFields),
                VisualNovelRelease.GetFlagsStringRepresentations(prefix, CharacterConstants.Fields.VnsReleaseAllFields),

                // thumbnail and thumbnail_dims not available because character images are currently always limited to 256x300px
                Image.GetFlagsStringRepresentations(prefix, CharacterConstants.Fields.ImageAllFields)
                    .Where(s => !s.EndsWith("thumbnail") && !s.EndsWith("thumbnail_dims")).ToList(),
                EnumUtilities.GetStringRepresentations(VisualNovelFlags, GetFullPrefixString(prefix, CharacterConstants.Fields.VnsAllFields))
            };

            return results.SelectMany(x => x).ToList();
        }
    }

    public class CharacterRequestFields : RequestFieldAbstractBase, IVndbRequestFields
    {
        public CharacterRequestFieldsFlags Flags =
            CharacterRequestFieldsFlags.Id | CharacterRequestFieldsFlags.Name | CharacterRequestFieldsFlags.VnsRole;
        public readonly CharacterRequestSubfields Subfields = new CharacterRequestSubfields();

        public void EnableAllFlags(bool enableSubfields)
        {
            EnumUtilities.SetAllEnumFlags(ref Flags);
            if (enableSubfields)
            {
                Subfields.EnableAllFlags();
            }
        }

        public void DisableAllFlags(bool disableSubfields)
        {
            Flags = default;
            if (disableSubfields)
            {
                Subfields.DisableAllFlags();
            }
        }

        public override List<string> GetFlagsStringRepresentations(params string[] prefixParts)
        {
            var prefix = GetFullPrefixString(prefixParts);
            var mainList = EnumUtilities.GetStringRepresentations(Flags, prefix);
            var subfieldsLists = Subfields.GetFlagsStringRepresentations(prefix);
            mainList.AddRange(subfieldsLists);

            return mainList;
        }
    }

    public class CharacterRequestQuery : RequestQueryBase
    {
        [JsonIgnore]
        public CharacterRequestFields Fields = new CharacterRequestFields();

        [JsonIgnore]
        public CharacterRequestSortEnum Sort = CharacterRequestSortEnum.SearchRank;

        public CharacterRequestQuery(SimpleFilterBase<Character> filter) : base(filter)
        {

        }

        public CharacterRequestQuery(ComplexFilterBase<Character> filter) : base(filter)
        {

        }

        protected override List<string> GetEnabledFields()
        {
            return Fields.GetFlagsStringRepresentations();
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