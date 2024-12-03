using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlowHttp.Enums;
using FlowHttp.Events;
using FlowHttp.Results;

namespace FlowHttp.Requests
{
    internal class FlowHttpRequest : FlowHttpRequestBase<FlowHttpRequest>
    {
        internal FlowHttpRequest(HttpClientFactory httpClientFactory) : base(httpClientFactory)
        {

        }


        internal HttpContentResult<string> DownloadString(CancellationToken cancellationToken = default, DownloadStateController downloadStateController = null, DownloadStateChangedCallback stateChangedCallback = null, DownloadProgressChangedCallback progressChangedCallback = null)
        {
            return Task.Run(() => DownloadStringAsync(cancellationToken, downloadStateController, stateChangedCallback, progressChangedCallback)).GetAwaiter().GetResult();
        }

        public async Task<HttpContentResult<string>> DownloadStringAsync(CancellationToken cancellationToken = default, DownloadStateController downloadStateController = null, DownloadStateChangedCallback stateChangedCallback = null, DownloadProgressChangedCallback progressChangedCallback = null)
        {
            return await DownloadContentAsync(ReadResponseString, cancellationToken, downloadStateController, stateChangedCallback, progressChangedCallback);
        }

        internal HttpContentResult<byte[]> DownloadBytes(CancellationToken cancellationToken = default, DownloadStateController downloadStateController = null, DownloadStateChangedCallback stateChangedCallback = null, DownloadProgressChangedCallback progressChangedCallback = null)
        {
            return Task.Run(() => DownloadBytesAsync(cancellationToken, downloadStateController, stateChangedCallback, progressChangedCallback)).GetAwaiter().GetResult();
        }

        public async Task<HttpContentResult<byte[]>> DownloadBytesAsync(CancellationToken cancellationToken = default, DownloadStateController downloadStateController = null, DownloadStateChangedCallback stateChangedCallback = null, DownloadProgressChangedCallback progressChangedCallback = null)
        {
            return await DownloadContentAsync(ReadResponseBytes, cancellationToken, downloadStateController, stateChangedCallback, progressChangedCallback);
        }

        protected async Task<HttpContentResult<T>> DownloadContentAsync<T>(
            Func<HttpResponseMessage, CancellationToken, DownloadStateController, DownloadStateChangedCallback, DownloadProgressChangedCallback, Task<T>> readResponseFunc,
            CancellationToken cancellationToken = default,
            DownloadStateController downloadStateController = null,
            DownloadStateChangedCallback stateChangedCallback = null,
            DownloadProgressChangedCallback progressChangedCallback = null)
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

                StringContent content = null;
                try
                {
                    if (!string.IsNullOrEmpty(_content))
                    {
                        content = new StringContent(_content, _contentEncoding, _contentMediaType);
                    }

                    using (var request = CreateRequest(_url, content))
                    {
                        var httpClient = _httpClientFactory.GetClient(request);
                        using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token))
                        {
                            httpStatusCode = response.StatusCode;
                            if (response.IsSuccessStatusCode)
                            {
                                var result = await readResponseFunc(response, cancellationToken, downloadStateController, stateChangedCallback, progressChangedCallback);
                                OnDownloadStateChanged(stateChangedCallback, HttpRequestClientStatus.Completed);
                                return HttpContentResult<T>.Success(_url, result, response.StatusCode, response);
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
                    content?.Dispose();
                }

                return HttpContentResult<T>.Failure(_url, error, httpStatusCode);
            }
        }

        private async Task<string> ReadResponseString(
            HttpResponseMessage response,
            CancellationToken cancellationToken,
            DownloadStateController downloadStateController,
            DownloadStateChangedCallback stateChangedCallback,
            DownloadProgressChangedCallback progressChangedCallback)
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
                        HandleDownloadProgress(ref contentProgressLength, totalContentLength, ref lastReportTime, ref lastTotalBytesRead, buffer.Length, downloadStateController, cancellationToken, stateChangedCallback, progressChangedCallback);
                        stringBuilder.Append(buffer, 0, bytesRead);
                    }
                }
            }

#if DEBUG
            _logger.Info("String download completed successfully.");
#endif

            return stringBuilder.ToString();
        }


        private async Task<byte[]> ReadResponseBytes(
            HttpResponseMessage response,
            CancellationToken cancellationToken,
            DownloadStateController downloadStateController,
            DownloadStateChangedCallback stateChangedCallback,
            DownloadProgressChangedCallback progressChangedCallback)
        {
            var totalContentLength = response.Content.Headers.ContentLength ?? 0;
            var buffer = new byte[4096];
            using (var contentStream = await response.Content.ReadAsStreamAsync())
            {
                using (var memoryStream = new MemoryStream())
                {
                    int bytesRead;
                    long contentProgressLength = 0;
                    long lastTotalBytesRead = 0;
                    var startTime = DateTime.Now;
                    var lastReportTime = startTime;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        HandleDownloadProgress(ref contentProgressLength, totalContentLength, ref lastReportTime, ref lastTotalBytesRead, buffer.Length, downloadStateController, cancellationToken, stateChangedCallback, progressChangedCallback);
                        await memoryStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    }

#if DEBUG
                    _logger.Info("Byte download completed successfully.");
#endif
                    return memoryStream.ToArray();
                }
            }
        }

        private void HandleDownloadProgress(
            ref long contentProgressLength,
            long totalContentLength, ref DateTime lastReportTime,
            ref long lastTotalBytesRead,
            long bytesRead,
            DownloadStateController downloadStateController,
            CancellationToken cancellationToken,
            DownloadStateChangedCallback stateChangedCallback,
            DownloadProgressChangedCallback progressChangedCallback)
        {
            var shouldPause = downloadStateController?.IsPaused() == true;
            cancellationToken.ThrowIfCancellationRequested();
            if (shouldPause)
            {
                OnDownloadStateChanged(stateChangedCallback, HttpRequestClientStatus.Paused);
                downloadStateController?.PauseAsync().GetAwaiter().GetResult();
                cancellationToken.ThrowIfCancellationRequested();
                OnDownloadStateChanged(stateChangedCallback, HttpRequestClientStatus.Downloading);
            }

            contentProgressLength += bytesRead;
            if (progressChangedCallback != null)
            {
                var currentTime = DateTime.Now;
                var isCompleted = contentProgressLength == totalContentLength;
                if (isCompleted || currentTime - lastReportTime >= _progressReportInterval)
                {
                    ReportProgress(progressChangedCallback, contentProgressLength, totalContentLength, DateTime.Now, currentTime, lastReportTime, lastTotalBytesRead);
                    lastReportTime = currentTime;
                    lastTotalBytesRead = contentProgressLength;
                }
            }
        }


    }
}