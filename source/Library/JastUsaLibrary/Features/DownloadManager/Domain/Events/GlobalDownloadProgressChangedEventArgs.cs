using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Events
{
    public class GlobalDownloadProgressChangedEventArgs : EventArgs
    {
        public Guid EventId { get; }
        public DateTime CreatedAtUtc { get; }
        public int TotalItems { get; }
        public double? AverageProgressPercentage { get; }
        public long? TotalBytesToDownload { get; }
        public long? TotalBytesDownloaded { get; }
        public double? TotalDownloadProgress { get; }

        public GlobalDownloadProgressChangedEventArgs(
            int totalItems,
            double? averageProgressPercentage,
            long? totalBytesToDownload,
            long? totalBytesDownloaded,
            double? totalDownloadProgress)
        {
            EventId = Guid.NewGuid();
            CreatedAtUtc = DateTime.UtcNow;
            TotalItems = totalItems;
            AverageProgressPercentage = averageProgressPercentage;
            TotalBytesToDownload = totalBytesToDownload;
            TotalBytesDownloaded = totalBytesDownloaded;
            TotalDownloadProgress = totalDownloadProgress;
        }
    }
}
