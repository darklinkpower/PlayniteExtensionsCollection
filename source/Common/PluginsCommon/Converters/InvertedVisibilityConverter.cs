using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PluginsCommon.Converters
{
    public class InvertedVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return (visibility == Visibility.Collapsed || visibility == Visibility.Hidden)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}