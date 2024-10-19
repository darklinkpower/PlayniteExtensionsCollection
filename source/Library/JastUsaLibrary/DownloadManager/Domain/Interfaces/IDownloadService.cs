using JastUsaLibrary.DownloadManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Domain.Interfaces
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
        Task<bool> AddAssetToDownloadAsync(JastAssetWrapper assetWrapper);
        bool GetExistsById(string Id);
        //void PersistDownloadData();
        int AvailableDownloadSlots { get; }
        IReadOnlyCollection<DownloadItem> DownloadsList { get; }
    }
}