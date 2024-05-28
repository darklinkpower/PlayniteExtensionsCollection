using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHttp.ValueObjects
{
    internal sealed class HttpContentType
    {
        internal string Value { get; }
        internal HttpContentType(string contentTypeString)
        {
            Value = contentTypeString;
        }
    }
}