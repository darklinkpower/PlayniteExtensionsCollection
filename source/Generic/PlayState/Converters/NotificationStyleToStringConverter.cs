using Playnite.SDK;
using PlayState.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace PlayState.Converters
{
    public class NotificationStyleToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (NotificationStyles)value;
            switch (source)
            {
                case NotificationStyles.Toast:
                    return ResourceProvider.GetString("LOCPlayState_NotificationStyleWindowsToast");
                case NotificationStyles.SplashWindow:
                    return ResourceProvider.GetString("LOCPlayState_NotificationStyleSplashWindow");
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
