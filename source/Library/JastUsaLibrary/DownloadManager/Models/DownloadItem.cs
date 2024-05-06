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
using WebCommon;
using WebCommon.Enums;
using WebCommon.HttpRequestClient;
using WebCommon.HttpRequestClient.Events;

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

        private readonly DownloadFileClient _client;
        private readonly JastUsaAccountClient _jastAccountClient;
        private readonly DownloadsManagerViewModel _downloadsManagerViewModel;
        private DownloadData _downloadData;
        private DownloadStateController _downloadStateController;
        private bool _isDownloadProcessRunning = false;
        private readonly SemaphoreSlim _stateControllerSemaphore = new SemaphoreSlim(1);
        private bool _isDisposed = false;
        private readonly object _disposeLock = new object();

        public bool CanStartDownloads => !_isDownloadProcessRunning &&
            (_downloadData.Status == DownloadItemStatus.Canceled ||
            _downloadData.Status == DownloadItemStatus.Failed ||
            _downloadData.Status == DownloadItemStatus.Idle);
        public bool CanPause => _stateControllerSemaphore != null && _downloadData.Status == DownloadItemStatus.Downloading;
        public bool CanResume => _stateControllerSemaphore != null && _downloadData.Status == DownloadItemStatus.Paused;


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
            var clientBuilder = HttpBuilderFactory.GetFileClientBuilder().WithAppendToFile(true);
            _client = clientBuilder.Build();
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
                switch (httpRequestStatus)
                {
                    case HttpRequestClientStatus.Idle:
                        newStatus = DownloadItemStatus.Idle;
                        break;
                    case HttpRequestClientStatus.Downloading:
                        newStatus = DownloadItemStatus.Downloading;
                        break;
                    case HttpRequestClientStatus.Paused:
                        newStatus = DownloadItemStatus.Paused;
                        break;
                    case HttpRequestClientStatus.Completed:
                        newStatus = DownloadItemStatus.Completed;
                        break;
                    case HttpRequestClientStatus.Failed:
                        newStatus = DownloadItemStatus.Failed;
                        break;
                    case HttpRequestClientStatus.Canceled:
                        newStatus = DownloadItemStatus.Canceled;
                        break;
                    default:
                        break;
                }

                if (newStatus == DownloadItemStatus.Completed)
                {
                    FileSystem.MoveFile(_downloadData.TemporaryDownloadPath, _downloadData.DownloadPath);
                }

                DownloadData.Status = newStatus;
                NotifyCommandsPropertyChanged();
                OnDownloadStatusChanged(newStatus);
            }

            void progressChangedCallback(DownloadProgressArgs args)
            {
                DownloadData.UpdateProperties(args);
            }

            _isDownloadProcessRunning = true;
            _downloadStateController?.Reset();
            _downloadStateController?.Dispose();
            _downloadStateController = null;

            var hasDownloadExpired = IsTimeStampExpired(DownloadData.UrlExpiresTimeStamp);
            if (hasDownloadExpired && !_downloadsManagerViewModel.RefreshDownloadItemUri(this))
            {
                return;
            }

            _client.SetDownloadPath(_downloadData.TemporaryDownloadPath);
            _client.SetUrl(_downloadData.Url);
            _downloadStateController = new DownloadStateController();

            var downloadResult = await _client.DownloadFileAsync(downloadStateController: _downloadStateController, stateChangedCallback: stateChangedCallback, progressChangedCallback: progressChangedCallback);
            _downloadStateController?.Reset();
            _downloadStateController?.Dispose();
            _downloadStateController = null;

            _isDownloadProcessRunning = false;
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

        private async Task RemoveFromDownloadsListAsync()
        {
            await _downloadsManagerViewModel.RemoveFromDownloadsListAsync(this);
        }

        private void NotifyCommandsPropertyChanged()
        {
            OnPropertyChanged(nameof(StartDownloadAsyncCommand));
            OnPropertyChanged(nameof(PauseDownloadAsyncCommand));
            OnPropertyChanged(nameof(ResumeDownloadAsyncCommand));
            OnPropertyChanged(nameof(CancelDownloadAsyncCommand));
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
            }, () => CanStartDownloads);
        }

        public RelayCommand ResumeDownloadAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await ResumeDownloadAsync();
            }, () => CanResume);
        }

        public RelayCommand PauseDownloadAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await PauseDownloadAsync();
            }, () => CanPause);
        }

        public RelayCommand CancelDownloadAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await CancelDownloadAsync();
            }, () => CanPause);
        }

        public RelayCommand RemoveFromDownloadsListAsyncCommand
        {
            get => new RelayCommand(async () =>
            {
                await RemoveFromDownloadsListAsync();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}