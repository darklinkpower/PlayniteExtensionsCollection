using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Infrastructure.SharedKernel.Requests;
using VndbApi.Infrastructure.ReleaseAggregate;
using VndbApi.Infrastructure.TagAggregate;
using VndbApi.Infrastructure.ProducerAggregate;
using VndbApi.Infrastructure.StaffAggregate;
using VndbApi.Infrastructure.CharacterAggregate;
using VndbApi.Infrastructure.SharedKernel;
using VndbApi.Infrastructure.SharedKernel.Filters;
using VndbApi.Domain.VisualNovelAggregate;

namespace VndbApi.Infrastructure.VisualNovelAggregate
{
    public class VnRequestSubfieldsFlags : RequestFieldAbstractBase, IRequestFields
    {
        public VnRequestFieldsFlags VisualNovelRelationsFlags = VnRequestFieldsFlags.Id | VnRequestFieldsFlags.Title;
        public ImageRequestFields Image = new ImageRequestFields();
        public ImageRequestFields Screenshots = new ImageRequestFields();
        public ReleaseRequestFields ScreenshotsRelease = new ReleaseRequestFields();
        public TagRequestFields Tags = new TagRequestFields();
        public ProducerRequestFields Developers = new ProducerRequestFields();
        public StaffRequestFields Staff = new StaffRequestFields();
        public StaffRequestFields VoiceActor = new StaffRequestFields();
        public CharacterRequestFields VoiceActorCharacter = new CharacterRequestFields();

        public void EnableAllFlags(bool enableSubfields)
        {
            EnumUtilities.SetAllEnumFlags(ref VisualNovelRelationsFlags);
            Image.EnableAllFlags();
            Screenshots.EnableAllFlags();
            ScreenshotsRelease.EnableAllFlags(enableSubfields);
            Tags.EnableAllFlags(enableSubfields);
            Developers.EnableAllFlags(enableSubfields);
            Staff.EnableAllFlags(enableSubfields);
            VoiceActor.EnableAllFlags(enableSubfields);
            VoiceActorCharacter.EnableAllFlags(enableSubfields);
        }

        public void DisableAllFlags(bool disableSubfields)
        {
            VisualNovelRelationsFlags = default;
            Image.DisableAllFlags();
            Screenshots.DisableAllFlags();
            ScreenshotsRelease.DisableAllFlags(disableSubfields);
            Tags.DisableAllFlags(disableSubfields);
            Developers.DisableAllFlags(disableSubfields);
            Staff.DisableAllFlags(disableSubfields);
            VoiceActor.DisableAllFlags(disableSubfields);
            VoiceActorCharacter.DisableAllFlags(disableSubfields);
        }

        public override List<string> GetFlagsStringRepresentations(params string[] prefixParts)
        {
            var prefix = GetFullPrefixString(prefixParts);
            var results = new List<List<string>>
            {
                EnumUtilities.GetStringRepresentations(VisualNovelRelationsFlags, GetFullPrefixString(prefix, VisualNovelConstants.Fields.Relations)),
                Image.GetFlagsStringRepresentations(prefix, VisualNovelConstants.Fields.Image),
                Screenshots.GetFlagsStringRepresentations(prefix, VisualNovelConstants.Fields.Screenshots),
                ScreenshotsRelease.GetFlagsStringRepresentations(prefix, VisualNovelConstants.Fields.ScreenshotsRelease),
                Tags.GetFlagsStringRepresentations(prefix, VisualNovelConstants.Fields.Tags),
                Developers.GetFlagsStringRepresentations(prefix, VisualNovelConstants.Fields.Developers),
                Staff.GetFlagsStringRepresentations(prefix, VisualNovelConstants.Fields.Staff),
                VoiceActor.GetFlagsStringRepresentations(prefix, VisualNovelConstants.Fields.VaStaff),
                VoiceActorCharacter.GetFlagsStringRepresentations(prefix, VisualNovelConstants.Fields.VaCharacter)
            };

            return results.SelectMany(x => x).ToList();
        }
    }

    public class VnRequestFields : RequestFieldAbstractBase, IRequestFields
    {
        public VnRequestFieldsFlags Flags = VnRequestFieldsFlags.Id | VnRequestFieldsFlags.Title;
        public readonly VnRequestSubfieldsFlags Subfields = new VnRequestSubfieldsFlags();

        public void EnableAllFlags(bool enableSubfields)
        {
            EnumUtilities.SetAllEnumFlags(ref Flags);
            if (enableSubfields)
            {
                Subfields.EnableAllFlags(enableSubfields);
            }
        }

        public void DisableAllFlags(bool disableSubfields)
        {
            Flags = default;
            if (disableSubfields)
            {
                Subfields.DisableAllFlags(disableSubfields);
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

    public class VisualNovelRequestQuery : RequestQueryBase
    {
        [JsonIgnore]
        public VnRequestFields Fields = new VnRequestFields();

        [JsonIgnore]
        public VnRequestSortEnum Sort = VnRequestSortEnum.SearchRank;

        public VisualNovelRequestQuery(SimpleFilterBase<VisualNovel> filter) : base(filter)
        {

        }

        public VisualNovelRequestQuery(ComplexFilterBase<VisualNovel> filter) : base(filter)
        {

        }

        protected override List<string> GetEnabledFields()
        {
            return Fields.GetFlagsStringRepresentations();
        }

        protected override string GetSortString()
        {
            if (Filters is SimpleFilterBase<VisualNovel> simpleFilter)
            {
                if (Sort == VnRequestSortEnum.SearchRank)
                {
                    if (simpleFilter.Name != VisualNovelFilterFactory.Search.FilterName)
                    {
                        return null;
                    }
                }
            }
            else if (Filters is ComplexFilterBase<VisualNovel> complexFilter)
            {
                var simplePredicates = complexFilter.Filters.OfType<SimpleFilterBase<VisualNovel>>();
                if (Sort == VnRequestSortEnum.SearchRank)
                {
                    var searchPredicatesCount = simplePredicates.Count(x => x.Name == VisualNovelFilterFactory.Search.FilterName);
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