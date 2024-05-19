using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.VndbDomain.Aggregates.TraitAggregate
{
    public static class TraitConstants
    {
        public static class Filters
        {
            /// <summary>
            /// vndbid
            /// Filter: o
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// String search.
            /// Filter: m
            /// </summary>
            public const string Search = "search";
        }

        public static class Fields
        {
            /// <summary>
            /// vndbid
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// String. Trait names are not necessarily self-describing, so they should always be displayed together with their “group” (see below), which is the top-level parent that the trait belongs to.
            /// </summary>
            public const string Name = "name";

            /// <summary>
            /// Array of strings.
            /// </summary>
            public const string Aliases = "aliases";

            /// <summary>
            /// String, may contain formatting codes.
            /// </summary>
            public const string Description = "description";

            /// <summary>
            /// Bool.
            /// </summary>
            public const string Searchable = "searchable";

            /// <summary>
            /// Bool.
            /// </summary>
            public const string Applicable = "applicable";

            /// <summary>
            /// vndbid
            /// </summary>
            public const string GroupId = "group_id";

            /// <summary>
            /// String
            /// </summary>
            public const string GroupName = "group_name";

            /// <summary>
            /// Integer, number of characters this trait has been applied to, including child traits.
            /// </summary>
            public const string CharCount = "char_count";
        }
    }
}