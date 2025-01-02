using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace JastUsaLibrary.Converters
{
    public class FileNameToExtensionGlyphCodeConverter : IValueConverter
    {
        private static readonly Dictionary<string, char> fileExtensionToGlyphMapper = new Dictionary<string, char>
        {
            [".pdf"] = '\uEB1E',
            [".zip"] = '\uEB30',
            [".rar"] = '\uEB30',
            [".exe"] = '\uEB11'
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string fileName)
            {
                var fileExtension = Path.GetExtension(fileName);
                if (fileExtensionToGlyphMapper.TryGetValue(fileExtension.ToLower(), out var glyph))
                {
                    return glyph;
                }

                return '\uEC57';
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
