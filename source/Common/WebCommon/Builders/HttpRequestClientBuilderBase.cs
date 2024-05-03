using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebCommon.Constants;
using WebCommon.HttpRequestClient;
using WebCommon.ValueObjects;

namespace WebCommon.Builders
{
    public abstract class HttpRequestClientBuilderBase<TBuilder, TClient>
        where TBuilder : HttpRequestClientBuilderBase<TBuilder, TClient>
        where TClient : HttpRequestClientBase
    {
        protected readonly HttpClientFactory _httpClientFactory;
        protected string _url;
        protected string _content;
        protected Encoding _contentEncoding = Encoding.UTF8;
        protected string _contentMediaType = HttpContentTypes.PlainText.Value;
        protected HttpMethod _httpMethod = HttpMethod.Get;
        protected CancellationToken _cancellationToken = CancellationToken.None;
        protected Dictionary<string, string> _headers;
        protected readonly List<Cookie> _cookies = new List<Cookie>();
        protected TimeSpan? _timeout;
        protected TimeSpan _progressReportInterval = TimeSpan.FromMilliseconds(1000);

        public abstract TClient Build();

        /// <summary>
        /// Initializes a new instance of the HttpRequestBuilder class with the specified HttpClientFactory.
        /// </summary>
        internal HttpRequestClientBuilderBase(HttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public TBuilder WithUrl(string url)
        {
            _url = url;
            return (TBuilder)this;
        }

        public TBuilder WithContent(string content, HttpContentType httpContentType = null, Encoding encoding = null)
        {
            _content = content;
            if (encoding != null)
            {
                _contentEncoding = encoding;
            }

            if (httpContentType != null)
            {
                _contentMediaType = httpContentType.Value;
            }

            return (TBuilder)this;
        }

        public TBuilder WithCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return (TBuilder)this;
        }

        public TBuilder WithHeaders(Dictionary<string, string> headers)
        {
            _headers = headers;
            return (TBuilder)this;
        }

        public TBuilder WithCookies(List<Cookie> cookies)
        {
            _cookies.AddRange(cookies);
            return (TBuilder)this;
        }

        public TBuilder WithCookies(Dictionary<string, string> cookiesDictionary)
        {
            _cookies.AddRange(cookiesDictionary.Select(kvp => new Cookie(kvp.Key, kvp.Value)));
            return (TBuilder)this;
        }

        public TBuilder WithHttpMethod(HttpMethod method)
        {
            _httpMethod = method;
            return (TBuilder)this;
        }

        public TBuilder WithGetHttpMethod()
        {
            _httpMethod = HttpMethod.Get;
            return (TBuilder)this;
        }

        public TBuilder WithPostHttpMethod()
        {
            _httpMethod = HttpMethod.Post;
            return (TBuilder)this;
        }

        public TBuilder WithHeadHttpMethod()
        {
            _httpMethod = HttpMethod.Head;
            return (TBuilder)this;
        }

        public TBuilder WithProgressReportInterval(TimeSpan reportInterval)
        {
            _progressReportInterval = reportInterval;
            return (TBuilder)this;
        }

        public TBuilder WithTimeout(double milliseconds)
        {
            _timeout = TimeSpan.FromMilliseconds(milliseconds);
            return (TBuilder)this;
        }

        public TBuilder WithTimeout(TimeSpan timeout)
        {
            _timeout = timeout;
            return (TBuilder)this;
        }
    }
}