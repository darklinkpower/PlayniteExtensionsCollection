using GamesSizeCalculator.GOG;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace GamesSizeCalculator.Tests.GOG
{
    public class FakeHttpDownloader : IHttpDownloader
    {
        public FakeHttpDownloader(Dictionary<string, string> urlFiles)
        {
            UrlFiles = urlFiles;
        }

        public Dictionary<string, string> UrlFiles { get; }

        public string DownloadString(string url, List<Cookie> cookies)
        {
            return DownloadString(url);
        }

        public string DownloadString(string url)
        {
            return File.ReadAllText(UrlFiles[url]);
        }
    }
}
