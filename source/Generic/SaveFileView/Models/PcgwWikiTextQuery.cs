using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveFileView.Models
{

    public class PcgwWikiTextQuery
    {
        [SerializationPropertyName("parse")]
        public Parse Parse { get; set; }
    }

    public class Parse
    {
        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("pageid")]
        public long Pageid { get; set; }

        [SerializationPropertyName("wikitext")]
        public WikitextClass Wikitext { get; set; }
    }

    public class WikitextClass
    {
        [SerializationPropertyName("*")]
        public string TextDump { get; set; }
    }
}
