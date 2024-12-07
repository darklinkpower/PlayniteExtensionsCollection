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
    public class CharacterBirthdayToStringConverter : IValueConverter
    {
        private static readonly Dictionary<int, string> _monthNames = new Dictionary<int, string>
        {
            { 1, ResourceProvider.GetString("LOC_VndbNexus_MonthJanuary") },
            { 2, ResourceProvider.GetString("LOC_VndbNexus_MonthFebruary") },
            { 3, ResourceProvider.GetString("LOC_VndbNexus_MonthMarch") },
            { 4, ResourceProvider.GetString("LOC_VndbNexus_MonthApril") },
            { 5, ResourceProvider.GetString("LOC_VndbNexus_MonthMay") },
            { 6, ResourceProvider.GetString("LOC_VndbNexus_MonthJune") },
            { 7, ResourceProvider.GetString("LOC_VndbNexus_MonthJuly") },
            { 8, ResourceProvider.GetString("LOC_VndbNexus_MonthAugust") },
            { 9, ResourceProvider.GetString("LOC_VndbNexus_MonthSeptember") },
            { 10, ResourceProvider.GetString("LOC_VndbNexus_MonthOctober") },
            { 11, ResourceProvider.GetString("LOC_VndbNexus_MonthNovember") },
            { 12, ResourceProvider.GetString("LOC_VndbNexus_MonthDecember") }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CharacterBirthday birthday && _monthNames.TryGetValue(birthday.Month, out string monthName))
            {
                return $"{birthday.Day} {monthName}";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
