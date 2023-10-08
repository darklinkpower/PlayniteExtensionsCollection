using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsViewer.Models
{
    public class SteamNewsRssFeed
    {
        public Channel Channel { get; set; }
    }

    public class Channel
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string Generator { get; set; }
        public List<RssItem> Items { get; set; }
    }

    public class RssItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public DateTime PubDate { get; set; }
        public string Author { get; set; }
        public NewsGuid Guid { get; set; }
    }

    public class NewsGuid
    {
        public bool IsPermaLink { get; set; }
        public string Value { get; set; }
    }
}