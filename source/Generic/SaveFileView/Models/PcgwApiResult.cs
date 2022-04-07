using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveFileView.Models
{
    public class PcgwApiResult
    {
        [SerializationPropertyName("query")]
        public Query Query { get; set; }
    }

    public class Query
    {
        [SerializationPropertyName("printrequests")]
        public Printrequest[] Printrequests { get; set; }

        [SerializationPropertyName("results")]
        public Dictionary<string, ResultsData> Results { get; set; }

        [SerializationPropertyName("serializer")]
        public string Serializer { get; set; }

        [SerializationPropertyName("version")]
        public long Version { get; set; }

        [SerializationPropertyName("meta")]
        public Meta Meta { get; set; }
    }

    public class Meta
    {
        [SerializationPropertyName("hash")]
        public string Hash { get; set; }

        [SerializationPropertyName("count")]
        public long Count { get; set; }

        [SerializationPropertyName("offset")]
        public long Offset { get; set; }

        [SerializationPropertyName("source")]
        public string Source { get; set; }

        [SerializationPropertyName("time")]
        public string Time { get; set; }
    }

    public class Printrequest
    {
        [SerializationPropertyName("label")]
        public string Label { get; set; }

        [SerializationPropertyName("key")]
        public string Key { get; set; }

        [SerializationPropertyName("redi")]
        public string Redi { get; set; }

        [SerializationPropertyName("typeid")]
        public string Typeid { get; set; }

        [SerializationPropertyName("mode")]
        public long Mode { get; set; }
    }

    public class ResultsData
    {
        //[SerializationPropertyName("printouts")]
        //public object[] Printouts { get; set; }

        [SerializationPropertyName("fulltext")]
        public string Fulltext { get; set; }

        [SerializationPropertyName("fullurl")]
        public Uri Fullurl { get; set; }

        [SerializationPropertyName("namespace")]
        public long Namespace { get; set; }

        [SerializationPropertyName("exists")]
        public string Exists { get; set; }

        [SerializationPropertyName("displaytitle")]
        public string Displaytitle { get; set; }
    }
}