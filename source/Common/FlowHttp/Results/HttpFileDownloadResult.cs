using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FlowHttp.Results
{
    internal class HttpFileDownloadResult : HttpResultBase
    {
        /// <summary>
        /// Gets the string value of the download request.
        /// </summary>
        internal string DownloadPath { get; }

        internal string DownloadDirectory => !string.IsNullOrEmpty(DownloadPath) ? Path.GetDirectoryName(DownloadPath) : null;
        /// <summary>
        /// Gets the long value that indicates the size of the downloaded file in bytes.
        /// </summary>
        internal long FileSize { get; }

        private HttpFileDownloadResult(Uri url, bool isSuccess, FileInfo fileInfo, Exception error, HttpStatusCode? httpStatusCode, HttpResponseMessage httpResponseMessage)
            : base(url, isSuccess, error, httpStatusCode, httpResponseMessage)
        {
            if (fileInfo != null)
            {
                DownloadPath = fileInfo.FullName;
                FileSize = fileInfo.Length;
            }
        }

        internal static HttpFileDownloadResult Success(Uri url, FileInfo fileInfo, HttpStatusCode? statusCode, HttpResponseMessage httpResponseMessage)
        {
            return new HttpFileDownloadResult(url, true, fileInfo, null, statusCode, httpResponseMessage);
        }

        internal static HttpFileDownloadResult Failure(Uri url, Exception error, HttpStatusCode? statusCode = null, HttpResponseMessage httpResponseMessage = default)
        {
            return new HttpFileDownloadResult(url, false, null, error, statusCode, httpResponseMessage);
        }
    }

}