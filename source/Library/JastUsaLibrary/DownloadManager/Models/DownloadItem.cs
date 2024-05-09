using JastUsaLibrary.DownloadManager.Enums;
using JastUsaLibrary.DownloadManager.ViewModels;
using JastUsaLibrary.Services;
using JastUsaLibrary.ViewModels;
using Playnite.SDK;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlowHttp;
using FlowHttp.Enums;
using FlowHttp.Events;

namespace JastUsaLibrary.DownloadManager.Models
{
    public class DownloadStatusChangedEventArgs : EventArgs
    {
        public DownloadItemStatus NewStatus { get; }

        public DownloadStatusChangedEventArgs(DownloadItemStatus newStatus)
        {
            NewStatus = newStatus;
        }
    }

    public class DownloadItem : INotifyPropertyChanged, IDisposable
    {
        public event EventHandler<DownloadStatusChangedEventArgs> DownloadStatusChanged;

        private void OnDownloadStatusChanged(DownloadItemStatus newStatus)
        {
            DownloadStatusChanged?.Invoke(this, new DownloadStatusChangedEventArgs(newStatus));
        }

        public event EventHandler<DownloadProgressArgs> DownloadProgressChanged;

        private void OnDownloadProgressChanged(DownloadProgressArgs downloadProgressArgs)
        {
            DownloadProgressChanged?.Invoke(this, downloadProgressArgs);
        }

        private readonly JastUsaAccountClient _jastAccountClient;
        private readonly DownloadsManagerViewModel _downloadsManagerViewModel;
        private DownloadData _downloadData;
        private DownloadStateController _downloadStateController;
        private bool _isDownloadProcessRunning = false;
        private readonly SemaphoreSlim _stateControllerSemaphore = new SemaphoreSlim(1);
        private bool _isDisposed = false;
        private readonly object _disposeLock = new object();

        public bool CanStartDownload => !_isDownloadProcessRunning &&
            (_downloadData.Status == DownloadItemStatus.Canceled ||
            _downloadData.Status == DownloadItemStatus.Failed ||
            _downloadData.Status == DownloadItemStatus.Idle);
        public bool CanPauseDownload => _stateControllerSemaphore != null && _downloadData.Status == DownloadItemStatus.Downloading;
        public bool CanResumeDownload => _stateControllerSemaphore != null && _downloadData.Status == DownloadItemStatus.Paused;
        public bool CanCancelDownload => _stateControllerSemaphore != null &&
            (_downloadData.Status == DownloadItemStatus.Downloading || _downloadData.Status == DownloadItemStatus.Paused);

        public DownloadData DownloadData
        {
            get => _downloadData;
            set
            {
                if (_downloadData != value)
                {
                    _downloadData = value;
                    OnPropertyChanged();
                }
            }
        }

        public DownloadItem(JastUsaAccountClient jastAccountClient, DownloadData downloadData, DownloadsManagerViewModel downloadsManagerViewModel)
        {
            _jastAccountClient = jastAccountClient;
            _downloadsManagerViewModel = downloadsManagerViewModel;
            DownloadData = downloadData;
        }

        public async Task StartDownloadAsync()
        {
            if (_isDownloadProcessRunning || _downloadData.IsComplete)
            {
                return;
            }
            
            void stateChangedCallback(DownloadStateArgs args)
            {
                var httpRequestStatus = args.Status;
                var newStatus = DownloadItemStatus.Idle;
                var clearDownloadingValues = true;
                switch (httpRequestStatus)
                {
                    case HttpRequestClientStatus.Idle:
                        newStatus = DownloadItemStatus.Idle;
                        clearDownloadingValues = true;
                        break;
                    case HttpRequestClientStatus.Downloading:
                        newStatus = DownloadItemStatus.Downloading;
                        break;
                    case HttpRequestClientStatus.Paused:
                        clearDownloadingValues = true;
                        newStatus = DownloadItemStatus.Paused;
                        break;
                    case HttpRequestClientStatus.Completed:
                        newStatus = DownloadItemStatus.Completed;
                        break;
                    case HttpRequestClientStatus.Failed:
                        clearDownloadingValues = true;
                        newStatus = DownloadItemStatus.Failed;
                        break;
                    case HttpRequestClientStatus.Canceled:
                        clearDownloadingValues = true;
                        newStatus = DownloadItemStatus.Canceled;
                        break;
                    default:
                        break;
                }

                if (clearDownloadingValues)
                {
                    _downloadData.FormattedDownloadSpeedPerSecond = string.Empty;
                    _downloadData.TimeRemaining = TimeSpan.MinValue;
                }

                if (newStatus == DownloadItemStatus.Completed)
                {
                    FileSystem.MoveFile(_downloadData.TemporaryDownloadPath, _downloadData.DownloadPath);
                }

                DownloadData.Status = newStatus;
                OnDownloadStatusChanged(newStatus);
                NotifyCommandsPropertyChanged();
            }

            void progressChangedCallback(DownloadProgressArgs args)
            {
                DownloadData.UpdateProperties(args);
                OnDownloadProgressChanged(args);
            }

            _isDownloadProcessRunning = true;
            _downloadStateController?.Reset();
            _downloadStateController?.Dispose();
            _downloadStateController = null;

            var hasDownloadExpired = IsTimeStampExpired(DownloadData.UrlExpiresTimeStamp);
            if (hasDownloadExpired && !_downloadsManagerViewModel.RefreshDownloadItemUri(this, true))
            {
                return;
            }

            var request = HttpRequestFactory.GetHttpFileRequest()
                .WithAppendToFile(true)
                .WithDownloadTo(_downloadData.TemporaryDownloadPath)
                .WithUrl(_downloadData.Url);
            _downloadStateController = new DownloadStateController();

            var downloadResult = await request.DownloadFileAsync(downloadStateController: _downloadStateController, stateChangedCallback: stateChangedCallback, progressChangedCallback: progressChangedCallback);
            _downloadStateController?.Reset();
            _downloadStateController?.Dispose();
            _downloadStateController = null;

            _isDownloadProcessRunning = false;
            NotifyCommandsPropertyChanged();
        }

        private static bool IsTimeStampExpired(long unixTimeStamp)
        {
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);
            var isExpired = expirationTime.DateTime < DateTime.Now.AddMinutes(-1);
            return isExpired;
        }

        public async Task PauseDownloadAsync()
        {
            await _stateControllerSemaphore.WaitAsync();
            try
            {
                _downloadStateController?.Pause();
            }
            finally
            {
                _stateControllerSemaphore.Release();
            }
        }

        public async Task ResumeDownloadAsync()
        {
            await _stateControllerSemaphore.WaitAsync();
            try
            {
                _downloadStateController?.Resume();
            }
            finally
            {
                _stateControllerSemaphore.Release();
            }
        }

        public async Task CancelDownloadAsync()
        {
            await _stateControllerSemaphore.WaitAsync();
            try
            {
                _downloadStateController?.Cancel();
            }
            finally
            {
                _stateControllerSemaphore.Release();
            }
        }

        public void OpenDownloadDirectory()
        {
            var downloadDirectory = _downloadData.DownloadDirectory;
            if (FileSystem.DirectoryExists(downloadDirectory))
            {
                ProcessStarter.StartProcess(downloadDirectory);
            }
        }

        private async Task RemoveFromDownloadsListAsync()
        {
            await _downloadsManagerViewModel.RemoveFromDownloadsListAsync(this, true);
        }

        private void NotifyCommandsPropertyChanged()
        {
            OnPropertyChanged(nameof(StartDownloadAsyncCommand));
            OnPropertyChanged(nameof(PauseDownloadAsyncCommand));
            OnPropertyChanged(nameof(ResumeDownloadAsyncCommand));
            OnPropertyChanged(nameof(CancelDownloadAsyncCommand));
            OnPropertyChanged(nameof(CanStartDownload));
            OnPropertyChanged(nameof(CanResumeDownload));
            OnPropertyChanged(nameof(CanPauseDownload));
            OnPropertyChanged(nameof(CanCancelDownload));
        }

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_isDisposed)
                {
                    return;
                }

                _downloadStateController?.Dispose();
                _stateControllerSemaphore?.Dispose();
                _isDisposed = true;
            }
        }

        public RelayCommand StartDownloadAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await StartDownloadAsync();
            }, () => CanStartDownload);
        }

        public RelayCommand ResumeDownloadAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await ResumeDownloadAsync();
            }, () => CanResumeDownload);
        }

        public RelayCommand PauseDownloadAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await PauseDownloadAsync();
            }, () => CanPauseDownload);
        }

        public RelayCommand CancelDownloadAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await CancelDownloadAsync();
            }, () => CanCancelDownload);
        }

        public RelayCommand RemoveFromDownloadsListAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await RemoveFromDownloadsListAsync();
            });
        }

        public RelayCommand OpenDownloadDirectoryCommand
        {
            get => new RelayCommand(() =>
            {
                OpenDownloadDirectory();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}