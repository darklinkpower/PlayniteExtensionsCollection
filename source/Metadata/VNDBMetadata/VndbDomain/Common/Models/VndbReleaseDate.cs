using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Converters;
using VNDBMetadata.VndbDomain.Common.Enums;

namespace VNDBMetadata.VndbDomain.Common.Models
{
    public class VndbReleaseDate
    {
        private int _year;
        private int? _month;
        private int? _day;

        public int Year
        {
            get => _year;
            set
            {
                _year = Guard.Against.NotLessThan(value, 1980);
            }
        }

        public int? Month
        {
            get => _month;
            set
            {
                if (value.HasValue)
                {
                    Guard.Against.NotInRange(value.Value, 1, 12);
                }

                _month = value;
            }
        }

        public int? Day
        {
            get => _day;
            set
            {
                if (value.HasValue)
                {
                    if (_month.HasValue)
                    {
                        if (value < 1 || value > DateTime.DaysInMonth(_year, _month.Value))
                        {
                            throw new ArgumentOutOfRangeException(nameof(value), "Day is not valid for the given month and year.");
                        }
                    }
                    else
                    {
                        Guard.Against.NotInRange(value.Value, 1, 31);
                    }
                }

                _day = value;
            }
        }

        public override string ToString()
        {
            if (!_month.HasValue)
            {
                return $"{_year:D2}";
            }

            if (!_day.HasValue)
            {
                return $"{_year:D2}-{_month:D2}";
            }

            return $"{_year:D2}-{_month:D2}-{_day:D2}";
        }
    }
}