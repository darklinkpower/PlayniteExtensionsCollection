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
        public DownloadItem DownloadItem { get; }
        internal DownloadProgressArgs DownloadProgressArgs { get; }

        internal DownloadItemProgressChangedEventArgs(DownloadItem downloadItem, DownloadProgressArgs downloadProgressArgs)
        {
            DownloadItem = downloadItem;
            DownloadProgressArgs = downloadProgressArgs;
        }
    }
}