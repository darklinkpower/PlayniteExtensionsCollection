using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDB.ApiConstants
{
    public static class DatabaseDumps
    {
        /// <summary>
        /// This dump includes information about all (approved) VN tags in the JSON format.
        /// </summary>
        public const string TagsUrl = @"https://dl.vndb.org/dump/vndb-tags-latest.json.gz";

        /// <summary>
        /// This dump includes information about all (approved) character traits in the JSON format.
        /// </summary>
        public const string TraitsUrl = @"https://dl.vndb.org/dump/vndb-traits-latest.json.gz";
    }
}
