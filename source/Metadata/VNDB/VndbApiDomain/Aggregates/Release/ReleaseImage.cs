using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.ImageAggregate;
using VndbApiDomain.SharedKernel;

namespace VndbApiDomain.ReleaseAggregate
{
    public class ReleaseImage : VndbImage
    {
        /// <summary>
        /// Visual novel ID to which this image applies, usually null. This field is only useful for bundle releases that are linked to multiple VNs. 
        /// </summary>
        [JsonProperty("vn")]
        public string Vn { get; set; }

        /// <summary>
        /// Array of strings, list of languages this VN is available in. Does not include machine translations.
        /// </summary>
        [JsonProperty("languages")]
        [JsonConverter(typeof(StringRepresentationEnumListConverter<LanguageEnum>))]
        public List<LanguageEnum> Languages { get; set; }

        /// <summary>
        /// Boolean.
        /// </summary>
        [JsonProperty("photo")]
        public bool Photo { get; set; }
    }
}