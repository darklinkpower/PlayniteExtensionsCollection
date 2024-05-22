using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Aggregates.ImageAggregate;
using VNDBFuze.VndbDomain.Common.Interfaces;
using VNDBFuze.VndbDomain.Common.Utilities;

namespace VNDBFuze.VndbDomain.Common.Models
{
    public class ImageRequestFields : RequestFieldAbstractBase, IVndbRequestSubfields
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