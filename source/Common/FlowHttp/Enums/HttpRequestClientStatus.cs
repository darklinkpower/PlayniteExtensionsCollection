using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHttp.Enums
{
    public enum HttpRequestClientStatus
    {
        Idle,
        Downloading,
        Paused,
        Completed,
        Failed,
        Canceled
    }
}