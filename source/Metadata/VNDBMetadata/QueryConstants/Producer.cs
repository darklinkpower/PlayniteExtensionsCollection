using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.QueryConstants
{
    public static class Producer
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

            /// <summary>
            /// Language.
            /// </summary>
            public const string Lang = "lang";

            /// <summary>
            /// Producer type, see the type field below.
            /// </summary>
            public const string Type = "type";
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
            /// String, possibly null, name in the original script.
            /// </summary>
            public const string Original = "original";

            /// <summary>
            /// Array of strings.
            /// </summary>
            public const string Aliases = "aliases";

            /// <summary>
            /// String, primary language.
            /// </summary>
            public const string Lang = "lang";

            /// <summary>
            /// String, producer type, "co" for company, "in" for individual, and "ng" for amateur group.
            /// </summary>
            public const string Type = "type";

            /// <summary>
            /// String, possibly null, may contain formatting codes.
            /// </summary>
            public const string Description = "description";
        }
    }
}