using Microsoft.Extensions.DependencyInjection;
using Playnite.SDK;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebCommon
{
    // Based on https://github.com/JosefNemec/Playnite
    public interface IDownloader
    {
        string DownloadString(IEnumerable<string> mirrors);

        DownloadStringResult DownloadString(string url);

        DownloadStringResult DownloadString(string url, List<Cookie> cookies);

        DownloadStringResult DownloadStringWithHeaders(string url, Dictionary<string, string> headersDictionary);

        DownloadStringResult DownloadStringWithHeaders(string url, Dictionary<string, string> headersDictionary, List<Cookie> cookies);

        void DownloadString(string url, string path);

        void DownloadString(string url, string path, Encoding encoding);

        byte[] DownloadData(string url);

        DownloadFileResult DownloadFile(string url, string path);

        DownloadFileResult DownloadFile(string url, string path, CancellationToken cancellationToken);

        void DownloadFile(IEnumerable<string> mirrors, string path);

        Task DownloadFileAsync(string url, string path, Action<DownloadProgressChangedEventArgs> progressHandler);

        Task DownloadFileAsync(IEnumerable<string> mirrors, string path, Action<DownloadProgressChangedEventArgs> progressHandler);
    }

    public class Downloader : IDownloader
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IHttpClientFactory _httpClientFactory;
        private static CancellationToken dummyCancelToken = new CancellationToken();

        public Downloader()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();

            //By default, the HttpClients created via HttpClientfactory use
            //a handlers with UseCookies set as true. To support adding
            //cookies via headers, we need to disable this in the requests handlers
            serviceCollection.AddHttpClient("HandlerNoUseCookies").ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    UseCookies = false,
                };
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            _httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            logger.Debug("Created service provider with IHttpClient factory");
        }

        private HttpClient GetClientForRequest(HttpRequestMessage httpRequestMessage)
        {
            if (httpRequestMessage.Headers.Contains("Cookie"))
            {
                return GetClientForCookies(httpRequestMessage.RequestUri);
            }
            else
            {
                return GetClient(httpRequestMessage.RequestUri);
            }
        }

        private HttpClient GetClient(string url)
        {
            return GetClient(new Uri(url));
        }

        private HttpClient GetClient(Uri uri)
        {
            // Needs testing
            //var sp = ServicePointManager.FindServicePoint(uri);
            //sp.ConnectionLeaseTimeout = 60 * 1000;

            return _httpClientFactory.CreateClient();
        }

        private HttpClient GetClientForCookies(string url)
        {
            return GetClientForCookies(new Uri(url));
        }

        private HttpClient GetClientForCookies(Uri uri)
        {
            // Needs testing
            //var sp = ServicePointManager.FindServicePoint(uri);
            //sp.ConnectionLeaseTimeout = 60 * 1000;

            return _httpClientFactory.CreateClient("HandlerNoUseCookies");
        }

        public string DownloadString(IEnumerable<string> mirrors)
        {
            logger.Debug($"Downloading string content from multiple mirrors.");
            foreach (var mirror in mirrors)
            {
                var downloadResult = DownloadString(mirror);
                if (downloadResult.Success)
                {
                    return downloadResult.Result;
                }
                else
                {
                    logger.Debug($"Failed to download {mirror} string.");
                }
            }

            throw new Exception("Failed to download string from all mirrors.");
        }

        private DownloadStringResult GetHttpRequestString(HttpRequestMessage httpRequest)
        {
            return GetHttpRequestString(httpRequest, dummyCancelToken);
        }

        private DownloadStringResult GetHttpRequestString(HttpRequestMessage httpRequest, CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                string result = null;
                bool success = false;
                bool wasCancelled = false;
                HttpRequestException httpRequestException = new HttpRequestException();
                HttpStatusCode httpStatusCode = HttpStatusCode.Ambiguous;

                logger.Debug($"Downloading string from {httpRequest.RequestUri} with method {httpRequest.Method}");
                try
                {
                    using (var httpResponseMessage = await GetClientForRequest(httpRequest).SendAsync(httpRequest, cancelToken))
                    {
                        try
                        {
                            httpResponseMessage.EnsureSuccessStatusCode();
                            var encoding = GetEncodingFromResponse(httpResponseMessage.Content.Headers);
                            using (var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync())
                            {
                                using (var streamReader = new StreamReader(responseStream, encoding))
                                {
                                    result = await streamReader.ReadToEndAsync();
                                    success = true;
                                }
                            }
                        }
                        catch (HttpRequestException e)
                        {
                            logger.Error(e, $"Request was not completed for url");
                            httpRequestException = e;
                        }
                        finally
                        {
                            httpStatusCode = httpResponseMessage.StatusCode;
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    wasCancelled = true;
                    logger.Debug("Request was cancelled");
                }

                return new DownloadStringResult(result, success, wasCancelled, httpStatusCode, httpRequestException);
            }).GetAwaiter().GetResult();
        }

        private Encoding GetEncodingFromResponse(HttpContentHeaders headers)
        {
            var charset = headers?.ContentType?.CharSet ?? null;
            if (charset.IsNullOrEmpty())
            {
                return Encoding.UTF8;
            }

            try
            {
                return Encoding.GetEncoding(charset);
            }
            catch
            {
                return Encoding.UTF8;
            }
        }

        public DownloadStringResult DownloadString(string url, CancellationToken cancelToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                return GetHttpRequestString(request, cancelToken);
            }
        }

        public DownloadStringResult DownloadString(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                return GetHttpRequestString(request);
            }
        }

        public DownloadStringResult DownloadString(string url, List<Cookie> cookies)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                var cookieString = string.Join(";", cookies.Select(a => $"{a.Name}={a.Value}"));
                request.Headers.Add("Cookie", cookieString);
                return GetHttpRequestString(request);
            }
        }

        public void DownloadString(string url, string path)
        {
            DownloadString(url, path, Encoding.UTF8);
        }

        public void DownloadString(string url, string path, Encoding encoding)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                var downloadStringResult = GetHttpRequestString(request);
                if (downloadStringResult.Success)
                {
                    logger.Debug($"Decoding string content from {url} to {path} using {encoding} encoding.");
                    File.WriteAllText(path, downloadStringResult.Result, encoding);
                }
            }
        }

        public DownloadStringResult DownloadStringWithHeaders(string url, Dictionary<string, string> headersDictionary)
        {
            return DownloadStringWithHeaders(url, headersDictionary, null);
        }

        public DownloadStringResult DownloadStringWithHeaders(string url, Dictionary<string, string> headersDictionary, List<Cookie> cookies)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                foreach (var pair in headersDictionary)
                {
                    if (!request.Headers.TryAddWithoutValidation(pair.Key, pair.Value))
                    {
                        logger.Warn($"Could not add header \"{pair.Key}\", \"SECRET\"");
                    }
                }

                if (cookies != null)
                {
                    var cookieString = string.Join(";", cookies.Select(a => $"{a.Name}={a.Value}"));
                    request.Headers.Add("Cookie", cookieString);
                }

                return GetHttpRequestString(request);
            }
        }

        public DownloadStringResult DownloadStringFromPostContent(string url, string jsonStringContent, Dictionary<string, string> headersDictionary = null)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                if (headersDictionary != null)
                {
                    foreach (var pair in headersDictionary)
                    {
                        if (!request.Headers.TryAddWithoutValidation(pair.Key, pair.Value))
                        {
                            logger.Warn($"Could not add header \"{pair.Key}\", \"SECRET\"");
                        }
                    }
                }
                
                using (var stringContent = new StringContent(jsonStringContent, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;
                    return GetHttpRequestString(request);
                }
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
                            return await httpResponseMessage.Content.ReadAsByteArrayAsync();
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

        public DownloadFileResult DownloadFile(string url, string path)
        {
            return DownloadFile(url, path, dummyCancelToken);
        }

        public DownloadFileResult DownloadFile(string url, string path, CancellationToken cancelToken)
        {
            logger.Debug($"Downloading file from {url} to {path}.");
            return Task.Run(async () =>
            {
                var success = false;
                var wasCancelled = false;
                Exception exception = null;
                var fileLocation = path;
                var httpStatusCode = HttpStatusCode.Ambiguous;
                long fileSize = -1;

                try
                {
                    using (var response = await GetClient(url).GetAsync(url, cancelToken))
                    {
                        httpStatusCode = response.StatusCode;
                        response.EnsureSuccessStatusCode();
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            FileSystem.PrepareSaveFile(path);
                            var fileInfo = new FileInfo(path);
                            using (var fs = File.Create(fileInfo.FullName))
                            {
                                await stream.CopyToAsync(fs);
                                success = true;
                                fileSize = stream.Position;
                            }
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    wasCancelled = true;
                    logger.Debug("Request was cancelled");
                }
                catch (Exception e)
                {
                    exception = e;
                    logger.Error(e, "Error while downloading file");
                }

                return new DownloadFileResult(fileLocation, success, wasCancelled, fileSize, httpStatusCode, exception);
            }).GetAwaiter().GetResult();
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
                if (DownloadFile(mirror, path).Success)
                {
                    return;
                }
                else
                {
                    logger.Debug($"Failed to download {mirror} file.");
                }
            }

            throw new Exception("Failed to download file from all mirrors.");
        }

        public HttpClient GetHttpClientInstance()
        {
            return _httpClientFactory.CreateClient();
        }

        public HttpClient GetHttpClientInstanceHandlerNoUseCookies()
        {
            return _httpClientFactory.CreateClient("HandlerNoUseCookies");
        }
    }
}