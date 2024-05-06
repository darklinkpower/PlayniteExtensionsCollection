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
                    OnPropertyChanged();
                }
            }
        }

        private string _size = string.Empty;
        public string Size
        {
            get => _size;
            set
            {
                if (_size != value)
                {
                    _size = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _speed = string.Empty;
        [DontSerialize]
        public string Speed
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

        private string _timeLeft = string.Empty;
        [DontSerialize]
        public string TimeLeft
        {
            get => _timeLeft;
            set
            {
                if (_timeLeft != value)
                {
                    _timeLeft = value;
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
                Size = string.Empty;
                Speed = string.Empty;
                TimeLeft = string.Empty;
            }
            else if (downloadProgressArgs.IsComplete)
            {
                Progress = downloadProgressArgs.ProgressPercentage;
                Size = string.Format("{0}/{1}", downloadProgressArgs.FormattedBytesReceived, downloadProgressArgs.FormattedTotalBytesToReceive);
                Speed = string.Empty;
                TimeLeft = GetTimeSpanReadable(downloadProgressArgs.TimeRemaining);
            }
            else
            {
                Progress = downloadProgressArgs.ProgressPercentage;
                Size = string.Format("{0}/{1}", downloadProgressArgs.FormattedBytesReceived, downloadProgressArgs.FormattedTotalBytesToReceive);
                Speed = downloadProgressArgs.FormattedDownloadSpeedPerSecond;
                TimeLeft = GetTimeSpanReadable(downloadProgressArgs.TimeRemaining);
            }
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

        private static string GetTimeSpanReadable(TimeSpan timeSpan)
        {
            if (timeSpan.Equals(TimeSpan.MinValue))
            {
                return string.Empty;
            }
            else if (timeSpan.TotalDays >= 1)
            {
                return $"{timeSpan.Days} d {timeSpan.Hours} h";
            }
            else if (timeSpan.TotalHours >= 1)
            {
                return $"{timeSpan.Hours} h {timeSpan.Minutes} m";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                return $"{timeSpan.Minutes} m {timeSpan.Seconds} s";
            }
            else
            {
                return $"{timeSpan.Seconds} s";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
