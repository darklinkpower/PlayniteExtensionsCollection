using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.SharedKernel
{
    [Flags]
    public enum ExtLinksFieldsFlags
    {
        /// <summary>
        /// String, URL.
        /// </summary>
        [StringRepresentation(ExtLinks.Fields.Url)]
        Url = 1 << 0,

        /// <summary>
        /// String, English human-readable label for this link.
        /// </summary>
        [StringRepresentation(ExtLinks.Fields.Label)]
        Label = 1 << 1,

        /// <summary>
        /// Internal identifier of the site, intended for applications that want to localize the label or to parse/format/extract remote identifiers. Keep in mind that the list of supported sites, their internal names and their ID types are subject to change, but I’ll try to keep things stable.
        /// </summary>
        [StringRepresentation(ExtLinks.Fields.Name)]
        Name = 1 << 2,

        /// <summary>
        /// Remote identifier for this link. Not all sites have a sensible identifier as part of their URL format, in such cases this field is simply equivalent to the URL.
        /// </summary>
        [StringRepresentation(ExtLinks.Fields.Id)]
        Id = 1 << 3
    }

}