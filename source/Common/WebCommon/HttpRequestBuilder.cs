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
using WebCommon.Models;

namespace WebCommon
{
    /// <summary>
    /// A builder class for constructing HTTP requests, including various options and settings.
    /// </summary>
    internal class HttpRequestBuilder
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly HttpClientFactory _httpClientFactory;
        private readonly List<string> _urls = new List<string>();
        private string _content;
        private Encoding _contentEncoding = Encoding.UTF8;
        private string _contentMediaType = StandardMediaTypesConstants.PlainText;
        private HttpMethod _httpMethod = HttpMethod.Get;
        private CancellationToken _cancellationToken = CancellationToken.None;
        private Dictionary<string, string> _headers;
        private readonly List<Cookie> _cookies = new List<Cookie>();
        private string _filePath;
        private TimeSpan? _timeout;
        private IProgress<DownloadProgressReport> _progressReporter;
        private TimeSpan _progressReportInterval = TimeSpan.FromMilliseconds(1000);

        /// <summary>
        /// Initializes a new instance of the HttpRequestBuilder class with the specified HttpClientFactory.
        /// </summary>
        internal HttpRequestBuilder(HttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public HttpRequestBuilder WithUrls(IEnumerable<string> urls)
        {
            _urls.AddRange(urls);
            return this;
        }

        public HttpRequestBuilder WithUrl(string url)
        {
            _urls.Add(url);
            return this;
        }

        public HttpRequestBuilder WithContent(string content, string mediaType = null, Encoding encoding = null)
        {
            _content = content;

            if (!(encoding is null))
            {
                _contentEncoding = encoding;
            }

            if (!(mediaType is null))
            {
                _contentMediaType = mediaType;
            }

            return this;
        }

        public HttpRequestBuilder WithCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return this;
        }

        public HttpRequestBuilder WithHeaders(Dictionary<string, string> headers)
        {
            _headers = headers;
            return this;
        }

        public HttpRequestBuilder WithCookies(List<Cookie> cookies)
        {
            _cookies.AddRange(cookies);
            return this;
        }

        public HttpRequestBuilder WithCookies(Dictionary<string, string> cookiesDictionary)
        {
            _cookies.AddRange(cookiesDictionary.Select(kvp => new Cookie(kvp.Key, kvp.Value)));
            return this;
        }

        public HttpRequestBuilder WithHttpMethod(HttpMethod method)
        {
            _httpMethod = method;
            return this;
        }

        public HttpRequestBuilder WithGetHttpMethod()
        {
            _httpMethod = HttpMethod.Get;
            return this;
        }

        public HttpRequestBuilder WithPostHttpMethod()
        {
            _httpMethod = HttpMethod.Post;
            return this;
        }

        public HttpRequestBuilder WithHeadHttpMethod()
        {
            _httpMethod = HttpMethod.Head;
            return this;
        }

        public HttpRequestBuilder WithDownloadTo(string filePath)
        {
            _filePath = filePath;
            return this;
        }

        /// <summary>
        /// Sets a progress reporter for tracking and reporting download progress during the HTTP download operation.
        /// </summary>
        /// <param name="downloadProgressReporter">An instance of IProgress&lt;DownloadProgressReport&gt; for reporting download progress.</param>
        /// <returns>The HttpRequestBuilder instance to continue configuring the download operation.</returns>
        public HttpRequestBuilder WithProgressReporter(IProgress<DownloadProgressReport> downloadProgressReporter)
        {
            _progressReporter = downloadProgressReporter;
            return this;
        }

        public HttpRequestBuilder WithProgressReportInterval(TimeSpan reportInterval)
        {
            _progressReportInterval = reportInterval;
            return this;
        }

        public HttpRequestBuilder WithTimeout(double milliseconds)
        {
            _timeout = TimeSpan.FromMilliseconds(milliseconds);
            return this;
        }

        public HttpRequestBuilder WithTimeout(TimeSpan timeout)
        {
            _timeout = timeout;
            return this;
        }

        /// <summary>
        /// Synchronously downloads content from the provided URLs and returns the result as an HttpDownloaderResult.
        /// </summary>
        /// <returns>An HttpDownloaderResult representing the result of the download operation.</returns>
        public StringHttpDownloaderResult DownloadString()
        {
            return Task.Run(() => DownloadStringAsync()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously downloads content from the provided URLs and returns the result as an HttpDownloaderResult.
        /// </summary>
        /// <returns>An awaitable Task that represents the asynchronous operation. The task's result is the HttpDownloaderResult.</returns>
        public async Task<StringHttpDownloaderResult> DownloadStringAsync()
        {
            if (_urls.Count == 0)
            {
                throw new InvalidOperationException("No URLs provided.");
            }

            var result = new StringHttpDownloaderResult();
            foreach (var url in _urls)
            {
#if DEBUG
                _logger.Info($"Starting download of url \"{url}\"...");
#endif

                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken))
                {
                    if (!(_timeout is null))
                    {
                        cts.CancelAfter(_timeout.Value);
                    }

                    StringContent stringContent = null;
                    if (!string.IsNullOrEmpty(_content))
                    {
                        stringContent = new StringContent(_content, _contentEncoding, _contentMediaType);
                    }

                    try
                    {
                        using (var request = CreateRequest(url, stringContent))
                        {
                            var httpClient = _httpClientFactory.GetClient(request);
                            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token))
                            {
                                result.StatusCode = response.StatusCode;
                                result.IsSuccessful = response.IsSuccessStatusCode;

                                if (result.IsSuccessful)
                                {
                                    result.Response.Content = await ReadResponseContent(response, cts.Token);
                                    RecordResponseData(result.Response, response);
                                    break;
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        HandleCancellation(ex, result);
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex, result);
                    }
                    finally
                    {
                        stringContent?.Dispose();
                    }
                }
            }

            return result;
        }

        private static void RecordResponseData(BaseResponse response, HttpResponseMessage responseMessage)
        {
            var responseCookies = new List<Cookie>();
            if (responseMessage.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
            {
                foreach (var cookieString in setCookieHeaders)
                {
                    if (CookiesUtilities.TryParseCookieFromString(cookieString, out var cookie))
                    {
                        responseCookies.Add(cookie);
                    }
                }
            }

            response.Cookies = responseCookies;
            response.Headers = responseMessage.Headers?.ToDictionary(h => h.Key, h => h.Value);
            response.ContentHeaders = responseMessage.Content?.Headers?.ToDictionary(h => h.Key, h => h.Value);
        }

        /// <summary>
        /// Synchronously downloads content from the provided URLs and saves it to the specified file path. Returns an HttpDownloaderResult.
        /// </summary>
        /// <returns>The HttpDownloaderResult representing the result of the download and file-saving operation.</returns>
        public FileDownloadHttpDownloaderResult DownloadFile()
        {
            return Task.Run(() => DownloadFileAsync()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously downloads content from the provided URLs and saves it to the specified file path. Returns an HttpDownloaderResult.
        /// </summary>
        /// <returns>An awaitable Task that represents the asynchronous download and file-saving operation. The task's result is the HttpDownloaderResult.</returns>
        public async Task<FileDownloadHttpDownloaderResult> DownloadFileAsync()
        {
            if (_urls.Count == 0)
            {
                throw new InvalidOperationException("No URLs provided.");
            }

            if (string.IsNullOrEmpty(_filePath))
            {
                throw new InvalidOperationException("File path must be provided for downloading a file.");
            }

            var result = new FileDownloadHttpDownloaderResult();
            foreach (var url in _urls)
            {
#if DEBUG
                _logger.Info($"Starting download of url \"{url}\" to \"{_filePath}\"...");
#endif

                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken))
                {
                    if (!(_timeout is null))
                    {
                        cts.CancelAfter(_timeout.Value);
                    }

                    StringContent stringContent = null;
                    if (!string.IsNullOrEmpty(_content))
                    {
                        stringContent = new StringContent(_content, _contentEncoding, _contentMediaType);
                    }

                    try
                    {
                        using (var request = CreateRequest(url, stringContent))
                        {
                            var httpClient = _httpClientFactory.GetClient(request);
                            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token))
                            {
                                result.StatusCode = response.StatusCode;
                                result.IsSuccessful = response.IsSuccessStatusCode;

                                if (result.IsSuccessful)
                                {
                                    await SaveFileContent(result, response, cts.Token);
                                    RecordResponseData(result.Response, response);
                                    break;
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        HandleCancellation(ex, result);
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex, result);
                    }
                    finally
                    {
                        stringContent?.Dispose();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates an HTTP request message for the specified URL, using configured HTTP method, headers, and content.
        /// </summary>
        /// <param name="url">The URL for the HTTP request.</param>
        /// <returns>An HttpRequestMessage instance representing the HTTP request.</returns>
        private HttpRequestMessage CreateRequest(string url, StringContent stringContent)
        {
            var request = new HttpRequestMessage(_httpMethod, url);
            if (_headers != null)
            {
                foreach (var header in _headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (_cookies.Count > 0)
            {
                var cookieString = string.Join(";", _cookies.Select(a => $"{a.Name}={a.Value}"));
                request.Headers.Add("Cookie", cookieString);
            }

            if (!(stringContent is null))
            {
                request.Content = stringContent;
            }

            return request;
        }

        /// <summary>
        /// Reads and retrieves the content from the response message using the retrieved encoding.
        /// </summary>
        /// <param name="response">The HttpResponseMessage containing the response content to read.</param>
        /// <returns>The content as a string.</returns>
        private async Task<string> ReadResponseContent(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var encoding = GetEncodingFromHeaders(response.Content.Headers);
            var startTime = DateTime.Now;
            var lastReportTime = startTime;
            var totalBytesToReceive = response.Content.Headers.ContentLength ?? 0;
            var sbCapacity = (totalBytesToReceive >= int.MinValue && totalBytesToReceive <= int.MaxValue) ? (int)totalBytesToReceive : 0;
            var stringBuilder = totalBytesToReceive > 0 ? new StringBuilder(sbCapacity) : new StringBuilder();
            using (var contentStream = await response.Content.ReadAsStreamAsync())
            {
                using (var streamReader = new StreamReader(contentStream, encoding))
                {
                    int bytesRead;
                    var buffer = new char[4096];
                    long totalBytesRead = 0;
                    long lastTotalBytesRead = 0;

                    while ((bytesRead = await streamReader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        stringBuilder.Append(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;

                        var currentTime = DateTime.Now;
                        if (_progressReporter != null && currentTime - lastReportTime >= _progressReportInterval)
                        {
                            ReportProgress(totalBytesRead, totalBytesToReceive, startTime, currentTime, lastReportTime, lastTotalBytesRead);
                            lastReportTime = currentTime;
                            lastTotalBytesRead = totalBytesRead;
                        }
                    }
                }
            }

#if DEBUG
            _logger.Info("Download completed successfully.");
#endif

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Saves the content of the HttpResponseMessage to a file, while reporting progress if applicable.
        /// </summary>
        /// <param name="result">The HttpDownloaderResult to update with file-related information.</param>
        /// <param name="response">The HttpResponseMessage containing the content to save.</param>
        private async Task SaveFileContent(FileDownloadHttpDownloaderResult result, HttpResponseMessage response, CancellationToken cancellationToken)
        {
            FileSystem.PrepareSaveFile(_filePath);
            var startTime = DateTime.Now;
            var lastReportTime = startTime;
            var totalBytesToReceive = response.Content.Headers.ContentLength ?? 0;
            using (var fs = new FileStream(_filePath, FileMode.Create))
            {
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    long totalBytesRead = 0;
                    long lastTotalBytesRead = 0;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException();
                        }

                        await fs.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;

                        var currentTime = DateTime.Now;
                        if (_progressReporter != null && currentTime - lastReportTime >= _progressReportInterval)
                        {
                            ReportProgress(totalBytesRead, totalBytesToReceive, startTime, currentTime, lastReportTime, lastTotalBytesRead);
                            lastReportTime = currentTime;
                            lastTotalBytesRead = totalBytesRead;
                        }
                    }
                }
            }

            result.FilePath = _filePath;
            result.FileSize = new FileInfo(_filePath).Length;
#if DEBUG
            _logger.Info("Download completed successfully.");
#endif
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
        private void ReportProgress(long bytesRead, long totalBytesToReceive, DateTime startTime, DateTime currentTime, DateTime lastReportTime, long lastTotalBytesRead)
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

            var progress = new DownloadProgressReport(bytesRead, totalBytesToReceive, timeElapsed, timeRemaining, downloadSpeedBytesPerSecond);
            _progressReporter.Report(progress);
        }

        /// <summary>
        /// Calculates the estimated time remaining for a download operation based on time elapsed, bytes received, and total bytes to receive.
        /// </summary>
        /// <param name="timeElapsed">The time elapsed during the download.</param>
        /// <param name="bytesReceived">The number of bytes received.</param>
        /// <param name="totalBytesToReceive">The total number of bytes to receive.</param>
        /// <returns>The estimated time remaining for the download operation.</returns>
        private TimeSpan CalculateTimeRemaining(TimeSpan timeElapsed, long bytesReceived, long totalBytesToReceive)
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
        /// Handles an OperationCanceledException by logging a message and updating the result object.
        /// </summary>
        /// <param name="ex">The OperationCanceledException that occurred.</param>
        /// <param name="result">The HttpDownloaderResult to update with the exception details.</param>
        private void HandleCancellation(OperationCanceledException ex, BaseHttpDownloaderResult result)
        {
            _logger.Info("Operation timed out or was cancelled");
            result.IsSuccessful = false;
            result.Exception = ex;
            _logger.Error(ex, "Download failed");
        }

        /// <summary>
        /// Handles an exception by logging a message and updating the result object.
        /// </summary>
        /// <param name="ex">The exception that occurred.</param>
        /// <param name="result">The HttpDownloaderResult to update with the exception details.</param>
        private void HandleException(Exception ex, BaseHttpDownloaderResult result)
        {
            result.IsSuccessful = false;
            result.Exception = ex;
            _logger.Error(ex, "Download failed");
        }

        /// <summary>
        /// Retrieves the character encoding from the HTTP content headers, falling back to UTF-8 if not specified.
        /// </summary>
        /// <param name="headers">The HTTP content headers that may include character encoding information.</param>
        /// <returns>The retrieved character encoding or UTF-8 if not specified.</returns>
        private Encoding GetEncodingFromHeaders(HttpContentHeaders headers)
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