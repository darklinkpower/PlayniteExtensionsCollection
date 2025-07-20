using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiInfrastructure.SharedKernel.Requests;
using VndbApiInfrastructure.StaffAggregate;
using VndbApiInfrastructure.VisualNovelAggregate;
using VndbApiInfrastructure.SharedKernel;
using VndbApiInfrastructure.SharedKernel.Filters;
using VndbApiDomain.ReleaseAggregate;
using VndbApiInfrastructure.ProducerAggregate;

namespace VndbApiInfrastructure.ReleaseAggregate
{
    public class ReleaseRequestSubfields : RequestFieldAbstractBase, IRequestFields
    {
        public ProducerRequestFields Producer = new ProducerRequestFields();
        public ExternalLinksRequestFields ExternalLinks = new ExternalLinksRequestFields();
        public ImageRequestFields Images = new ImageRequestFields();
        public VnRequestFieldsFlags VisualNovelFlags = VnRequestFieldsFlags.Id | VnRequestFieldsFlags.Title;

        public void EnableAllFlags(bool enableSubfields)
        {
            EnumUtilities.SetAllEnumFlags(ref VisualNovelFlags);
            ExternalLinks.EnableAllFlags();
            Producer.DisableAllFlags(enableSubfields);
        }

        public void DisableAllFlags(bool disableSubfields)
        {
            VisualNovelFlags = default;
            Producer.DisableAllFlags(disableSubfields);
            ExternalLinks.DisableAllFlags();
        }

        public override List<string> GetFlagsStringRepresentations(params string[] prefixParts)
        {
            var prefix = GetFullPrefixString(prefixParts);
            var results = new List<List<string>>
            {
                Producer.GetFlagsStringRepresentations(prefix, ReleaseConstants.Fields.ProducersAll),
                EnumUtilities.GetStringRepresentations(VisualNovelFlags, GetFullPrefixString(prefix, ReleaseConstants.Fields.VnsAll)),
                ExternalLinks.GetFlagsStringRepresentations(prefix, ReleaseConstants.Fields.ExternalLinks)
            };

            return results.SelectMany(x => x).ToList();
        }
    }

    public class ReleaseRequestFields : RequestFieldAbstractBase, IRequestFields
    {
        public ReleaseRequestFieldsFlags Flags =
            ReleaseRequestFieldsFlags.Id | ReleaseRequestFieldsFlags.Title | ReleaseRequestFieldsFlags.Official | ReleaseRequestFieldsFlags.Platforms;
        public readonly ReleaseRequestSubfields Subfields = new ReleaseRequestSubfields();

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

    public class ReleaseRequestQuery : RequestQueryBase
    {
        [JsonIgnore]
        public ReleaseRequestFields Fields = new ReleaseRequestFields();

        [JsonIgnore]
        public ReleaseRequestSortEnum Sort = ReleaseRequestSortEnum.SearchRank;

        public ReleaseRequestQuery(SimpleFilterBase<Release> filter) : base(filter)
        {

        }

        public ReleaseRequestQuery(ComplexFilterBase<Release> filter) : base(filter)
        {

        }

        protected override List<string> GetEnabledFields()
        {
            return Fields.GetFlagsStringRepresentations();
        }

        protected override string GetSortString()
        {
            if (Filters is SimpleFilterBase<Release> simpleFilter)
            {
                if (Sort == ReleaseRequestSortEnum.SearchRank)
                {
                    if (simpleFilter.Name != ReleaseFilterFactory.Search.FilterName)
                    {
                        return null;
                    }
                }
            }
            else if (Filters is ComplexFilterBase<Release> complexFilter)
            {
                var simplePredicates = complexFilter.Filters.OfType<SimpleFilterBase<Release>>();
                if (Sort == ReleaseRequestSortEnum.SearchRank)
                {
                    var searchPredicatesCount = simplePredicates.Count(x => x.Name == ReleaseFilterFactory.Search.FilterName);
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