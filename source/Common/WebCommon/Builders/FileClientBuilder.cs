using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCommon.HttpRequestClient;

namespace WebCommon.Builders
{
    public class FileClientBuilder : HttpRequestClientBuilderBase<FileClientBuilder, DownloadFileClient>
    {
        private string _filePath;
        private bool _appendToFile = false;

        internal FileClientBuilder(HttpClientFactory httpClientFactory) : base(httpClientFactory)
        {

        }

        public override DownloadFileClient Build()
        {
            return new DownloadFileClient(
                _httpClientFactory,
                _url,
                _content,
                _contentEncoding,
                _contentMediaType,
                _httpMethod,
                _cancellationToken,
                _headers,
                _cookies,
                _timeout,
                _progressReportInterval,
                 _filePath,
                _appendToFile
            );
        }

        public FileClientBuilder WithDownloadTo(string filePath)
        {
            _filePath = filePath;
            return this;
        }

        public FileClientBuilder WithAppendToFile(bool appendToFile)
        {
            _appendToFile = appendToFile;
            return this;
        }
    }
}