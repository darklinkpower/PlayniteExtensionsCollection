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
using FlowHttp.Exceptions;
using FlowHttp.Events;
using FlowHttp.Results;
using PluginsCommon;

namespace FlowHttp.Requests
{
    internal class FlowHttpFileRequest : FlowHttpRequestBase<FlowHttpFileRequest>
    {
        private string _downloadPath;
        private bool _appendToFile = false;

        internal FlowHttpFileRequest(HttpClientFactory httpClientFactory) : base(httpClientFactory)
        {

        }

        internal FlowHttpFileRequest WithDownloadTo(string filePath)
        {
            _downloadPath = filePath;
            return this;
        }

        internal FlowHttpFileRequest WithAppendToFile(bool appendToFile)
        {
            _appendToFile = appendToFile;
            return this;
        }

        /// <summary>
        /// Synchronously downloads content from the provided URLs and saves it to the specified file path.
        /// </summary>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <param name="downloadStateController">The optional controller for managing download state.</param>
        /// <param name="stateChangedCallback">The optional callback for reporting changes in download state.</param>
        /// <param name="progressChangedCallback">The optional callback for reporting download progress.</param>
        /// <returns>The result of the download and file-saving operation represented by an <see cref="HttpFileDownloadResult"/>.</returns>
        internal HttpFileDownloadResult DownloadFile(CancellationToken cancellationToken = default, DownloadStateController downloadStateController = null, DownloadStateChangedCallback stateChangedCallback = null, DownloadProgressChangedCallback progressChangedCallback = null)
        {
            return Task.Run(() => DownloadFileAsync(cancellationToken, downloadStateController, stateChangedCallback, progressChangedCallback)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously downloads content from the provided URLs and saves it to the specified file path.
        /// </summary>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <param name="downloadStateController">The optional controller for managing download state.</param>
        /// <param name="stateChangedCallback">The optional callback for reporting changes in download state.</param>
        /// <param name="progressChangedCallback">The optional callback for reporting download progress.</param>
        /// <returns>An awaitable task representing the asynchronous download and file-saving operation. The task's result is the <see cref="HttpFileDownloadResult"/>.</returns>
        internal async Task<HttpFileDownloadResult> DownloadFileAsync(CancellationToken cancellationToken = default, DownloadStateController downloadStateController = null, DownloadStateChangedCallback stateChangedCallback = null, DownloadProgressChangedCallback progressChangedCallback = null)
        {
            if (_url is null)
            {
                var ex = new InvalidOperationException("No URL provided.");
                return HttpFileDownloadResult.Failure(_url, ex);
            }

            if (string.IsNullOrEmpty(_downloadPath))
            {
                var ex = new InvalidOperationException("No download path provided.");
                return HttpFileDownloadResult.Failure(_url, ex);
            }

            OnDownloadStateChanged(stateChangedCallback, HttpRequestClientStatus.Downloading);
            long resumeOffset = 0;
            var appendToFile = false;
            if (_appendToFile && FileSystem.FileExists(_downloadPath))
            {
                var fileInfo = new FileInfo(FileSystem.FixPathLength(_downloadPath));
                if (fileInfo.Length > 0)
                {
                    resumeOffset = fileInfo.Length;
                    appendToFile = true;
                }
            }

#if DEBUG
            _logger.Info($"Starting download of url \"{_url}\" to \"{_downloadPath}\"...");
#endif
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
                    int redirectCount = 0;
                    int maxRedirects = _maxRedirects;
                    var currentUrl = _url;
                    if (!string.IsNullOrEmpty(_content))
                    {
                        stringContent = new StringContent(_content, _contentEncoding, _contentMediaType);
                    }

                    while (redirectCount <= maxRedirects)
                    {
                        using (var request = CreateRequest(currentUrl, stringContent, resumeOffset))
                        {
                            var httpClient = _httpClientFactory.GetClient(request);
                            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                            {
                                httpStatusCode = response.StatusCode;
                                if (httpStatusCode == HttpStatusCode.Redirect ||
                                    httpStatusCode == HttpStatusCode.Moved ||
                                    httpStatusCode == HttpStatusCode.RedirectMethod ||
                                    httpStatusCode == HttpStatusCode.TemporaryRedirect ||
                                    (int)httpStatusCode == 308) // Permanent Redirect (308) is not in HttpStatusCode enum
                                {
                                    currentUrl = response.Headers.Location;
                                    redirectCount++;
                                    continue;
                                }

                                if (appendToFile && response.Content.Headers.ContentRange is null)
                                {
                                    throw new MissingContentRangeHeaderException();
                                }

                                await SaveFileContent(response, cts.Token, appendToFile, downloadStateController, stateChangedCallback, progressChangedCallback);
                                var fileInfo = new FileInfo(_downloadPath);
                                var result = HttpFileDownloadResult.Success(_url, fileInfo, httpStatusCode, response);
                                OnDownloadStateChanged(stateChangedCallback, HttpRequestClientStatus.Completed);
                                return result;
                            }
                        }
                    }

                    throw new HttpRequestException($"Too many redirects: {redirectCount}");
                }
                catch (MissingContentRangeHeaderException ex)
                {
                    error = ex;
                    _logger.Error(ex, "Download failed");
                    OnDownloadStateChanged(stateChangedCallback, HttpRequestClientStatus.Failed);
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

                return HttpFileDownloadResult.Failure(_url, error, httpStatusCode);
            }
        }

        /// <summary>
        /// Saves the content of the HttpResponseMessage to a file, while reporting progress if applicable.
        /// </summary>
        /// <param name="result">The HttpRequestFactoryResult to update with file-related information.</param>
        /// <param name="response">The HttpResponseMessage containing the content to save.</param>
        private async Task SaveFileContent(HttpResponseMessage response, CancellationToken cancellationToken, bool appendToFile, DownloadStateController downloadStateController, DownloadStateChangedCallback stateChangedCallback, DownloadProgressChangedCallback progressChangedCallback)
        {
            var startTime = DateTime.Now;
            var lastReportTime = startTime;
            var totalContentLength = response.Content.Headers.ContentLength ?? 0;
            long contentProgressLength = 0;
            if (response.Content.Headers.ContentRange?.HasLength == true)
            {
                totalContentLength = response.Content.Headers.ContentRange.Length.Value;
                contentProgressLength = response.Content.Headers.ContentRange.From.Value;
            }

            var fileMode = appendToFile ? FileMode.Append : FileMode.Create;
            var deleteExistingFile = fileMode == FileMode.Create;
            FileSystem.PrepareSaveFile(_downloadPath, deleteExistingFile);
            using (var fs = new FileStream(_downloadPath, fileMode))
            {
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    long lastTotalBytesRead = 0;
                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        var shouldPause = downloadStateController?.IsPaused() == true;
                        cancellationToken.ThrowIfCancellationRequested();

                        if (shouldPause)
                        {
                            OnDownloadStateChanged(stateChangedCallback, HttpRequestClientStatus.Paused);
                            cancellationToken.ThrowIfCancellationRequested();
                            await downloadStateController.PauseAsync();
                            OnDownloadStateChanged(stateChangedCallback, HttpRequestClientStatus.Downloading);
                        }

                        await fs.WriteAsync(buffer, 0, bytesRead);
                        if (progressChangedCallback != null && totalContentLength > 0)
                        {
                            contentProgressLength += bytesRead;
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
        }


    }
}