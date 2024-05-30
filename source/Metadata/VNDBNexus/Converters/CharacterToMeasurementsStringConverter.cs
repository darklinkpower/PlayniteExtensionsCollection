using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using VndbApiDomain.CharacterAggregate;

namespace VNDBNexus.Converters
{
    public class CharacterToMeasurementsStringConverter : IValueConverter
    {
        private static readonly string _heightFormatString = ResourceProvider.GetString("LOC_VndbNexus_CharacterHeightFormat");
        private static readonly string _weightFormatString = ResourceProvider.GetString("LOC_VndbNexus_CharacterWeightFormat");
        private static readonly string _bustWeightHipsFormatString = ResourceProvider.GetString("LOC_VndbNexus_CharacterBustWaistHipsFormat");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Character character)
            {
                var parts = new[]
                {
                    character.Height.HasValue ? string.Format(_heightFormatString, character.Height) : null,
                    character.Weight.HasValue ? string.Format(_weightFormatString, character.Weight) : null,
                    character.Bust.HasValue && character.Waist.HasValue && character.Hips.HasValue
                        ? string.Format(_bustWeightHipsFormatString, character.Bust, character.Waist, character.Hips)
                        : null
                }.Where(part => !string.IsNullOrWhiteSpace(part));

                if (parts.Count() > 0)
                {
                    return string.Join(", ", parts);
                }

                return string.Empty;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
