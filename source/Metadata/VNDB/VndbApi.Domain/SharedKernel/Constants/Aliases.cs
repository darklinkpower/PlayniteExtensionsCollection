using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApi.Domain.SharedKernel
{
    public static class Aliases
    {
        public static class Fields
        {
            /// <summary>
            /// Integer, alias id.
            /// </summary>
            public const string Aid = "aliases.aid";

            /// <summary>
            /// String, name in original script.
            /// </summary>
            public const string Name = "aliases.name";

            /// <summary>
            /// String, possibly null, romanized version of ‘name’.
            /// </summary>
            public const string Latin = "aliases.latin";

            /// <summary>
            /// Boolean, whether this alias is used as “main” name for the staff entry.
            /// </summary>
            public const string IsMain = "aliases.ismain";
        }
    }
}
