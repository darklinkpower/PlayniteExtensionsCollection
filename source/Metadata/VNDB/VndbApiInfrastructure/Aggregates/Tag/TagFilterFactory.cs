using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;
using VndbApiDomain.TagAggregate;
using VndbApiInfrastructure.SharedKernel;
using VndbApiInfrastructure.SharedKernel.Filters;
using VndbApiInfrastructure.SharedKernel.Requests;

namespace VndbApiInfrastructure.TagAggregate
{

    public static class TagFilterFactory
    {
        /// <summary>
        /// vndbid
        /// </summary>
        public static class Id
        {
            public static string FilterName = TagConstants.Filters.Id;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Tag> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Tag>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Tag> EqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Tag> NotEqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Tag> GreaterThanOrEqual(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Tag> GreaterThan(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Tag> LessThanOrEqual(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Tag> LessThan(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// String search.
        /// </summary>
        public static class Search
        {
            public static string FilterName = TagConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Tag> CreateFilter(string operatorString, string value) =>
                 FilterFactory.CreateFilter<Tag>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Tag> EqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Tag> NotEqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Searches tag category.
        /// </summary>
        public static class Category
        {
            public static string FilterName = TagConstants.Filters.Category;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Tag> CreateFilter(string operatorString, TagCategoryEnum value) =>
                FilterFactory.CreateFilter<Tag, TagCategoryEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Tag> EqualTo(TagCategoryEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Tag> NotEqualTo(TagCategoryEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        public static ComplexFilterBase<Tag> And(params SimpleFilterBase<Tag>[] values) =>
            FilterFactory.CreateComplexFilter(RequestConstants.Operators.Predicates.And, values);

        public static ComplexFilterBase<Tag> Or(params SimpleFilterBase<Tag>[] values) =>
            FilterFactory.CreateComplexFilter(RequestConstants.Operators.Predicates.Or, values);
    }

}