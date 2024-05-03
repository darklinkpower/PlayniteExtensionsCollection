using PluginsCommon;
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
using WebCommon.Exceptions;
using WebCommon.Results;

namespace WebCommon.HttpRequestClient
{
    public class DownloadFileClient : HttpRequestClientBase
    {
        private readonly string _filePath;
        private readonly bool _appendToFile;

        internal DownloadFileClient(
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
            TimeSpan progressReportInterval,
            string filePath,
            bool appendToFile
        ) : base(
            httpClientFactory,
            url,
            content,
            contentEncoding,
            contentMediaType,
            httpMethod,
            cancellationToken,
            headers,
            cookies,
            timeout,
            progressReportInterval
        )
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException("No URL provided.");
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new InvalidOperationException("File path must be provided for downloading a file.");
            }

            _filePath = filePath;
            _appendToFile = appendToFile;
        }

        /// <summary>
        /// Synchronously downloads content from the provided URLs and saves it to the specified file path. Returns an HttpDownloaderResult.
        /// </summary>
        /// <returns>The HttpDownloaderResult representing the result of the download and file-saving operation.</returns>
        public HttpFileDownloadResult DownloadFile()
        {
            return Task.Run(() => DownloadFileAsync()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously downloads content from the provided URLs and saves it to the specified file path. Returns an HttpDownloaderResult.
        /// </summary>
        /// <returns>An awaitable Task that represents the asynchronous download and file-saving operation. The task's result is the HttpDownloaderResult.</returns>
        public async Task<HttpFileDownloadResult> DownloadFileAsync()
        {
            Status = HttpRequestClientStatus.Downloading;
            long resumeOffset = 0;
            var appendToFile = false;
            if (_appendToFile && FileSystem.FileExists(_filePath))
            {
                var fileInfo = new FileInfo(FileSystem.FixPathLength(_filePath));
                if (fileInfo.Length > 0)
                {
                    resumeOffset = fileInfo.Length;
                    appendToFile = true;
                }
            }

#if DEBUG
            _logger.Info($"Starting download of url \"{_url}\" to \"{_filePath}\"...");
#endif
            Exception error = null;
            HttpStatusCode? httpStatusCode = null;
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
                    using (var request = CreateRequest(_url, stringContent, resumeOffset))
                    {
                        var httpClient = _httpClientFactory.GetClient(request);
                        using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token))
                        {
                            httpStatusCode = response.StatusCode;
                            if (response.IsSuccessStatusCode)
                            {
                                if (appendToFile && response.Content.Headers.ContentRange is null)
                                {
                                    throw new MissingContentRangeHeaderException();
                                }

                                await SaveFileContent(response, cts.Token, appendToFile);
                                var fileInfo = new FileInfo(_filePath);
                                return HttpFileDownloadResult.Success(_url, fileInfo, httpStatusCode, response);
                            }
                        }
                    }
                }
                catch (MissingContentRangeHeaderException ex)
                {
                    HandleException(ex);
                    error = ex;
                }
                catch (OperationCanceledException ex)
                {
                    HandleCancellation(ex);
                    error = ex;
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                    error = ex;
                }
                finally
                {
                    stringContent?.Dispose();
                    _pauseEvent?.Dispose();
                    _pauseEvent = null;
                }

                return HttpFileDownloadResult.Failure(_url, error, httpStatusCode);
            }
        }

        /// <summary>
        /// Saves the content of the HttpResponseMessage to a file, while reporting progress if applicable.
        /// </summary>
        /// <param name="result">The HttpDownloaderResult to update with file-related information.</param>
        /// <param name="response">The HttpResponseMessage containing the content to save.</param>
        private async Task SaveFileContent(HttpResponseMessage response, CancellationToken cancellationToken, bool appendToFile)
        {
            var startTime = DateTime.Now;
            var lastReportTime = startTime;
            var requestContentLength = response.Content.Headers.ContentLength ?? 0;

            var fileMode = appendToFile ? FileMode.Append : FileMode.Create;
            var deleteExistingFile = fileMode == FileMode.Create;
            FileSystem.PrepareSaveFile(_filePath, deleteExistingFile);
            using (_pauseEvent = new ManualResetEventSlim(!_isPaused))
            {
                using (var fs = new FileStream(_filePath, fileMode))
                {
                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long lastTotalBytesRead = 0;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            _pauseEvent.Wait(cancellationToken);

                            await fs.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;

                            var currentTime = DateTime.Now;
                            if (currentTime - lastReportTime >= _progressReportInterval)
                            {
                                ReportProgress(totalBytesRead, requestContentLength, startTime, currentTime, lastReportTime, lastTotalBytesRead);
                                lastReportTime = currentTime;
                                lastTotalBytesRead = totalBytesRead;
                            }
                        }
                    }
                }
            }

#if DEBUG
            _logger.Info("Download completed successfully.");
#endif
        }
    }
}