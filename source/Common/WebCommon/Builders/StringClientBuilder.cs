using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCommon.HttpRequestClient;

namespace WebCommon.Builders
{
    public class StringClientBuilder : HttpRequestClientBuilderBase<StringClientBuilder, DownloadStringClient>
    {
        internal StringClientBuilder(HttpClientFactory httpClientFactory) : base(httpClientFactory)
        {

        }

        public override DownloadStringClient Build()
        {
            return new DownloadStringClient(
                _httpClientFactory,
                _url,
                _content,
                _contentEncoding,
                _contentMediaType,
                _httpMethod,
                _headers,
                _cookies,
                _timeout,
                _progressReportInterval
            );
        }
    }
}