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
    public class TimeSpanToStringReadable : IValueConverter
    {
        private static readonly string HoursFormat = ResourceProvider.GetString("LOC_JUL_JastDownloaderTimeHoursMinsFormat");
        private static readonly string MinutesFormat = ResourceProvider.GetString("LOC_JUL_JastDownloaderTimeMinsSecondsFormat");
        private static readonly string SecondsFormat = ResourceProvider.GetString("LOC_JUL_JastDownloaderTimeSecondsFormat");

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                if (timeSpan.Equals(TimeSpan.MinValue) || timeSpan.Equals(TimeSpan.MaxValue))
                {
                    return string.Empty;
                }
                else if (timeSpan.TotalHours >= 1)
                {
                    return string.Format(HoursFormat, timeSpan.Hours, timeSpan.Minutes);
                }
                else if (timeSpan.TotalMinutes >= 1)
                {
                    return string.Format(MinutesFormat, timeSpan.Minutes, timeSpan.Seconds);
                }
                else
                {
                    return string.Format(SecondsFormat, timeSpan.Seconds);
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