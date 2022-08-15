using System.Collections.Generic;

namespace GamesSizeCalculator.GOG
{
    public interface IHttpDownloader
    {
        string DownloadString(string url, List<System.Net.Cookie> cookies);
        string DownloadString(string url);
    }
}
