using Playnite.SDK;
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
        private static readonly string _officialPatchMtlString = ResourceProvider.GetString("LOC_VndbNexus_ReleaseOfficialPatchMtl");
        private static readonly string _officialPatchString = ResourceProvider.GetString("LOC_VndbNexus_ReleaseOfficialPatch");
        private static readonly string _officialReleaseMtlString = ResourceProvider.GetString("LOC_VndbNexus_ReleaseOfficialReleaseMtl");
        private static readonly string _officialReleaseString = string.Empty;

        private static readonly string _unofficialPatchMtlString = ResourceProvider.GetString("LOC_VndbNexus_ReleaseUnofficialPatchMtl");
        private static readonly string _unofficialPatchString = ResourceProvider.GetString("LOC_VndbNexus_ReleaseUnofficialPatch");
        private static readonly string _unofficialReleaseMtlString = ResourceProvider.GetString("LOC_VndbNexus_ReleaseUnofficialReleaseMtl");
        private static readonly string _unofficialReleaseString = ResourceProvider.GetString("LOC_VndbNexus_ReleaseUnofficialRelease");

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
