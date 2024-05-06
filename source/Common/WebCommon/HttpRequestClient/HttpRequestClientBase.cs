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
using WebCommon.Enums;
using WebCommon.HttpRequestClient.Events;

namespace WebCommon.HttpRequestClient
{
    public abstract class HttpRequestClientBase
    {
        protected static readonly ILogger _logger = LogManager.GetLogger();
        protected readonly HttpClientFactory _httpClientFactory;
        protected string _url;
        protected string _content;
        protected Encoding _contentEncoding;
        protected string _contentMediaType;
        protected HttpMethod _httpMethod;
        protected Dictionary<string, string> _headers;
        protected readonly IEnumerable<Cookie> _cookies;
        protected TimeSpan? _timeout;
        protected TimeSpan _progressReportInterval;

        internal HttpRequestClientBase(
            HttpClientFactory httpClientFactory,
            string url,
            string content,
            Encoding contentEncoding,
            string contentMediaType,
            HttpMethod httpMethod,
            Dictionary<string, string> headers,
            List<Cookie> cookies,
            TimeSpan? timeout,
            TimeSpan progressReportInterval)
        {
            _httpClientFactory = httpClientFactory;
            _url = url;
            _content = content;
            _contentEncoding = contentEncoding;
            _contentMediaType = contentMediaType;
            _httpMethod = httpMethod;
            _headers = new Dictionary<string, string>(headers ?? new Dictionary<string, string>());
            _cookies = (cookies ?? Enumerable.Empty<Cookie>()).ToList();
            _timeout = timeout;
            _progressReportInterval = progressReportInterval;
        }

        public void SetUrl(string url)
        {
            _url = url;
        }

        public void SetUrl(Uri url)
        {
            _url = url.ToString();
        }

        /// <summary>
        /// Creates an HTTP request message for the specified URL, using configured HTTP method, headers, and content.
        /// </summary>
        /// <param name="url">The URL for the HTTP request.</param>
        /// <returns>An HttpRequestMessage instance representing the HTTP request.</returns>
        protected HttpRequestMessage CreateRequest(string url, StringContent stringContent, long resumeOffset = 0)
        {
            var request = new HttpRequestMessage(_httpMethod, url);
            if (_headers != null)
            {
                foreach (var header in _headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (_cookies.Count() > 0)
            {
                var cookieString = string.Join(";", _cookies.Select(a => $"{a.Name}={a.Value}"));
                request.Headers.Add("Cookie", cookieString);
            }

            if (!(stringContent is null))
            {
                request.Content = stringContent;
            }

            if (resumeOffset > 0)
            {
                request.Headers.Range = new RangeHeaderValue(resumeOffset, null);
                // No idea why but this is needed or there will be an error during download if not set to Close
                // "Unable to read data from the transport connection:An existing connection was forcibly closed by the remote host"
                // https://stackoverflow.com/a/11326290
                request.Headers.ConnectionClose = true;
            }

            return request;
        }

        /// <summary>
        /// Reports download progress to the HttpDownload progress reporter based on bytes received, total bytes to receive, and time measurements.
        /// </summary>
        /// <param name="contentProgressLength">The number of bytes received.</param>
        /// <param name="totalContentLength">The total number of bytes to receive.</param>
        /// <param name="startTime">The start time of the download operation.</param>
        /// <param name="currentTime">The current time for progress reporting.</param>
        /// <param name="lastReportTime">The time of the last progress report.</param>
        /// <param name="lastTotalBytesRead">The total bytes received at the time of the last report.</param>
        protected void ReportProgress(DownloadProgressChangedCallback progressChangedCallback, long contentProgressLength, long totalContentLength, DateTime startTime, DateTime currentTime, DateTime lastReportTime, long lastTotalBytesRead)
        {
            var totalElapsedDownloadTime = currentTime - startTime;
            var intervalElapsedTime = currentTime - lastReportTime;
            var bytesReadThisInterval = contentProgressLength - lastTotalBytesRead;
            var progressReport = new DownloadProgressArgs(contentProgressLength, totalContentLength, totalElapsedDownloadTime, intervalElapsedTime, bytesReadThisInterval);
            OnDownloadProgressChanged(progressChangedCallback, progressReport);
        }

        /// <summary>
        /// Retrieves the character encoding from the HTTP content headers, falling back to UTF-8 if not specified.
        /// </summary>
        /// <param name="headers">The HTTP content headers that may include character encoding information.</param>
        /// <returns>The retrieved character encoding or UTF-8 if not specified.</returns>
        protected Encoding GetEncodingFromHeaders(HttpContentHeaders headers)
        {
            var charset = headers?.ContentType?.CharSet ?? null;
            if (!string.IsNullOrEmpty(charset))
            {
                try
                {
                    return Encoding.GetEncoding(charset);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to get encoding from headers with \"{charset}\" charset");
                }
            }

            return Encoding.UTF8;
        }

        protected void OnDownloadStateChanged(DownloadStateChangedCallback callback, HttpRequestClientStatus status)
        {
            callback?.Invoke(new DownloadStateArgs(status));
        }

        protected void OnDownloadProgressChanged(DownloadProgressChangedCallback callback, DownloadProgressArgs progressArgs)
        {
            callback?.Invoke(progressArgs);
        }
    }

}