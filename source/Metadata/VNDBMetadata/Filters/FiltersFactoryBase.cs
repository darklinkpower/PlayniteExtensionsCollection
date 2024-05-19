using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDB.ApiConstants;

namespace VNDBMetadata.Filters
{
    public class GenericFilter<TFilterType>
    {
        public string FilterName { get; }
        
        public string FilterOperator { get; }
        public object Value { get; }

        internal GenericFilter(string filterName, string filterOperator, object value)
        {
            FilterName = filterName;
            FilterOperator = filterOperator;
            Value = value;
        }
    }

    public class FiltersFactoryBase<TFilterType, TValue>
    {
        public static GenericFilter<TFilterType> CreateFilter(string filterName, string filterOperator, TValue value, bool canBeNull = false)
        {
            if (!canBeNull && value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new GenericFilter<TFilterType>(filterName, filterOperator, value);
        }

        public static GenericFilter<TFilterType> EqualTo(string filterName, TValue value) => CreateFilter(
                filterName, Operators.Matching.IsEqual, value);
        public static GenericFilter<TFilterType> NotEqualTo(string filterName, TValue value) => CreateFilter(
                filterName, Operators.Matching.NotEqual, value);
    }


}
