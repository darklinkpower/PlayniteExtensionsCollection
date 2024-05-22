using Newtonsoft.Json;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Filters;
using VNDBFuze.VndbDomain.Common.Flags;
using VNDBFuze.VndbDomain.Common.Interfaces;
using VNDBFuze.VndbDomain.Common.Models;
using VNDBFuze.VndbDomain.Common.Queries;
using VNDBFuze.VndbDomain.Common.Utilities;

namespace VNDBFuze.VndbDomain.Aggregates.StaffAggregate
{
    public class StaffRequestSubfieldsFlags : RequestFieldAbstractBase, IVndbRequestFields
    {
        public ExtLinksRequestFields ExtLinks = new ExtLinksRequestFields();
        public AliasesRequestFields Aliases = new AliasesRequestFields();

        public void EnableAllFlags(bool enableSubfields)
        {
            ExtLinks.EnableAllFlags();
            Aliases.EnableAllFlags();
        }

        public void DisableAllFlags(bool disableSubfields)
        {
            ExtLinks.DisableAllFlags();
            Aliases.DisableAllFlags();
        }

        public override List<string> GetFlagsStringRepresentations(params string[] prefixParts)
        {
            var prefix = GetFullPrefixString(prefixParts);
            var results = new List<List<string>>
            {
                ExtLinks.GetFlagsStringRepresentations(prefix),
                Aliases.GetFlagsStringRepresentations(prefix)
            };

            return results.SelectMany(x => x).ToList();
        }
    }

    public class StaffRequestFields : RequestFieldAbstractBase, IVndbRequestFields
    {
        public StaffRequestFieldsFlags Flags = StaffRequestFieldsFlags.Id | StaffRequestFieldsFlags.Name;
        public readonly StaffRequestSubfieldsFlags Subfields = new StaffRequestSubfieldsFlags();

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

    public class StaffRequestQuery : RequestQueryBase
    {
        [JsonIgnore]
        public StaffRequestFields Fields = new StaffRequestFields();

        [JsonIgnore]
        public StaffRequestSortEnum Sort = StaffRequestSortEnum.SearchRank;

        public StaffRequestQuery(SimpleFilterBase<Staff> filter) : base(filter)
        {

        }

        public StaffRequestQuery(ComplexFilterBase<Staff> filter) : base(filter)
        {

        }

        protected override List<string> GetEnabledFields()
        {
            return Fields.GetFlagsStringRepresentations();
        }

        protected override string GetSortString()
        {
            if (Filters is SimpleFilterBase<Staff> simpleFilter)
            {
                if (Sort == StaffRequestSortEnum.SearchRank)
                {
                    if (simpleFilter.Name != StaffFilterFactory.Search.FilterName)
                    {
                        return null;
                    }
                }
            }
            else if (Filters is ComplexFilterBase<Staff> complexFilter)
            {
                var simplePredicates = complexFilter.Filters.OfType<SimpleFilterBase<Staff>>();
                if (Sort == StaffRequestSortEnum.SearchRank)
                {
                    var searchPredicatesCount = simplePredicates.Count(x => x.Name == StaffFilterFactory.Search.FilterName);
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