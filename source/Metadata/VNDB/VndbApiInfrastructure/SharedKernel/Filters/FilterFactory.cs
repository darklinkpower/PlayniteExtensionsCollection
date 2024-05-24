using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApiInfrastructure.SharedKernel.Filters
{
    public static class FilterFactory
    {
        internal static SimpleFilterBase<TFilterType> CreateFilter<TFilterType>(string filterName, bool canBeNull, string operatorString, string value)
        {
            if (!canBeNull)
            {
                Guard.Against.NullOrEmpty(value, "Array contains null values.");
            }

            return new SimpleFilterBase<TFilterType>(filterName, operatorString, value);
        }

        internal static SimpleFilterBase<TFilterType> CreateFilter<TFilterType, TValue>(string filterName, bool canBeNull, string operatorString, TValue value)
        {
            if (!canBeNull)
            {
                
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Value cannot be null.");
                }
                else
                {
                    var valueType = typeof(TValue);
                    if (!valueType.IsEnum && EqualityComparer<TValue>.Default.Equals(value, default(TValue)))
                    {
                        throw new ArgumentNullException(nameof(value), "Value cannot be default.");
                    }
                }
            }

            return new SimpleFilterBase<TFilterType>(filterName, operatorString, value);
        }

        internal static SimpleFilterBase<TFilterType> CreateFilter<TFilterType, T>(string filterName, bool canBeNull, string operatorString, params T[] values) where T : struct
        {
            if (!canBeNull)
            {
                foreach (T value in values)
                {
                    Guard.Against.Null<T>(value, "Array contains null values.");
                }
            }

            return new SimpleFilterBase<TFilterType>(filterName, operatorString, values);
        }

        internal static SimpleFilterBase<TFilterType> CreateFilter<TFilterType>(string filterName, bool canBeNull, string operatorString, params object[] values)
        {
            if (!canBeNull)
            {
                foreach (object value in values)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException(nameof(value), "Value cannot be null.");
                    }
                }
            }

            return new SimpleFilterBase<TFilterType>(filterName, operatorString, values);
        }

        internal static ComplexFilterBase<TFilterType> CreateComplexFilter<TFilterType>(string operatorString, SimpleFilterBase<TFilterType>[] values)
        {
            Guard.Against.Null(values);
            return new ComplexFilterBase<TFilterType>(operatorString, values);
        }
    }
}
