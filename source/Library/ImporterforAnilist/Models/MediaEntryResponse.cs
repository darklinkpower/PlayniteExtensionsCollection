using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImporterforAnilist.Models
{
    public partial class MediaEntryData
    {
        [SerializationPropertyName("data")]
        public MediaEntryMedia Data { get; set; }
    }

    public partial class MediaEntryMedia
    {
        [SerializationPropertyName("Media")]
        public Media Media { get; set; }
    }
}