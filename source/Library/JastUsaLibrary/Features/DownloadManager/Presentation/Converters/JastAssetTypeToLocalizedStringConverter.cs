using JastUsaLibrary.DownloadManager.Domain.Enums;
using JastUsaLibrary.Services.JastUsaIntegration.Domain.Enums;
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
    public class JastAssetTypeToLocalizedStringConverter : IValueConverter
    {
        private static readonly string GameString = ResourceProvider.GetString("LOC_JUL_AssetTypeGame");
        private static readonly string ExtraString = ResourceProvider.GetString("LOC_JUL_AssetTypeExtra");
        private static readonly string PatchString = ResourceProvider.GetString("LOC_JUL_AssetTypePatch");

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is JastDownloadType assetType)
            {
                switch (assetType)
                {
                    case JastDownloadType.Game:
                        return GameString;
                    case JastDownloadType.Extra:
                        return ExtraString;
                    case JastDownloadType.Patch:
                        return PatchString;
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
