using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.QueryConstants
{
    public static class Tag
    {
        public static class Sorting
        {
            /// <summary>
            /// Id
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// Name
            /// </summary>
            public const string Name = "name";

            /// <summary>
            /// Visual novel count
            /// </summary>
            public const string VnCount = "vn_count";

            /// <summary>
            /// Search rank
            /// </summary>
            public const string SearchRank = "searchrank";
        }

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

            /// <summary>
            /// String, see category field.
            /// </summary>
            public const string Category = "category";
        }

        public static class Fields
        {
            /// <summary>
            /// vndbid.
            /// </summary>
            public const string Id = "id";

            /// <summary>
            /// String.
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
            /// String, "cont" for content, "ero" for sexual content, and "tech" for technical tags.
            /// </summary>
            public const string Category = "category";

            /// <summary>
            /// Bool.
            /// </summary>
            public const string Searchable = "searchable";

            /// <summary>
            /// Bool.
            /// </summary>
            public const string Applicable = "applicable";

            /// <summary>
            /// Integer, number of VNs this tag has been applied to, including any child tags.
            /// </summary>
            public const string VnCount = "vn_count";
        }

        public static class Category
        {
            /// <summary>
            /// Content
            /// </summary>
            public const string Content = "cont";

            /// <summary>
            /// Sexual Content
            /// </summary>
            public const string SexualContent = "ero";

            /// <summary>
            /// Technical
            /// </summary>
            public const string Technical = "tech";
        }
    }
}