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
    public class GamePadToKeyboardHotkeyModesToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (GamePadToKeyboardHotkeyModes)value;
            switch (source)
            {
                case GamePadToKeyboardHotkeyModes.Disabled:
                    return "Disabled";
                case GamePadToKeyboardHotkeyModes.OnGameRunning:
                    return "Game running";
                case GamePadToKeyboardHotkeyModes.OnGameNotRunning:
                    return "Game not running";
                case GamePadToKeyboardHotkeyModes.Always:
                    return "Always";
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