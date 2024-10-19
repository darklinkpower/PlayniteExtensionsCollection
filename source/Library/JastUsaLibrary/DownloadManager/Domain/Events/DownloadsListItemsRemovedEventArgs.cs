using JastUsaLibrary.DownloadManager.Domain.Entities;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Events
{
    public class DownloadsListItemsRemovedEventArgs : EventArgs
    {
        public IReadOnlyCollection<DownloadItem> Items { get; }

        public DownloadsListItemsRemovedEventArgs(IEnumerable<DownloadItem> items)
        {
            Items = items.ToList();
        }
    }
}
