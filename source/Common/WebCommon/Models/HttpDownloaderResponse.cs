using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebCommon.Models
{
    public class BaseResponse
    {
        public Dictionary<string, IEnumerable<string>> Headers { get; internal set; }
        public Dictionary<string, IEnumerable<string>> ContentHeaders { get; internal set; }
        public IEnumerable<Cookie> Cookies { get; internal set; }
    }

    public class StringResponse : BaseResponse
    {
        public string Content { get; internal set; }
    }

    public class ByteArrayResponse : BaseResponse
    {
        public byte[] Content { get; internal set; }
    }
}