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
    public class IconPathConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var fullFilePath = value != null && !((string)value).IsNullOrEmpty() ? API.Instance.Database.GetFullFilePath((string)value) : null;
            if (fullFilePath != null)
            {
                return fullFilePath;
            }
            else if (ResourceProvider.GetResource("DefaultGameIcon") != null)
            {
                return ResourceProvider.GetResource("DefaultGameIcon");
            }
            else
            {
                return null;
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
