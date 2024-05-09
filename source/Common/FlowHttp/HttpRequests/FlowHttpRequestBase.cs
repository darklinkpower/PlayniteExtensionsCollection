using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlowHttp.Constants;
using FlowHttp.Enums;
using FlowHttp.Events;
using FlowHttp.ValueObjects;
using Playnite.SDK;

namespace FlowHttp.Requests
{
    internal abstract class FlowHttpRequestBase<T> where T : FlowHttpRequestBase<T>
    {
        protected static readonly ILogger _logger = LogManager.GetLogger();

        protected readonly HttpClientFactory _httpClientFactory;
        protected string _url;
        protected string _content;
        protected Encoding _contentEncoding = Encoding.UTF8;
        protected string _contentMediaType = HttpContentTypes.PlainText.Value;
        protected HttpMethod _httpMethod = HttpMethod.Get;
        protected readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
        protected readonly List<Cookie> _cookies = new List<Cookie>();
        protected TimeSpan? _timeout;
        protected TimeSpan _progressReportInterval = TimeSpan.FromMilliseconds(1000);

        /// <summary>
        /// Initializes a new instance of the HttpRequesT class with the specified HttpClientFactory.
        /// </summary>
        internal FlowHttpRequestBase(HttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public T WithUrl(string url)
        {
            _url = url;
            return (T)this;
        }

        public T WithContent(string content, HttpContentType httpContentType = null, Encoding encoding = null)
        {
            _content = content;
            if (encoding != null)
            {
                _contentEncoding = encoding;
            }

            if (httpContentType != null)
            {
                _contentMediaType = httpContentType.Value;
            }

            return (T)this;
        }

        public T WithHeaders(Dictionary<string, string> headers)
        {
            _headers.Clear();
            headers.ForEach(kv => _headers[kv.Key] = kv.Value);
            return (T)this;
        }

        public T AddHeader(string name, string value)
        {
            _headers[name] = value;
            return (T)this;
        }

        public T WithCookies(List<Cookie> cookies)
        {
            var cookiesClone = cookies.Select(x => new Cookie(x.Name, x.Value));
            _cookies.Clear();
            _cookies.AddRange(cookiesClone);
            return (T)this;
        }

        public T WithCookies(Dictionary<string, string> cookiesDictionary)
        {
            var cookiesClone = cookiesDictionary.Select(kvp => new Cookie(kvp.Key, kvp.Value));
            _cookies.Clear();
            _cookies.AddRange(cookiesClone);
            return (T)this;
        }

        public T WithHttpMethod(HttpMethod method)
        {
            _httpMethod = method;
            return (T)this;
        }

        public T WithGetHttpMethod()
        {
            _httpMethod = HttpMethod.Get;
            return (T)this;
        }

        public T WithPostHttpMethod()
        {
            _httpMethod = HttpMethod.Post;
            return (T)this;
        }

        public T WithHeadHttpMethod()
        {
            _httpMethod = HttpMethod.Head;
            return (T)this;
        }

        public T WithProgressReportInterval(TimeSpan reportInterval)
        {
            _progressReportInterval = reportInterval;
            return (T)this;
        }

        public T WithTimeout(double milliseconds)
        {
            _timeout = TimeSpan.FromMilliseconds(milliseconds);
            return (T)this;
        }

        public T WithTimeout(TimeSpan timeout)
        {
            _timeout = timeout;
            return (T)this;
        }

        protected void OnDownloadStateChanged(DownloadStateChangedCallback callback, HttpRequestClientStatus status)
        {
            callback?.Invoke(new DownloadStateArgs(status));
        }

        protected void OnDownloadProgressChanged(DownloadProgressChangedCallback callback, DownloadProgressArgs progressArgs)
        {
            callback?.Invoke(progressArgs);
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

    }
}