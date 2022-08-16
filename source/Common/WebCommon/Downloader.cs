using Microsoft.Extensions.DependencyInjection;
using Playnite.SDK;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebCommon
{
    // Based on https://github.com/JosefNemec/Playnite
    public interface IDownloader
    {
        string DownloadString(IEnumerable<string> mirrors);

        string DownloadString(string url);

        string DownloadString(string url, Encoding encoding);

        string DownloadString(string url, List<Cookie> cookies);

        string DownloadString(string url, List<Cookie> cookies, Encoding encoding);

        string DownloadStringWithHeadersAsync(string url, Dictionary<string, string> headersDictionary);

        void DownloadString(string url, string path);

        void DownloadString(string url, string path, Encoding encoding);

        byte[] DownloadData(string url);

        bool DownloadFile(string url, string path);

        void DownloadFile(IEnumerable<string> mirrors, string path);

        Task DownloadFileAsync(string url, string path, Action<DownloadProgressChangedEventArgs> progressHandler);

        Task DownloadFileAsync(IEnumerable<string> mirrors, string path, Action<DownloadProgressChangedEventArgs> progressHandler);
    }

    public class Downloader : IDownloader
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        protected readonly IHttpClientFactory _httpClientFactory;

        public Downloader()
        {
            var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            _httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        }

        protected HttpClient GetClient(string url)
        {
            return GetClient(new Uri(url));
        }

        protected HttpClient GetClient(Uri uri)
        {
            var sp = ServicePointManager.FindServicePoint(uri);
            sp.ConnectionLeaseTimeout = 60 * 1000;

            return _httpClientFactory.CreateClient();
        }

        public string DownloadString(IEnumerable<string> mirrors)
        {
            logger.Debug($"Downloading string content from multiple mirrors.");
            foreach (var mirror in mirrors)
            {
                try
                {
                    return DownloadString(mirror);
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to download {mirror} string.");
                }
            }

            throw new Exception("Failed to download string from all mirrors.");
        }

        public string DownloadString(string url)
        {
            return DownloadString(url, Encoding.UTF8);
        }

        private string GetHttpRequestString(HttpRequestMessage httpRequest)
        {
            return GetHttpRequestString(httpRequest, Encoding.UTF8);
        }

        private string GetHttpRequestString(HttpRequestMessage httpRequest, Encoding encoding)
        {
            return GetHttpRequestString(httpRequest, encoding, new CancellationToken());
        }

        private string GetHttpRequestString(HttpRequestMessage httpRequest, CancellationToken cancelToken)
        {
            return GetHttpRequestString(httpRequest, Encoding.UTF8, cancelToken);
        }

        private string GetHttpRequestString(HttpRequestMessage httpRequest, Encoding encoding, CancellationToken cancelToken)
        {
            try
            {
                return Task.Run(async () =>
                {
                    using (var httpResponseMessage = await GetClient(httpRequest.RequestUri).SendAsync(httpRequest, cancelToken))
                    {
                        httpResponseMessage.EnsureSuccessStatusCode();
                        var bytes = httpResponseMessage.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                        return encoding.GetString(bytes);
                    }
                }).GetAwaiter().GetResult();
            }
            catch (HttpRequestException e)
            {
                logger.Warn(e, $"ExecuteHttpRequest not completed for url {httpRequest.RequestUri.AbsoluteUri}");
                return null;
            }
        }

        public string DownloadString(string url, CancellationToken cancelToken)
        {
            logger.Debug($"Downloading string content from {url} using UTF8 encoding.");
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                return GetHttpRequestString(request, Encoding.UTF8, cancelToken);
            }
        }

        public string DownloadString(string url, Encoding encoding)
        {
            logger.Debug($"Downloading string content from {url} using {encoding} encoding.");
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                return GetHttpRequestString(request, encoding);
            }
        }

        public string DownloadString(string url, List<Cookie> cookies)
        {
            return DownloadString(url, cookies, Encoding.UTF8);
        }

        public string DownloadString(string url, List<Cookie> cookies, Encoding encoding)
        {
            logger.Debug($"Downloading string content from {url} using cookies and {encoding} encoding.");
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                var cookieString = string.Join(";", cookies.Select(a => $"{a.Name}={a.Value}"));
                request.Headers.Add("Cookie", cookieString);
                return GetHttpRequestString(request, encoding);
            }
        }

        public string DownloadStringWithHeaders(string url, Dictionary<string, string> headersDictionary)
        {
            logger.Debug($"DownloadStringWithHeadersAsync method with url {url}");
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                foreach (var pair in headersDictionary)
                {
                    request.Headers.Add(pair.Key, pair.Value);
                }

                return GetHttpRequestString(request);
            }
        }

        public void DownloadString(string url, string path)
        {
            DownloadString(url, path, Encoding.UTF8);
        }

        public void DownloadString(string url, string path, Encoding encoding)
        {
            logger.Debug($"Downloading string content from {url} to {path} using {encoding} encoding.");
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                var data = GetHttpRequestString(request, encoding);
                File.WriteAllText(path, data);
            }
        }

        public string DownloadStringWithHeadersAsync(string url, Dictionary<string, string> headersDictionary)
        {
            logger.Debug($"DownloadStringWithHeadersAsync method with url {url}");
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                foreach (var pair in headersDictionary)
                {
                    request.Headers.Add(pair.Key, pair.Value);
                }

                return GetHttpRequestString(request);
            }
        }

        public byte[] DownloadData(string url)
        {
            logger.Debug($"Downloading data from {url}.");
            try
            {
                return Task.Run(async () =>
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        using (var httpResponseMessage = await GetClient(request.RequestUri).SendAsync(request))
                        {
                            httpResponseMessage.EnsureSuccessStatusCode();
                            return httpResponseMessage.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                        }
                    }
                }).GetAwaiter().GetResult();
            }
            catch (HttpRequestException e)
            {
                logger.Warn(e, $"DownloadData not completed for url {url}");
                return null;
            }
        }

        public bool DownloadFile(string url, string path)
        {
            return DownloadFile(url, path, new CancellationToken());
        }

        public bool DownloadFile(string url, string path, CancellationToken cancelToken)
        {
            logger.Debug($"Downloading data from {url} to {path}.");
            var success = false;
            try
            {
                Task.Run(async () =>
                {
                    using (var response = await GetClient(url).GetAsync(url, cancelToken))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            FileSystem.PrepareSaveFile(path);
                            var fileInfo = new FileInfo(path);
                            using (var fs = File.Create(fileInfo.FullName))
                            {
                                await stream.CopyToAsync(fs);
                                success = true;
                            }
                        }
                    }
                }).GetAwaiter().GetResult();
            }
            catch (HttpRequestException e)
            {
                logger.Warn(e, $"Download not completed for url {url}");
            }

            return success;
        }

        public bool DownloadFile(string url, string path, CancellationToken cancelToken, Action<DownloadProgressChangedEventArgs> progressHandler)
        {
            logger.Debug($"Downloading data from {url} to {path}.");
            FileSystem.CreateDirectory(Path.GetDirectoryName(path));
            var downloadCompleted = false;
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += (s, e) => progressHandler(e);
                    webClient.DownloadFileCompleted += (s, e) =>
                    {
                        // This event also triggers if the Cancellation Token cancels the download
                        // so we have to check if it was not what stopped the download
                        if (!cancelToken.IsCancellationRequested)
                        {
                            downloadCompleted = true;
                        }

                        webClient.Dispose();
                    };

                    using (var registration = cancelToken.Register(() => webClient.CancelAsync()))
                    {
                        webClient.DownloadFileTaskAsync(new Uri(url), path).GetAwaiter().GetResult();
                    }
                }
            }
            catch (WebException e)
            {
                logger.Warn(e, $"Download not completed for url {url}");
            }

            return downloadCompleted;
        }

        public async Task DownloadFileAsync(string url, string path, Action<DownloadProgressChangedEventArgs> progressHandler)
        {
            logger.Debug($"Downloading data async from {url} to {path}.");
            FileSystem.CreateDirectory(Path.GetDirectoryName(path));
            using (var webClient = new WebClient())
            {
                webClient.DownloadProgressChanged += (s, e) => progressHandler(e);
                webClient.DownloadFileCompleted += (s, e) => webClient.Dispose();
                await webClient.DownloadFileTaskAsync(url, path);
            }
        }

        public async Task DownloadFileAsync(IEnumerable<string> mirrors, string path, Action<DownloadProgressChangedEventArgs> progressHandler)
        {
            logger.Debug($"Downloading data async from multiple mirrors.");
            foreach (var mirror in mirrors)
            {
                try
                {
                    await DownloadFileAsync(mirror, path, progressHandler);
                    return;
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to download {mirror} file.");
                }
            }

            throw new Exception("Failed to download file from all mirrors.");
        }

        public void DownloadFile(IEnumerable<string> mirrors, string path)
        {
            logger.Debug($"Downloading data from multiple mirrors.");
            foreach (var mirror in mirrors)
            {
                try
                {
                    DownloadFile(mirror, path);
                    return;
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to download {mirror} file.");
                }
            }

            throw new Exception("Failed to download file from all mirrors.");
        }
    }
}