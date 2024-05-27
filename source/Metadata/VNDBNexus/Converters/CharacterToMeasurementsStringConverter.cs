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
        private static readonly string _heightFormatString = "Height: {0}cm";
        private static readonly string _weightFormatString = "Weight: {0}kg";
        private static readonly string _bustWeightHipsFormatString = "Bust-Waist-Hips: {0}-{1}-{2}cm";

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
