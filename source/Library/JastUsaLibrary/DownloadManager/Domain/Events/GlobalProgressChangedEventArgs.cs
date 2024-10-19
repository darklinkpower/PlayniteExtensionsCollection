using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Events
{
    public class GlobalProgressChangedEventArgs : EventArgs
    {
        public int TotalItems { get; }
        public double? AverageProgressPercentage { get; }
        public long? TotalBytesToDownload { get; }
        public long? TotalBytesDownloaded { get; }
        public double? TotalDownloadProgress { get; }

        public GlobalProgressChangedEventArgs(int totalItems, double? averageProgressPercentage, long? totalBytesToDownload, long? totalBytesDownloaded, double? totalDownloadProgress)
        {
            TotalItems = totalItems;
            AverageProgressPercentage = averageProgressPercentage;
            TotalBytesToDownload = totalBytesToDownload;
            TotalBytesDownloaded = totalBytesDownloaded;
            TotalDownloadProgress = totalDownloadProgress;
        }
    }
}
