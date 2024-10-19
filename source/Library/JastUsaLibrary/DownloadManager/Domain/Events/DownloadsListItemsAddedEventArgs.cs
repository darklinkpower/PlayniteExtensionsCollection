using JastUsaLibrary.DownloadManager.Domain.Entities;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Events
{
    public class DownloadsListItemsAddedEventArgs : EventArgs
    {
        public IReadOnlyCollection<DownloadItem> Items { get; }

        public DownloadsListItemsAddedEventArgs(IEnumerable<DownloadItem> items)
        {
            Items = items.ToList();
        }
    }
}