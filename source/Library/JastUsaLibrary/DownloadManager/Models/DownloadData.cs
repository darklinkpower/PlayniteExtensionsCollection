using JastUsaLibrary.DownloadManager.Enums;
using JastUsaLibrary.Models;
using JastUsaLibrary.ViewModels;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebCommon.Enums;
using WebCommon.HttpRequestClient.Events;

namespace JastUsaLibrary.DownloadManager.Models
{
    public class DownloadData : INotifyPropertyChanged
    {
        #region Game Information

        public Guid GameId { get; set; }
        public GameLink GameLink { get; set; }
        public JastAssetType AssetType { get; set; }
        public string Name => GameLink.Label;
        public string Id { get; set; }
        #endregion

        #region Download URL

        private Uri _url;
        public Uri Url
        {
            get { return _url; }
            set
            {
                _url = value;
                OnPropertyChanged();
            }
        }

        public long UrlExpiresTimeStamp { get; set; }


        #endregion

        #region Download Data

        private DownloadItemStatus _status = DownloadItemStatus.Idle;

        public DownloadItemStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        private double _progress = 0;
        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    ProgressReadable = $"{value:0.00}%";
                    OnPropertyChanged();
                }
            }
        }

        private string _progressReadable = "0.00%";
        [DontSerialize]
        public string ProgressReadable
        {
            get => _progressReadable;
            set
            {
                if (_progressReadable != value)
                {
                    _progressReadable = value;
                    OnPropertyChanged();
                }
            }
        }

        private long _progressSize = 0;
        public long ProgressSize
        {
            get => _progressSize;
            set
            {
                if (_progressSize != value)
                {
                    _progressSize = value;
                    OnPropertyChanged();
                }
            }
        }

        private long _totalSize = 0;
        public long TotalSize
        {
            get => _totalSize;
            set
            {
                if (_totalSize != value)
                {
                    _progressSize = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _progressAndTotalSize = string.Empty;
        public string ProgressAndTotalSize
        {
            get => _progressAndTotalSize;
            set
            {
                if (_progressAndTotalSize != value)
                {
                    _progressAndTotalSize = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _speed = string.Empty;
        [DontSerialize]
        public string FormattedDownloadSpeedPerSecond
        {
            get => _speed;
            set
            {
                if (_speed != value)
                {
                    _speed = value;
                    OnPropertyChanged();
                }
            }
        }

        private TimeSpan _timeRemaining = TimeSpan.MinValue;
        [DontSerialize]
        public TimeSpan TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                if (_timeRemaining != value)
                {
                    _timeRemaining = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region File Details

        private string _fileName = string.Empty;
        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _downloadDirectory = string.Empty;
        public string DownloadDirectory
        {
            get => _downloadDirectory;
            set
            {
                if (_downloadDirectory != value)
                {
                    _downloadDirectory = value;
                    OnPropertyChanged();
                }
            }
        }

        [DontSerialize]
        public string DownloadPath => Path.Combine(DownloadDirectory, FileName);

        [DontSerialize]
        public string TemporaryDownloadPath => Path.Combine(DownloadDirectory, FileName + ".tmp");

        [DontSerialize]
        public bool IsComplete => Progress == 100;

        #endregion

        public DownloadData()
        {

        }

        public DownloadData(Game game, string id, JastAssetWrapper assetWrapper, Uri uri, string downloadDirectory)
        {
            AssetType = assetWrapper.Type;
            GameLink = assetWrapper.Asset;
            GameId = game.Id;
            Id = id;
            DownloadDirectory = downloadDirectory;
            SetUrl(uri);
        }

        public void UpdateProperties(DownloadProgressArgs downloadProgressArgs)
        {
            if (downloadProgressArgs is null)
            {
                Progress = 0;
                ProgressSize = 0;
                ProgressAndTotalSize = string.Empty;
                FormattedDownloadSpeedPerSecond = string.Empty;
                TimeRemaining = TimeSpan.MinValue;
                return;
            }
            else if (downloadProgressArgs.IsComplete)
            {
                Progress = downloadProgressArgs.ProgressPercentage;
                ProgressAndTotalSize = string.Format("{0}/{1}", downloadProgressArgs.FormattedBytesReceived, downloadProgressArgs.FormattedTotalBytesToReceive);
                FormattedDownloadSpeedPerSecond = string.Empty;
                TimeRemaining = downloadProgressArgs.TimeRemaining;
            }
            else
            {
                Progress = downloadProgressArgs.ProgressPercentage;
                ProgressAndTotalSize = string.Format("{0}/{1}", downloadProgressArgs.FormattedBytesReceived, downloadProgressArgs.FormattedTotalBytesToReceive);
                FormattedDownloadSpeedPerSecond = downloadProgressArgs.FormattedDownloadSpeedPerSecond;
                TimeRemaining = downloadProgressArgs.TimeRemaining;
            }

            ProgressSize = downloadProgressArgs.BytesReceived;
            TotalSize = downloadProgressArgs.TotalBytesToReceive;
        }

        public void SetUrl(Uri uri)
        {
            if (Url is null || FileName == Path.GetFileName(uri.LocalPath))
            {
                Url = uri;
                FileName = Path.GetFileName(uri.LocalPath);
                var queryParameters = HttpUtility.ParseQueryString(uri.Query);
                var unixExpiresTimeStamp = long.Parse(queryParameters["expires"]);
                UrlExpiresTimeStamp = unixExpiresTimeStamp;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
