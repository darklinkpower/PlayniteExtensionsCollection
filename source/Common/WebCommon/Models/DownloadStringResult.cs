using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebCommon
{
    /// <summary>
    /// Represents result of download string request.
    /// </summary>
    public class DownloadStringResult
    {
        /// <summary>
        /// Gets the string value of the download request.
        /// </summary>
        public string Result { get; }

        /// <summary>
        /// Gets boolean value indicating if the request was successful.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the HttpStatusCode value of the request.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; }

        /// <summary>
        /// Gets the HttpRequestException exception of the request if it was unsuccessful.
        /// </summary>
        public HttpRequestException HttpRequestException { get; }

        /// <summary>
        /// Creates new instance of <see cref="DownloadStringResult"/>.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="success"></param>
        /// <param name="httpStatusCode"></param>
        /// <param name="httpRequestException"></param>
        public DownloadStringResult(string result, bool success, HttpStatusCode httpStatusCode, HttpRequestException httpRequestException)
        {
            Result = result;
            Success = success;
            HttpStatusCode = httpStatusCode;
            HttpRequestException = httpRequestException;
        }
    }

    public class DownloadFileResult
    {
        /// <summary>
        /// Gets the string value of the download request.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets boolean value indicating if the request was successful.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the long value that indicates the size of the downloaded file in bytes. -1 indicates that no file was downloaded.
        /// </summary>
        public long FileSize { get; }

        /// <summary>
        /// Gets the HttpStatusCode value of the request.
        /// </summary>
        public HttpStatusCode? HttpStatusCode { get; } = null;

        /// <summary>
        /// Gets the exception of the request if it was unsuccessful.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Creates new instance of <see cref="DownloadStringResult"/>.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="success"></param>
        /// <param name="httpStatusCode"></param>
        /// <param name="httpRequestException"></param>
        public DownloadFileResult(string path, bool success, long fileSize, HttpStatusCode httpStatusCode, Exception exception)
        {
            Path = path;
            Success = success;
            HttpStatusCode = httpStatusCode;
            Exception = exception;
            FileSize = fileSize;
        }
    }
}