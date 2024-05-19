using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Constants;
using VNDBMetadata.VndbDomain.Common.Enums;
using VNDBMetadata.VndbDomain.Common.Filters;
using VNDBMetadata.VndbDomain.Common.Interfaces;

namespace VNDBMetadata.VndbDomain.Aggregates.TraitAggregate
{
    public class TraitFilter : SimpleFilterBase
    {
        internal TraitFilter(string filterName, string filterOperator, object value) : base(filterName, filterOperator, value)
        {

        }

        internal TraitFilter(string filterName, string filterOperator, params object[] values) : base(filterName, filterOperator, values)
        {

        }
    }

    public class TraitComplexFilter : ComplexFilterBase
    {
        internal TraitComplexFilter(string filterOperator, params IFilter[] value) : base(filterOperator, value)
        {

        }
    }

    public static class TraitFilterFactory
    {
        public static class Id
		{
			public static string FilterName = TraitConstants.Filters.Id;
			public static bool CanBeNull { get; } = false;
			public static TraitFilter EqualTo(string value) =>  new TraitFilter(
				FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static TraitFilter NotEqualTo(string value) => new TraitFilter(
				FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static TraitFilter GreaterThanOrEqual(string value) => new TraitFilter(
				FilterName, Operators.Ordering.GreaterThanOrEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static TraitFilter GreaterThan(string value) => new TraitFilter(
				FilterName, Operators.Ordering.GreaterThan, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static TraitFilter LessThanOrEqual(string value) => new TraitFilter(
				FilterName, Operators.Ordering.LessThanOrEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static TraitFilter LessThan(string value) => new TraitFilter(
				FilterName, Operators.Ordering.LessThan, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
		}

        public static class Search
        {
            public static string FilterName = TraitConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            public static TraitFilter EqualTo(string value) => new TraitFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
            public static TraitFilter NotEqualTo(string value) => new TraitFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
        }

        public static TraitComplexFilter And(params TraitFilter[] value) => new TraitComplexFilter(
            Operators.Predicates.And, Guard.Against.Null(value));

        public static TraitComplexFilter Or(params TraitFilter[] value) => new TraitComplexFilter(
            Operators.Predicates.Or, Guard.Against.Null(value));
    }

}