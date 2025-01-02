using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.DownloadManager.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.DownloadManager.Domain.Events
{
    public class DownloadItemStatusChangedEventArgs : EventArgs
    {
        public Guid EventId { get; }
        public DateTime CreatedAtUtc { get; }
        public DownloadItem DownloadItem { get; }
        public DownloadItemStatus NewStatus { get; }

        public DownloadItemStatusChangedEventArgs(DownloadItem downloadItem, DownloadItemStatus newStatus)
        {
            EventId = Guid.NewGuid();
            CreatedAtUtc = DateTime.UtcNow;
            DownloadItem = downloadItem;
            NewStatus = newStatus;
        }
    }
}