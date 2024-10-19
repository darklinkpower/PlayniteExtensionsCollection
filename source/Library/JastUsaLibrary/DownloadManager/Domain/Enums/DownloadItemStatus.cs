using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Enums
{
    public enum DownloadItemStatus
    {
        Idle,
        Downloading,
        Paused,
        Completed,
        Failed,
        Canceled,
        Extracting,
        ExtractionCompleted,
        ExtractionFailed
    }
}