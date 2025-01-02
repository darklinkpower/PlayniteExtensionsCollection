using JastUsaLibrary.DownloadManager.Domain.Entities;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Events
{
    public class DownloadItemMovedEventArgs : EventArgs
    {
        public Guid EventId { get; }
        public DateTime CreatedAtUtc { get; }
        public DownloadItem DownloadItem { get; }
        public int OldIndex { get; }
        public int NewIndex { get; }

        public DownloadItemMovedEventArgs(DownloadItem downloadItem, int oldIndex, int newIndex)
        {
            EventId = Guid.NewGuid();
            CreatedAtUtc = DateTime.UtcNow;
            DownloadItem = downloadItem;
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }
    }
}
