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
        public event EventHandler<DownloadStateArgs> DownloadStateChanged;
        public event EventHandler<DownloadProgressArgs> DownloadProgressChanged;

        protected virtual void OnDownloadStateChanged(DownloadStateArgs e)
        {
            DownloadStateChanged?.Invoke(this, e);
        }

        protected virtual void OnDownloadProgressChanged(DownloadProgressArgs progressReport)
        {
            DownloadProgressChanged?.Invoke(this, progressReport);
        }

        protected ManualResetEventSlim _pauseEvent;
        protected bool _isPaused = false;
        protected HttpRequestClientStatus _state = HttpRequestClientStatus.Idle;

        public HttpRequestClientStatus Status
        {
            get { return _state; }
            protected set
            {
                if (_state != value)
                {
                    _state = value;
                    var eventArgs = new DownloadStateArgs(_state);
                    OnDownloadStateChanged(eventArgs);
                }
            }
        }

        protected static readonly ILogger _logger = LogManager.GetLogger();
        protected readonly HttpClientFactory _httpClientFactory;
        protected readonly string _url;
        protected string _content;
        protected Encoding _contentEncoding;
        protected string _contentMediaType;
        protected HttpMethod _httpMethod;
        protected CancellationToken _cancellationToken;
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
            CancellationToken cancellationToken,
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
            _cancellationToken = cancellationToken;
            _headers = new Dictionary<string, string>(headers ?? new Dictionary<string, string>());
            _cookies = (cookies ?? Enumerable.Empty<Cookie>()).ToList();
            _timeout = timeout;
            _progressReportInterval = progressReportInterval;
        }

        public void Pause()
        {
            if (_state == HttpRequestClientStatus.Downloading && _pauseEvent != null)
            {
                _pauseEvent?.Reset(); // Set the event to block the download loop
                _isPaused = true;
                Status = HttpRequestClientStatus.Paused;
            }
        }

        public void Resume()
        {
            if (_state == HttpRequestClientStatus.Paused && _pauseEvent != null)
            {
                _pauseEvent?.Set(); // Allow the download loop to continue
                _isPaused = false;
                Status = HttpRequestClientStatus.Downloading;
            }
        }

        public bool IsPaused()
        {
            return _isPaused;
        }

        public ManualResetEventSlim GetPauseEvent()
        {
            return _pauseEvent;
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
        /// <param name="bytesRead">The number of bytes received.</param>
        /// <param name="totalBytesToReceive">The total number of bytes to receive.</param>
        /// <param name="startTime">The start time of the download operation.</param>
        /// <param name="currentTime">The current time for progress reporting.</param>
        /// <param name="lastReportTime">The time of the last progress report.</param>
        /// <param name="lastTotalBytesRead">The total bytes received at the time of the last report.</param>
        protected void ReportProgress(long bytesRead, long totalBytesToReceive, DateTime startTime, DateTime currentTime, DateTime lastReportTime, long lastTotalBytesRead)
        {
            var timeElapsed = currentTime - startTime;
            var timeRemaining = CalculateTimeRemaining(timeElapsed, bytesRead, totalBytesToReceive);

            var bytesReadThisInterval = bytesRead - lastTotalBytesRead;
            var intervalElapsedTime = currentTime - lastReportTime;
            long downloadSpeedBytesPerSecond = 0;
            if (intervalElapsedTime.TotalSeconds > 0)
            {
                downloadSpeedBytesPerSecond = (long)Math.Round(bytesReadThisInterval / intervalElapsedTime.TotalSeconds);
            }

            var progressReport = new DownloadProgressArgs(bytesRead, totalBytesToReceive, timeElapsed, timeRemaining, downloadSpeedBytesPerSecond);
            OnDownloadProgressChanged(progressReport);
        }

        /// <summary>
        /// Calculates the estimated time remaining for a download operation based on time elapsed, bytes received, and total bytes to receive.
        /// </summary>
        /// <param name="timeElapsed">The time elapsed during the download.</param>
        /// <param name="bytesReceived">The number of bytes received.</param>
        /// <param name="totalBytesToReceive">The total number of bytes to receive.</param>
        /// <returns>The estimated time remaining for the download operation.</returns>
        protected TimeSpan CalculateTimeRemaining(TimeSpan timeElapsed, long bytesReceived, long totalBytesToReceive)
        {
            if (bytesReceived == 0 || totalBytesToReceive == 0)
            {
                return TimeSpan.MaxValue; // Unable to calculate time remaining
            }

            var bytesRemaining = totalBytesToReceive - bytesReceived;
            var bytesPerSecond = bytesReceived / timeElapsed.TotalSeconds;

            return TimeSpan.FromSeconds(bytesRemaining / bytesPerSecond);
        }

        /// <summary>
        /// Handles an OperationCanceledException by logging a message.
        /// </summary>
        /// <param name="ex">The OperationCanceledException that occurred.</param>
        protected void HandleCancellation(OperationCanceledException ex)
        {
            Status = HttpRequestClientStatus.Canceled;
            _logger.Info("Operation timed out or was cancelled");
        }

        /// <summary>
        /// Handles an exception by logging a message.
        /// </summary>
        /// <param name="ex">The exception that occurred.</param>
        protected void HandleException(Exception ex)
        {
            Status = HttpRequestClientStatus.Failed;
            _logger.Error(ex, "Download failed");
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