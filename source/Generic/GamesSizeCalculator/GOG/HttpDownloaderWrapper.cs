using PluginsCommon.Web;
using System.Collections.Generic;

namespace GamesSizeCalculator.GOG
{
    public class HttpDownloaderWrapper : IHttpDownloader
    {
        public string DownloadString(string url, List<System.Net.Cookie> cookies)
        {
            return HttpDownloader.DownloadString(url, cookies);
        }

        public string DownloadString(string url)
        {
            return HttpDownloader.DownloadString(url);
        }
    }
}
