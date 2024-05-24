using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using VndbApiDomain.CharacterAggregate;
using VndbApiDomain.SharedKernel;
using VndbApiDomain.TraitAggregate;

namespace VNDBFuze.Converters
{
    public class TraitGroupAndSpoilerFilterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count() != 3)
            {
                return null;
            }
            
            if (values[0] is IEnumerable<CharacterTrait> traits && values[1] is TraitGroupEnum group && values[2] is SpoilerLevelEnum maxSpoilerLevel)
            {
                return traits.Where(t =>
                    t.Group == group &&
                    (
                        t.SpoilerLevel == SpoilerLevelEnum.None ||
                        (t.SpoilerLevel == SpoilerLevelEnum.Minimum && maxSpoilerLevel == SpoilerLevelEnum.Minimum) ||
                        maxSpoilerLevel == SpoilerLevelEnum.Major
                    )
                ).OrderBy(x => x.Name);
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}