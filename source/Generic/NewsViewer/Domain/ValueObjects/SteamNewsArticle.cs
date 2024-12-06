using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsViewer.Domain.ValueObjects
{
    public class SteamNewsArticle
    {
        public string Title { get; }
        public string DescriptionHtmlFormatted { get; }
        public string Url { get; }
        public DateTime PublishedDate { get; }
        public string AuthorName { get; }
        public SteamNewsIdentifier Identifier { get; }

        public SteamNewsArticle(string title, string htmlDescription, string url, DateTime publishedDate, string authorName, SteamNewsIdentifier guid)
        {
            Title = title;
            DescriptionHtmlFormatted = htmlDescription;
            Url = url;
            PublishedDate = publishedDate;
            AuthorName = authorName;
            Identifier = guid;
        }

        public override string ToString()
        {
            return $"{Title} ({PublishedDate})";
        }
    }
}
