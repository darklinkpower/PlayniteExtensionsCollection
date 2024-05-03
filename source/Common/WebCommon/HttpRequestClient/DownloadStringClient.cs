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
            CancellationToken cancellationToken,
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
            cancellationToken,
            headers,
            cookies,
            timeout,
            progressReportInterval
        )
        {

        }

        public HttpContentResult<string> DownloadString()
        {
            return Task.Run(() => DownloadStringAsync()).GetAwaiter().GetResult();
        }

        private async Task<HttpContentResult<string>> DownloadStringAsync()
        {
#if DEBUG
            _logger.Info($"Starting download of url \"{_url}\"...");
#endif

            Status = HttpRequestClientStatus.Downloading;
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
                    using (var request = CreateRequest(_url, stringContent))
                    {
                        var httpClient = _httpClientFactory.GetClient(request);
                        using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token))
                        {
                            httpStatusCode = response.StatusCode;
                            if (response.IsSuccessStatusCode)
                            {
                                var content = await ReadResponseContent(response, cts.Token);
                                return HttpContentResult<string>.Success(_url, content, httpStatusCode, response);
                            }
                        }
                    }
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
                }

                return HttpContentResult<string>.Failure(_url, error, httpStatusCode);
            }
        }

        /// <summary>
        /// Reads and retrieves the content from the response message using the retrieved encoding.
        /// </summary>
        /// <param name="response">The HttpResponseMessage containing the response content to read.</param>
        /// <returns>The content as a string.</returns>
        protected async Task<string> ReadResponseContent(HttpResponseMessage response, CancellationToken cancellationToken)
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
                        cancellationToken.ThrowIfCancellationRequested();
                        stringBuilder.Append(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;

                        var currentTime = DateTime.Now;
                        if (currentTime - lastReportTime >= _progressReportInterval)
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


    }
}