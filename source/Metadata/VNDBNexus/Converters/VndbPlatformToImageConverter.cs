using PluginsCommon.Converters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using VndbApiDomain.SharedKernel;

namespace VNDBNexus.Converters
{
    public class VndbPlatformToImageConverter : IValueConverter
    {
        private readonly ConcurrentDictionary<PlatformEnum, BitmapImage> _platormToBitmapImageMapper;

        public VndbPlatformToImageConverter()
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var iconsDirectory = Path.Combine(assemblyDirectory, "Resources", "VndbIcons");
            _platormToBitmapImageMapper = new ConcurrentDictionary<PlatformEnum, BitmapImage>()
            {
                [PlatformEnum.Windows] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g581.png")),
                [PlatformEnum.Linux] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g327.png")),
                [PlatformEnum.MacOs] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g343.png")),
                [PlatformEnum.Website] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g567.png")),
                [PlatformEnum.ThreeDO] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g555.png")),
                [PlatformEnum.AppleIProduct] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g315.png")),
                [PlatformEnum.Android] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g259.png")),
                [PlatformEnum.BluRayPlayer] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g267.png")),
                [PlatformEnum.DOS] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g277.png")),
                [PlatformEnum.DVDPlayer] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g285.png")),
                [PlatformEnum.Dreamcast] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g281.png")),
                [PlatformEnum.Famicom] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g391.png")),
                [PlatformEnum.SuperFamicom] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g525.png")),
                [PlatformEnum.FM7] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g289.png")),
                [PlatformEnum.FM8] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g293.png")),
                [PlatformEnum.FMTowns] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g297.png")),
                [PlatformEnum.GameBoyAdvance] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g303.png")),
                [PlatformEnum.GameBoyColor] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g311.png")),
                [PlatformEnum.MSX] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g377.png")),
                [PlatformEnum.NintendoDS] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g387.png")),
                [PlatformEnum.NintendoSwitch] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g545.png")),
                [PlatformEnum.NintendoWii] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g571.png")),
                [PlatformEnum.NintendoWiiU] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g589.png")),
                [PlatformEnum.Nintendo3DS] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g383.png")),
                [PlatformEnum.PC88] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g405.png")),
                [PlatformEnum.PC98] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g413.png")),
                [PlatformEnum.PCEngine] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g419.png")),
                [PlatformEnum.PCFX] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g441.png")),
                [PlatformEnum.PlayStationPortable] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g479.png")),
                [PlatformEnum.PlayStation1] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g459.png")),
                [PlatformEnum.PlayStation2] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g463.png")),
                [PlatformEnum.PlayStation3] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g467.png")),
                [PlatformEnum.PlayStation4] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g471.png")),
                [PlatformEnum.PlayStation5] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g475.png")),
                [PlatformEnum.PlayStationVita] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g483.png")),
                [PlatformEnum.SegaMegaDrive] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g535.png")),
                [PlatformEnum.SegaMegaCD] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g515.png")),
                [PlatformEnum.SegaSaturn] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g501.png")),
                [PlatformEnum.VNDS] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g563.png")),
                [PlatformEnum.SharpX1] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g593.png")),
                [PlatformEnum.SharpX68000] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g597.png")),
                [PlatformEnum.Xbox] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g623.png")),
                [PlatformEnum.Xbox360] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g641.png")),
                [PlatformEnum.XboxOne] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g645.png")),
                [PlatformEnum.XboxX_S] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g651.png")),
                [PlatformEnum.OtherMobile] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g371.png")),
                [PlatformEnum.Other] = ConvertersUtilities.CreateResizedBitmapImageFromPath(Path.Combine(iconsDirectory, "g395.png")),
            };
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PlatformEnum platform && _platormToBitmapImageMapper.TryGetValue(platform, out var bitmapImage))
            {
                return bitmapImage;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
