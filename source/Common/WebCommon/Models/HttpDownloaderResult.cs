using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebCommon.Models
{
    public class BaseHttpDownloaderResult
    {
        public bool IsSuccessful { get; internal set; }
        public HttpStatusCode StatusCode { get; internal set; }
        public Exception Exception { get; internal set; }
    }

    public class StringHttpDownloaderResult : BaseHttpDownloaderResult
    {
        public StringResponse Response { get; internal set; } = new StringResponse();
    }

    public class ByteArrayHttpDownloaderResult : BaseHttpDownloaderResult
    {
        public ByteArrayResponse Response { get; internal set; } = new ByteArrayResponse();
    }

    public class FileDownloadHttpDownloaderResult : BaseHttpDownloaderResult
    {
        public BaseResponse Response { get; internal set; } = new BaseResponse();
        public string FilePath { get; internal set; }
        public long FileSize { get; internal set; }
    }
}