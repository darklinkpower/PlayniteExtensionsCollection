using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using VndbApiDomain.VisualNovelAggregate;

namespace VNDBNexus.Converters
{
    public class VnRelationFiltererConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
            {
                return null;
            }

            if (!(values[0] is IEnumerable<VnRelation> relations))
            {
                return null;
            }

            if (!(values[1] is VnRelationTypeEnum relationType))
            {
                return null;
            }

            if (!(values[2] is bool onlyOfficial))
            {
                return null;
            }

            return relations.Where(x => x.Relation == relationType && (x.RelationOfficial || !onlyOfficial))
                .OrderBy(x => x.RelationOfficial)
                .ThenBy(x => x.ReleaseDate.Year);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
