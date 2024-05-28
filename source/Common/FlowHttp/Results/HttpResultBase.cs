using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FlowHttp.Results
{
    internal abstract class HttpResultBase
    {
        internal bool IsSuccess { get; }
        internal bool IsFailure => !IsSuccess;
        internal bool IsCancelled => Error is OperationCanceledException;
        internal Uri Url { get; }
        internal Exception Error { get; }
        internal HttpStatusCode? HttpStatusCode { get; }
        internal Dictionary<string, string> ResponseHeaders { get; }
        internal string ResponseReaderPhrase { get; }
        internal Dictionary<string, string> ContentHeaders { get; }
        internal List<Cookie> ResponseCookies { get; }

        protected HttpResultBase(Uri url, bool isSuccess, Exception error, HttpStatusCode? httpStatusCode, HttpResponseMessage httpResponseMessage)
        {
            Url = url;
            IsSuccess = isSuccess;
            Error = error;
            if (httpStatusCode != null && httpStatusCode.HasValue)
            {
                HttpStatusCode = httpStatusCode;
            }

            if (httpResponseMessage != default)
            {
                ResponseHeaders = httpResponseMessage.Headers.ToDictionary(h => h.Key, h => h.Value.FirstOrDefault());
                ResponseReaderPhrase = httpResponseMessage.ReasonPhrase;
                ContentHeaders = httpResponseMessage.Content?.Headers?.ToDictionary(h => h.Key, h => h.Value.FirstOrDefault());
                var ssss = httpResponseMessage.ReasonPhrase;
                var responseCookies = new List<Cookie>();
                if (httpResponseMessage.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
                {
                    foreach (var cookieString in setCookieHeaders)
                    {
                        if (CookiesUtilities.TryParseCookieFromString(cookieString, out var cookie))
                        {
                            responseCookies.Add(cookie);
                        }
                    }
                }

                ResponseCookies = responseCookies;
            }
        }
    }

}