﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHttp.Exceptions
{
    internal class MissingContentRangeHeaderException : Exception
    {
        internal MissingContentRangeHeaderException() : base("File download was set to append, but the server response did not include a Content-Range header indicating the range of the response.") { }
    }
}