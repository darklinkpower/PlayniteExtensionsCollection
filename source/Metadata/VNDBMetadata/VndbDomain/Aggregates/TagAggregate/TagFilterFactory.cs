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

namespace VNDBMetadata.VndbDomain.Aggregates.TagAggregate
{
    public class TagFilter : SimpleFilterBase
    {
        internal TagFilter(string filterName, string filterOperator, object value) : base(filterName, filterOperator, value)
        {

        }

        internal TagFilter(string filterName, string filterOperator, params object[] values) : base(filterName, filterOperator, values)
        {

        }
    }

    public class TagComplexFilter : ComplexFilterBase
    {
        internal TagComplexFilter(string filterOperator, params IFilter[] value) : base(filterOperator, value)
        {

        }
    }

    public static class TagFilterFactory
    {
        public static class Id
		{
			public static string FilterName = TagConstants.Filters.Id;
			public static bool CanBeNull { get; } = false;
			public static TagFilter EqualTo(string value) =>  new TagFilter(
				FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static TagFilter NotEqualTo(string value) => new TagFilter(
				FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static TagFilter GreaterThanOrEqual(string value) => new TagFilter(
				FilterName, Operators.Ordering.GreaterThanOrEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static TagFilter GreaterThan(string value) => new TagFilter(
				FilterName, Operators.Ordering.GreaterThan, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static TagFilter LessThanOrEqual(string value) => new TagFilter(
				FilterName, Operators.Ordering.LessThanOrEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static TagFilter LessThan(string value) => new TagFilter(
				FilterName, Operators.Ordering.LessThan, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
		}

        public static class Search
        {
            public static string FilterName = TagConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            public static TagFilter EqualTo(string value) => new TagFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
            public static TagFilter NotEqualTo(string value) => new TagFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
        }

        public static class Category
        {
            public static string FilterName = TagConstants.Filters.Category;
            public static bool CanBeNull { get; } = false;
            public static TagFilter EqualTo(TagCategoryEnum value) => new TagFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<TagCategoryEnum>(value) : value);
            public static TagFilter NotEqualTo(TagCategoryEnum value) => new TagFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<TagCategoryEnum>(value) : value);
        }

        public static TagComplexFilter And(params TagFilter[] value) => new TagComplexFilter(
            Operators.Predicates.And, Guard.Against.Null(value));

        public static TagComplexFilter Or(params TagFilter[] value) => new TagComplexFilter(
            Operators.Predicates.Or, Guard.Against.Null(value));
    }

}