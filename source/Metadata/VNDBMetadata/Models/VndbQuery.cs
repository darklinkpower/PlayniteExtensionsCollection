using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.Interfaces;

namespace VNDBMetadata.Models
{
    public class VndbQuery
    {
        public readonly object filters = new object();
        public readonly string fields = "";
        public string sort = "id";
        public bool reverse = false;
        public uint results = 10;
        public uint page = 1;
        public string user = null;
        public bool count = false;
        public bool compact_filters = false;
        public bool normalized_filters = false;

        public VndbQuery(IPredicate filter, VisualNovelFieldsSettings fields)
        {
            this.filters = Serialization.FromJson<object>(filter.ToJsonString());
            this.fields = fields.ToString();
        }

    }
}