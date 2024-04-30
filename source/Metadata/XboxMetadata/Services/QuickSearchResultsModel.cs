using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XboxMetadata.Services
{
    public class QuickSearchResult
    {
        [SerializationPropertyName("Query")]
        public string Query { get; set; }

        [SerializationPropertyName("ResultSets")]
        public ResultSet[] ResultSets { get; set; }

        [SerializationPropertyName("ErrorSets")]
        public ErrorSet[] ErrorSets { get; set; }
    }

    public class ErrorSet
    {
        [SerializationPropertyName("Source")]
        public string Source { get; set; }

        [SerializationPropertyName("Message")]
        public string Message { get; set; }
    }

    public class ResultSet
    {
        [SerializationPropertyName("Source")]
        public string Source { get; set; }

        [SerializationPropertyName("FromCache")]
        public bool FromCache { get; set; }

        [SerializationPropertyName("Type")]
        public string Type { get; set; }

        [SerializationPropertyName("Suggests")]
        public Suggest[] Suggests { get; set; }

        [SerializationPropertyName("Metas")]
        public object Metas { get; set; }
    }

    public class Suggest
    {
        [SerializationPropertyName("Source")]
        public string Source { get; set; }

        [SerializationPropertyName("Title")]
        public string Title { get; set; }

        [SerializationPropertyName("Description")]
        public string Description { get; set; }

        [SerializationPropertyName("Url")]
        public string Url { get; set; }

        [SerializationPropertyName("ImageUrl")]
        public string ImageUrl { get; set; }

        [SerializationPropertyName("Metas")]
        public Meta[] Metas { get; set; }

        [SerializationPropertyName("Curated")]
        public bool Curated { get; set; }
    }

    public class Meta
    {
        [SerializationPropertyName("Key")]
        public string Key { get; set; }

        [SerializationPropertyName("Value")]
        public string Value { get; set; }
    }
}