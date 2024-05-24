using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.ImageAggregate;

namespace VndbApiInfrastructure.SharedKernel.Requests
{
    public class ImageRequestFields : RequestFieldAbstractBase, IRequestSubfields
    {
        public ImageRequestFieldsFlags Flags =
            ImageRequestFieldsFlags.ThumbnailUrl | ImageRequestFieldsFlags.VoteCount | ImageRequestFieldsFlags.Sexual |
            ImageRequestFieldsFlags.Violence | ImageRequestFieldsFlags.Url;

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