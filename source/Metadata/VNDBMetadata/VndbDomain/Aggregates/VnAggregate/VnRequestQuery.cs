using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.CharacterAggregate;
using VNDBMetadata.VndbDomain.Aggregates.ImageAggregate;
using VNDBMetadata.VndbDomain.Aggregates.ProducerAggregate;
using VNDBMetadata.VndbDomain.Aggregates.ReleaseAggregate;
using VNDBMetadata.VndbDomain.Aggregates.StaffAggregate;
using VNDBMetadata.VndbDomain.Aggregates.TagAggregate;
using VNDBMetadata.VndbDomain.Common.Filters;
using VNDBMetadata.VndbDomain.Common.Flags;
using VNDBMetadata.VndbDomain.Common.Interfaces;
using VNDBMetadata.VndbDomain.Common.Models;
using VNDBMetadata.VndbDomain.Common.Queries;
using VNDBMetadata.VndbDomain.Common.Utilities;

namespace VNDBMetadata.VndbDomain.Aggregates.VnAggregate
{
    public class VnRequestSubfieldsFlags : RequestFieldAbstractBase, IVndbRequestFields
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
                EnumUtilities.GetStringRepresentations(VisualNovelRelationsFlags, GetFullPrefixString(prefix, VnConstants.Fields.Relations)),
                Image.GetFlagsStringRepresentations(prefix, VnConstants.Fields.Image),
                Screenshots.GetFlagsStringRepresentations(prefix, VnConstants.Fields.Screenshots),
                ScreenshotsRelease.GetFlagsStringRepresentations(prefix, VnConstants.Fields.ScreenshotsRelease),
                Tags.GetFlagsStringRepresentations(prefix, VnConstants.Fields.Tags),
                Developers.GetFlagsStringRepresentations(prefix, VnConstants.Fields.Developers),
                Staff.GetFlagsStringRepresentations(prefix, VnConstants.Fields.Staff),
                VoiceActor.GetFlagsStringRepresentations(prefix, VnConstants.Fields.VaStaff),
                VoiceActorCharacter.GetFlagsStringRepresentations(prefix, VnConstants.Fields.VaCharacter)
            };

            return results.SelectMany(x => x).ToList();
        }
    }

    public class VnRequestFields : RequestFieldAbstractBase, IVndbRequestFields
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

    public class VnRequestQuery : RequestQueryBase
    {
        [JsonIgnore]
        public VnRequestFields Fields = new VnRequestFields();

        [JsonIgnore]
        public VnRequestSortEnum Sort = VnRequestSortEnum.SearchRank;

        public VnRequestQuery(SimpleFilterBase<Vn> filter) : base(filter)
        {

        }

        public VnRequestQuery(ComplexFilterBase<Vn> filter) : base(filter)
        {

        }

        protected override List<string> GetEnabledFields()
        {
            return Fields.GetFlagsStringRepresentations();
        }

        protected override string GetSortString()
        {
            if (Filters is SimpleFilterBase<Vn> simpleFilter)
            {
                if (Sort == VnRequestSortEnum.SearchRank)
                {
                    if (simpleFilter.Name != VnFilterFactory.Search.FilterName)
                    {
                        return null;
                    }
                }
            }
            else if (Filters is ComplexFilterBase<Vn> complexFilter)
            {
                var simplePredicates = complexFilter.Filters.OfType<SimpleFilterBase<Vn>>();
                if (Sort == VnRequestSortEnum.SearchRank)
                {
                    var searchPredicatesCount = simplePredicates.Count(x => x.Name == VnFilterFactory.Search.FilterName);
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