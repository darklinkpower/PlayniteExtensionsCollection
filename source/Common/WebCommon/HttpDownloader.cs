using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using Playnite.SDK;
using System.Threading;

namespace WebCommon
{
    // Based on https://github.com/JosefNemec/Playnite
    public class HttpDownloader
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly Downloader downloader = new Downloader();

        public static string DownloadString(IEnumerable<string> mirrors)
        {
            return downloader.DownloadString(mirrors);
        }

        public static DownloadStringResult DownloadString(string url)
        {
            return downloader.DownloadString(url);
        }

        public static DownloadStringResult DownloadString(string url, CancellationToken cancelToken)
        {
            return downloader.DownloadString(url, cancelToken);
        }

        public static DownloadStringResult DownloadString(string url, List<Cookie> cookies)
        {
            return downloader.DownloadString(url, cookies);
        }

        public static void DownloadString(string url, string path)
        {
            downloader.DownloadString(url, path);
        }

        public static void DownloadString(string url, string path, Encoding encoding)
        {
            downloader.DownloadString(url, path, encoding);
        }

        public static byte[] DownloadData(string url)
        {
            return downloader.DownloadData(url);
        }

        public static DownloadFileResult DownloadFile(string url, string path)
        {
            return downloader.DownloadFile(url, path);
        }

        public static DownloadFileResult DownloadFile(string url, string path, CancellationToken cancelToken)
        {
            return downloader.DownloadFile(url, path, cancelToken);
        }

        public static bool DownloadFile(string url, string path, CancellationToken cancelToken, Action<DownloadProgressChangedEventArgs> progressHandler)
        {
            return downloader.DownloadFile(url, path, cancelToken, progressHandler);
        }

        public static DownloadStringResult DownloadStringWithHeaders(string url, Dictionary<string, string> headersDictionary)
        {
            return downloader.DownloadStringWithHeaders(url, headersDictionary);
        }

        public static DownloadStringResult DownloadStringWithHeaders(string url, Dictionary<string, string> headersDictionary, List<Cookie> cookies)
        {
            return downloader.DownloadStringWithHeaders(url, headersDictionary, cookies);
        }

        public static HttpClient GetHttpClientInstance()
        {
            return downloader.GetHttpClientInstance();
        }

        public static HttpClient GetHttpClientInstanceHandlerNoUseCookies()
        {
            return downloader.GetHttpClientInstanceHandlerNoUseCookies();
        }

    }
}