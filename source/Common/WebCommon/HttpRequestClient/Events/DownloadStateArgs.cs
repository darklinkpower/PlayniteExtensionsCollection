using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCommon.Enums;

namespace WebCommon.HttpRequestClient.Events
{
    public class DownloadStateArgs : EventArgs
    {
        public HttpRequestClientStatus Status { get; }

        public DownloadStateArgs(HttpRequestClientStatus status)
        {
            Status = status;
        }
    }
}