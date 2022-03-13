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

namespace PluginsCommon.Web
{
    // Based on https://github.com/JosefNemec/Playnite
    public class HttpDownloader
    {
        private static ILogger logger = LogManager.GetLogger();
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly HttpClient httpClientJson = new HttpClient();
        private static readonly Downloader downloader = new Downloader();

        public static string DownloadString(IEnumerable<string> mirrors)
        {
            return downloader.DownloadString(mirrors);
        }

        public static string DownloadString(string url)
        {
            return downloader.DownloadString(url);
        }

        public static string DownloadString(string url, CancellationToken cancelToken)
        {
            return downloader.DownloadString(url, cancelToken);
        }

        public static string DownloadString(string url, Encoding encoding)
        {
            return downloader.DownloadString(url, encoding);
        }

        public static string DownloadString(string url, List<Cookie> cookies)
        {
            return downloader.DownloadString(url, cookies);
        }

        public static string DownloadString(string url, List<Cookie> cookies, Encoding encoding)
        {
            return downloader.DownloadString(url, cookies, encoding);
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

        public static void DownloadFile(string url, string path)
        {
            downloader.DownloadFile(url, path);
        }

        public static void DownloadFile(string url, string path, CancellationToken cancelToken)
        {
            downloader.DownloadFile(url, path, cancelToken);
        }

        public static HttpStatusCode GetResponseCode(string url)
        {
            try
            {
                var response = httpClient.GetAsync(url).GetAwaiter().GetResult();
                return response.StatusCode;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to get HTTP response for {url}.");
                return HttpStatusCode.ServiceUnavailable;
            }
        }

        public static async Task<bool> DownloadFileAsync(string requestUri, string fileToWriteTo)
        {
            logger.Debug($"DownloadFileAsync method with url {requestUri} and file to write {fileToWriteTo}");
            try
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                        {
                            using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                            {
                                await streamToReadFrom.CopyToAsync(streamToWriteTo);
                                logger.Debug("Ran to completion");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        logger.Debug($"Request Url {requestUri} didn't give OK status code and was not downloaded");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error during file download, url {requestUri}.");
            }

            return false;
        }

        public static async Task<bool> DownloadJsonFileAsync(string requestUri, string fileToWriteTo)
        {
            logger.Debug($"DownloadJsonFileAsync method with url {requestUri} and file to write {fileToWriteTo}");
            if (httpClientJson.DefaultRequestHeaders.Count() == 0)
            {
                httpClientJson.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.Timeout = TimeSpan.FromMilliseconds(2000);
            }
            try
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                        {
                            using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                            {
                                await streamToReadFrom.CopyToAsync(streamToWriteTo);
                                logger.Debug("Ran to completion");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        logger.Debug($"Request Url {requestUri} didn't give OK status code and was not downloaded");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error during file download, url {requestUri}.");
            }

            return false;
        }

        public static async Task<string> DownloadStringAsync(string requestUri)
        {
            logger.Debug($"DownloadStringAsync method with url {requestUri}");
            try
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    using (HttpContent content = response.Content)
                    {
                        logger.Debug("Ran to completion");
                        return content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error during file download, url {requestUri}.");
                return string.Empty;
            }
        }

        public static async Task<bool> DownloadFileWithHeadersAsync(string requestUri, string fileToWriteTo, Dictionary<string, string> headersDictionary)
        {
            logger.Debug($"DownloadFileWithHeadersAsync method with url {requestUri} and file to write {fileToWriteTo}");
            using (var request = new HttpRequestMessage(HttpMethod.Put, requestUri))
            {
                foreach (var pair in headersDictionary)
                {
                    request.Headers.Add(pair.Key, pair.Value);
                }
                try
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                            {
                                using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                                {
                                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
                                    logger.Debug("Ran to completion");
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            logger.Debug($"Request Url {requestUri} didn't give OK status code and was not downloaded");
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Error during file download, url {requestUri}.");
                }
                return false;
            }
        }

        public static async Task<string> DownloadStringWithHeadersAsync(string requestUri, Dictionary<string, string> headersDictionary)
        {
            logger.Debug($"DownloadStringWithHeadersAsync method with url {requestUri}");
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                foreach (var pair in headersDictionary)
                {
                    request.Headers.Add(pair.Key, pair.Value);
                }
                try
                {
                    using (HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                    {
                        using (HttpContent content = response.Content)
                        {
                            logger.Debug("Ran to completion");
                            return content.ReadAsStringAsync().Result;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Error during file download, url {requestUri}.");
                    return string.Empty;
                }
            }
        }
    }
}