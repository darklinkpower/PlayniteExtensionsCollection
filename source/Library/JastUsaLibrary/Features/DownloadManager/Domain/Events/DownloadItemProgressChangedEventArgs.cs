using FlowHttp.Events;
using JastUsaLibrary.DownloadManager.Domain.Entities;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Events
{
    public class DownloadItemProgressChangedEventArgs : EventArgs
    {
        public Guid EventId { get; }
        public DateTime CreatedAtUtc { get; }
        public DownloadItem DownloadItem { get; }
        internal DownloadProgressArgs DownloadProgressArgs { get; }

        internal DownloadItemProgressChangedEventArgs(DownloadItem downloadItem, DownloadProgressArgs downloadProgressArgs)
        {
            EventId = Guid.NewGuid();
            CreatedAtUtc = DateTime.UtcNow;
            DownloadItem = downloadItem;
            DownloadProgressArgs = downloadProgressArgs;
        }
    }
}