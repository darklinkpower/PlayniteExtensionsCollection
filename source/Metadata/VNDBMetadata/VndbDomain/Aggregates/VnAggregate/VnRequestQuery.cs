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
using VNDBMetadata.VndbDomain.Common.Queries;
using VNDBMetadata.VndbDomain.Common.Utilities;

namespace VNDBMetadata.VndbDomain.Aggregates.VnAggregate
{
    public class VnRequestQuery : RequestQueryBase
    {
        [JsonIgnore]
        public VnRequestFieldsFlags FieldsFlags;
        [JsonIgnore]
        public VnRequestSortEnum Sort = VnRequestSortEnum.Id;

        [JsonIgnore]
        public ImageRequestFieldsFlags ImageRequestFieldsFlags;
        [JsonIgnore]
        public ImageRequestFieldsFlags ScreenshotsRequestFieldsFlags;
        [JsonIgnore]
        public ReleaseRequestFieldsFlags ScreenshotsReleaseRequestFieldsFlags;
        [JsonIgnore]
        public VnRequestFieldsFlags RelationsRequestFieldsFlags;
        [JsonIgnore]
        public TagRequestFieldsFlags TagsRequestFieldsFlags;
        [JsonIgnore]
        public ProducerRequestFieldsFlags DevelopersRequestFieldsFlags;
        [JsonIgnore]
        public StaffRequestFieldsFlags StaffRequestFieldsFlags;
        [JsonIgnore]
        public StaffRequestFieldsFlags VaRequestFieldsFlags;
        [JsonIgnore]
        public CharacterRequestFieldsFlags VaCharacterRequestFieldsFlags;

        public VnRequestQuery(SimpleFilterBase<Vn> filter) : base(filter)
        {
            Initialize();
        }

        public VnRequestQuery(ComplexFilterBase<Vn> filter) : base(filter)
        {
            Initialize();
        }

        private void Initialize()
        {
            EnumUtilities.SetAllEnumFlags(ref FieldsFlags);
            EnumUtilities.SetAllEnumFlags(ref RelationsRequestFieldsFlags);

            EnumUtilities.SetAllEnumFlags(ref ImageRequestFieldsFlags);
            EnumUtilities.SetAllEnumFlags(ref ScreenshotsRequestFieldsFlags);
            EnumUtilities.SetAllEnumFlags(ref ScreenshotsReleaseRequestFieldsFlags);

            EnumUtilities.SetAllEnumFlags(ref TagsRequestFieldsFlags);

            EnumUtilities.SetAllEnumFlags(ref DevelopersRequestFieldsFlags);

            EnumUtilities.SetAllEnumFlags(ref StaffRequestFieldsFlags);
            EnumUtilities.SetAllEnumFlags(ref VaRequestFieldsFlags);

            EnumUtilities.SetAllEnumFlags(ref VaCharacterRequestFieldsFlags);
        }

        public override List<string> GetEnabledFields()
        {
            var results = new List<List<string>>
            {
                EnumUtilities.GetStringRepresentations(FieldsFlags),
                EnumUtilities.GetStringRepresentations(RelationsRequestFieldsFlags, VnConstants.Fields.Relations),

                EnumUtilities.GetStringRepresentations(ImageRequestFieldsFlags, VnConstants.Fields.Image),
                EnumUtilities.GetStringRepresentations(ScreenshotsRequestFieldsFlags, VnConstants.Fields.Screenshots),
                EnumUtilities.GetStringRepresentations(ScreenshotsReleaseRequestFieldsFlags, VnConstants.Fields.ScreenshotsRelease),

                EnumUtilities.GetStringRepresentations(TagsRequestFieldsFlags, VnConstants.Fields.Tags),

                EnumUtilities.GetStringRepresentations(DevelopersRequestFieldsFlags, VnConstants.Fields.Developers),

                EnumUtilities.GetStringRepresentations(StaffRequestFieldsFlags, VnConstants.Fields.Staff),
                EnumUtilities.GetStringRepresentations(VaRequestFieldsFlags, VnConstants.Fields.VaStaff),

                EnumUtilities.GetStringRepresentations(VaCharacterRequestFieldsFlags, VnConstants.Fields.VaCharacter)
            };

            return results.SelectMany(x => x).ToList();
        }

        public override string GetSortString()
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