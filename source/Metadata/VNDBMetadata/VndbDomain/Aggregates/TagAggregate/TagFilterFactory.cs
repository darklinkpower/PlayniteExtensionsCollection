using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common;
using VNDBMetadata.VndbDomain.Common.Constants;
using VNDBMetadata.VndbDomain.Common.Enums;
using VNDBMetadata.VndbDomain.Common.Filters;
using VNDBMetadata.VndbDomain.Common.Interfaces;

namespace VNDBMetadata.VndbDomain.Aggregates.TagAggregate
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
            private static SimpleFilterBase<VndbTag> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<VndbTag>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VndbTag> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VndbTag> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<VndbTag> GreaterThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<VndbTag> GreaterThan(string value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<VndbTag> LessThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<VndbTag> LessThan(string value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// String search.
        /// </summary>
        public static class Search
        {
            public static string FilterName = TagConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<VndbTag> CreateFilter(string operatorString, string value) =>
                 FilterFactory.CreateFilter<VndbTag>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VndbTag> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VndbTag> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Searches tag category.
        /// </summary>
        public static class Category
        {
            public static string FilterName = TagConstants.Filters.Category;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<VndbTag> CreateFilter(string operatorString, TagCategoryEnum value) =>
                FilterFactory.CreateFilter<VndbTag, TagCategoryEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VndbTag> EqualTo(TagCategoryEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VndbTag> NotEqualTo(TagCategoryEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static ComplexFilterBase<VndbTag> And(params SimpleFilterBase<VndbTag>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.And, values);

        public static ComplexFilterBase<VndbTag> Or(params SimpleFilterBase<VndbTag>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.Or, values);
    }

}