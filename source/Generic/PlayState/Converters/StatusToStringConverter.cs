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
    public class StatusToStringConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (SuspendModes)values[0];
            var isSuspended = (bool)values[1];
            switch (source)
            {
                case SuspendModes.Processes:
                    return isSuspended ? ResourceProvider.GetString("LOCPlayState_StatusSuspendedMessage") : ResourceProvider.GetString("LOCPlayState_StatusResumedMessage");
                case SuspendModes.Playtime:
                    return isSuspended ? ResourceProvider.GetString("LOCPlayState_StatusPlaytimeSuspendedMessage") : ResourceProvider.GetString("LOCPlayState_StatusPlaytimeResumedMessage");
                default:
                    return string.Empty;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
