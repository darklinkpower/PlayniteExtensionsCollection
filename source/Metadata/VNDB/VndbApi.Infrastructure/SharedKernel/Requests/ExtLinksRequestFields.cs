using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Infrastructure.SharedKernel.Requests
{
    public class ExtLinksRequestFields : RequestFieldAbstractBase, IRequestSubfields
    {
        public ExtLinksFieldsFlags Flags =
            ExtLinksFieldsFlags.Id | ExtLinksFieldsFlags.Label | ExtLinksFieldsFlags.Name | ExtLinksFieldsFlags.Url;

        public void EnableAllFlags()
        {
            EnumUtilities.SetAllEnumFlags(ref Flags);
        }

        public void DisableAllFlags()
        {
            Flags = default;
        }

        public override List<string> GetFlagsStringRepresentations(params string[] prefixParts)
        {
            var prefix = GetFullPrefixString(prefixParts);
            return EnumUtilities.GetStringRepresentations(Flags, prefix);
        }

        
    }
}