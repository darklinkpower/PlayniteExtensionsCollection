using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using VndbApiDomain.CharacterAggregate;
using VndbApiDomain.ReleaseAggregate;

namespace VNDBNexus.Converters
{
    public class ReleaseToAdditionalnformationConverter : IValueConverter
    {
        private static readonly string _officialPatchMtlString = "(machine translation patch)";
        private static readonly string _officialPatchString = "(patch)";
        private static readonly string _officialReleaseMtlString = "(machine translation)";
        private static readonly string _officialReleaseString = string.Empty;

        private static readonly string _unofficialPatchMtlString = "(unofficial machine translation patch)";
        private static readonly string _unofficialPatchString = "(unofficial patch)";
        private static readonly string _unofficialReleaseMtlString = "(unofficial machine translation)";
        private static readonly string _unofficialReleaseString = "(unofficial)";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Release release)
            {
                var isMachineTranslated = release.LanguagesAvailability?
                    .Any(x => x.MachineTranslated) == true;
                if (release.IsOfficial)
                {
                    if (release.IsPatch)
                    {
                        if (isMachineTranslated)
                        {
                            return _officialPatchMtlString;
                        }
                        else
                        {
                            return _officialPatchString;
                        }
                    }
                    else
                    {
                        if (isMachineTranslated)
                        {
                            return _officialReleaseMtlString;
                        }
                        else
                        {
                            return _officialReleaseString;
                        }
                    }
                }
                else
                {
                    if (release.IsPatch)
                    {
                        if (isMachineTranslated)
                        {
                            return _unofficialPatchMtlString;
                        }
                        else
                        {
                            return _unofficialPatchString;
                        }
                    }
                    else
                    {
                        if (isMachineTranslated)
                        {
                            return _unofficialReleaseMtlString;
                        }
                        else
                        {
                            return _unofficialReleaseString;
                        }
                    }
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
