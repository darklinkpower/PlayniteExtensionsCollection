using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.Common
{
    public class HttpDownloader
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly HttpClient httpClient = new HttpClient();

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
                logger.Error(e, $"Error during file download, url {requestUri}. Error: {e.Message}.");
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
                logger.Error(e, $"Error during file download, url {requestUri}. Error: {e.Message}.");
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
                    logger.Error(e, $"Error during file download, url {requestUri}. Error: {e.Message}.");
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
                    logger.Error(e, $"Error during file download, url {requestUri}. Error: {e.Message}.");
                    return string.Empty;
                }
            }
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
    }
}