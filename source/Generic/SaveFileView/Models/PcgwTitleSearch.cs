using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveFileView.Models
{
    public class PcgwTitleSearch
    {
        [SerializationPropertyName("batchcomplete")]
        public string BatchComplete { get; set; }

        [SerializationPropertyName("query")]
        public Query Query { get; set; }
    }

    public class Query
    {
        [SerializationPropertyName("searchinfo")]
        public Searchinfo SearchInfo { get; set; }

        [SerializationPropertyName("search")]
        public Search[] Search { get; set; }
    }

    public class Search
    {
        [SerializationPropertyName("ns")]
        public long Ns { get; set; }

        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("pageid")]
        public long PageId { get; set; }

        [SerializationPropertyName("size")]
        public long Size { get; set; }

        [SerializationPropertyName("wordcount")]
        public long WordCount { get; set; }

        [SerializationPropertyName("snippet")]
        public string Snippet { get; set; }

        [SerializationPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }

    public class Searchinfo
    {
        [SerializationPropertyName("totalhits")]
        public long TotalHits { get; set; }
    }
}