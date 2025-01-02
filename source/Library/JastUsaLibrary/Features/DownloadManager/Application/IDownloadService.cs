using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.DownloadManager.Domain.Events;
using JastUsaLibrary.Features.DownloadManager.Domain.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.DownloadManager.Application
{
    public interface IDownloadService
    {
        Task StartDownloadsAsync(bool forceRestart = false, bool silent = false);
        //Task StopAllDownloadsAsync();
        Task CancelDownloadsAsync();
        Task RemoveCompletedDownloadsAsync();
        Task PauseDownloadsAsync();
        Task<bool> AddDownloadAsync(DownloadItem downloadItem);
        //Task RestoreDownloadsAsync();
        Task MoveDownloadItemOnePlaceBeforeAsync(DownloadItem downloadItem);
        Task MoveDownloadItemOnePlaceAfterAsync(DownloadItem downloadItem);
        Task<bool> AddAssetToDownloadAsync(JastAssetWrapper assetWrapper, CancellationToken cancellationToken = default);
        bool GetExistsById(string Id);
        //void PersistDownloadData();
        int AvailableDownloadSlots { get; }
        IReadOnlyCollection<DownloadItem> DownloadsList { get; }
        event EventHandler<GameInstallationAppliedEventArgs> GameInstallationApplied;
        event EventHandler<GlobalDownloadProgressChangedEventArgs> GlobalDownloadProgressChanged;
        event EventHandler<DownloadsListItemsAddedEventArgs> DownloadsListItemsAdded;
        event EventHandler<DownloadsListItemsRemovedEventArgs> DownloadsListItemsRemoved;
        event EventHandler<DownloadItemMovedEventArgs> DownloadItemMoved;
        event EventHandler<DownloadItemStatusChangedEventArgs> DownloadItemStatusChanged;
    }
}