using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsViewer.Domain.ValueObjects
{
    public class SteamNewsFeed
    {
        public string Title { get; }
        public string Link { get; }
        public string Description { get; }
        public string Language { get; }
        public IReadOnlyList<SteamNewsArticle> Items { get; }

        public SteamNewsFeed(string title, string link, string description, string language, List<SteamNewsArticle> items)
        {
            Title = title;
            Link = link;
            Description = description;
            Language = language;
            Items = items.AsReadOnly();
        }
    }
}