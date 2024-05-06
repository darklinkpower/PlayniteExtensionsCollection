using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebCommon.Enums;
using WebCommon.HttpRequestClient.Events;
using WebCommon.Results;

namespace WebCommon.HttpRequestClient
{
    public class DownloadStringClient : HttpRequestClientBase
    {
        internal DownloadStringClient(
            HttpClientFactory httpClientFactory,
            string url,
            string content,
            Encoding contentEncoding,
            string contentMediaType,
            HttpMethod httpMethod,
            Dictionary<string, string> headers,
            List<Cookie> cookies,
            TimeSpan? timeout,
            TimeSpan progressReportInterval
        ) : base(
            httpClientFactory,
            url,
            content,
            contentEncoding,
            contentMediaType,
            httpMethod,
            headers,
            cookies,
            timeout,
            progressReportInterval
        )
        {

        }

        public HttpContentResult<string> DownloadString(CancellationToken cancellationToken = default, DownloadStateController downloadStateController = null, DownloadStateChangedCallback stateChangedCallback = null, DownloadProgressChangedCallback progressChangedCallback = null)
        {
            return Task.Run(() => DownloadStringAsync(cancellationToken, downloadStateController, stateChangedCallback, progressChangedCallback)).GetAwaiter().GetResult();
        }

        public async Task<HttpContentResult<string>> DownloadStringAsync(CancellationToken cancellationToken = default, DownloadStateController downloadStateController = null, DownloadStateChangedCallback stateChangedCallback = null, DownloadProgressChangedCallback progressChangedCallback = null)
        {
#if DEBUG
            _logger.Info($"Starting download of url \"{_url}\"...");
#endif

            OnDownloadStateChanged(stateChangedCallback, HttpRequestClientStatus.Downloading);
            Exception error = null;
            HttpStatusCode? httpStatusCode = null;
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, downloadStateController?.CancellationToken ?? CancellationToken.None))
            {
                if (_timeout != null)
                {
                    cts.CancelAfter(_timeout.Value);
                }

                StringContent stringContent = null;
                try
                {
                    if (!string.IsNullOrEmpty(_content))
                    {
                        stringContent = new StringContent(_content, _contentEncoding, _contentMediaType);
                    }

                    using (var request = CreateRequest(_url, stringContent))
                    {
                        var httpClient = _httpClientFactory.GetClient(request);
                        using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token))
                        {
                            httpStatusCode = response.StatusCode;
                            if (response.IsSuccessStatusCode)
                            {
                                var content = await ReadResponseContent(response, cts.Token, downloadStateController, stateChangedCallback, progressChangedCallback);
                                OnDownloadStateChanged(stateChangedCallback, HttpRequestClientStatus.Completed);
                                return HttpContentResult<string>.Success(_url, content, httpStatusCode, response);
                            }
                        }
                    }
                }
                catch (OperationCanceledException ex)
                {
                    error = ex;
                    _logger.Error(ex, "Operation timed out or was cancelled");
                    OnDownloadStateChanged(stateChangedCallback, HttpRequestClientStatus.Canceled);
                }
                catch (Exception ex)
                {
                    error = ex;
                    _logger.Error(ex, "Download failed");
                    OnDownloadStateChanged(stateChangedCallback, HttpRequestClientStatus.Failed);
                }
                finally
                {
                    stringContent?.Dispose();
                }

                return HttpContentResult<string>.Failure(_url, error, httpStatusCode);
            }
        }

        /// <summary>
        /// Reads and retrieves the content from the response message using the retrieved encoding.
        /// </summary>
        /// <param name="response">The HttpResponseMessage containing the response content to read.</param>
        /// <returns>The content as a string.</returns>
        protected async Task<string> ReadResponseContent(HttpResponseMessage response, CancellationToken cancellationToken, DownloadStateController downloadStateController, DownloadStateChangedCallback stateChangedCallback, DownloadProgressChangedCallback progressChangedCallback)
        {
            var encoding = GetEncodingFromHeaders(response.Content.Headers);
            var startTime = DateTime.Now;
            var lastReportTime = startTime;
            var totalContentLength = response.Content.Headers.ContentLength ?? 0;
            var sbCapacity = (totalContentLength >= int.MinValue && totalContentLength <= int.MaxValue) ? (int)totalContentLength : 0;
            var stringBuilder = totalContentLength > 0 ? new StringBuilder(sbCapacity) : new StringBuilder();
            using (var contentStream = await response.Content.ReadAsStreamAsync())
            {
                using (var streamReader = new StreamReader(contentStream, encoding))
                {
                    int bytesRead;
                    var buffer = new char[4096];
                    long contentProgressLength = 0;
                    long lastTotalBytesRead = 0;

                    while ((bytesRead = await streamReader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        var shouldPause = downloadStateController?.IsPaused() == true;
                        cancellationToken.ThrowIfCancellationRequested();
                        if (shouldPause)
                        {
                            OnDownloadStateChanged(stateChangedCallback, HttpRequestClientStatus.Paused);
                            await downloadStateController.PauseAsync();
                            cancellationToken.ThrowIfCancellationRequested();
                            OnDownloadStateChanged(stateChangedCallback, HttpRequestClientStatus.Downloading);
                        }

                        stringBuilder.Append(buffer, 0, bytesRead);
                        contentProgressLength += bytesRead;
                        if (progressChangedCallback != null)
                        {
                            var currentTime = DateTime.Now;
                            var isCompleted = contentProgressLength == totalContentLength;
                            if (isCompleted || currentTime - lastReportTime >= _progressReportInterval)
                            {
                                ReportProgress(progressChangedCallback, contentProgressLength, totalContentLength, startTime, currentTime, lastReportTime, lastTotalBytesRead);
                                lastReportTime = currentTime;
                                lastTotalBytesRead = contentProgressLength;
                            }
                        }
                    }
                }
            }

#if DEBUG
            _logger.Info("Download completed successfully.");
#endif

            return stringBuilder.ToString();
        }


    }
}