using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebCommon.Results
{
    public class HttpContentResult<T> : HttpResultBase
    {
        public T Content { get; }

        private HttpContentResult(string url, bool isSuccess, T content, Exception error, HttpStatusCode? httpStatusCode, HttpResponseMessage httpResponseMessage)
            : base(url, isSuccess, error, httpStatusCode, httpResponseMessage)
        {
            Content = content;
        }

        internal static HttpContentResult<T> Success(string url, T content, HttpStatusCode? statusCode, HttpResponseMessage httpResponseMessage)
        {
            return new HttpContentResult<T>(url, true, content, null, statusCode, httpResponseMessage);
        }

        internal static HttpContentResult<T> Failure(string url, Exception error, HttpStatusCode? statusCode = null, HttpResponseMessage httpResponseMessage = default)
        {
            return new HttpContentResult<T>(url, false, default, error, statusCode, httpResponseMessage);
        }
    }

}