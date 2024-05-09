using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHttp.ValueObjects
{
    public sealed class HttpContentType
    {
        public string Value { get; }
        public HttpContentType(string contentTypeString)
        {
            Value = contentTypeString;
        }
    }
}