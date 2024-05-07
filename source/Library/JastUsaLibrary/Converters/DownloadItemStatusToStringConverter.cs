using JastUsaLibrary.DownloadManager.Enums;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace JastUsaLibrary.Converters
{
    public class DownloadItemStatusToStringConverter : IValueConverter
    {
        private static readonly string IdleString = ResourceProvider.GetString("LOCJast_Usa_Library_DownloadStatusIdle");
        private static readonly string DownloadingString = ResourceProvider.GetString("LOCJast_Usa_Library_DownloadStatusDownloading");
        private static readonly string PausedString = ResourceProvider.GetString("LOCJast_Usa_Library_DownloadStatusPaused");
        private static readonly string CompletedString = ResourceProvider.GetString("LOCJast_Usa_Library_DownloadStatusCompleted");
        private static readonly string FailedString = ResourceProvider.GetString("LOCJast_Usa_Library_DownloadStatusFailed");
        private static readonly string CanceledString = ResourceProvider.GetString("LOCJast_Usa_Library_DownloadStatusCanceled");
        private static readonly string ExtractingString = ResourceProvider.GetString("LOCJast_Usa_Library_DownloadStatusExtracting");
        private static readonly string ExtractionCompletedString = ResourceProvider.GetString("LOCJast_Usa_Library_DownloadStatusExtractionCompleted");
        private static readonly string ExtractionFailedString = ResourceProvider.GetString("LOCJast_Usa_Library_DownloadStatusExtractionExtractionFailed");

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is DownloadItemStatus status)
            {
                switch (status)
                {
                    case DownloadItemStatus.Idle:
                        return string.Empty;
                    case DownloadItemStatus.Downloading:
                        return DownloadingString;
                    case DownloadItemStatus.Paused:
                        return PausedString;
                    case DownloadItemStatus.Completed:
                        return CompletedString;
                    case DownloadItemStatus.Failed:
                        return FailedString;
                    case DownloadItemStatus.Canceled:
                        return CanceledString;
                    case DownloadItemStatus.Extracting:
                        return ExtractingString;
                    case DownloadItemStatus.ExtractionCompleted:
                        return ExtractionCompletedString;
                    case DownloadItemStatus.ExtractionFailed:
                        return ExtractionFailedString;
                    default:
                        return value.ToString();
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
