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
        public static class Id
        {
            public static string FilterName = TagConstants.Filters.Id;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Tag> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Tag>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Tag> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Tag> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Tag> GreaterThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Tag> GreaterThan(string value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Tag> LessThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Tag> LessThan(string value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        public static class Search
        {
            public static string FilterName = TagConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Tag> CreateFilter(string operatorString, string value) =>
                 FilterFactory.CreateFilter<Tag>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Tag> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Tag> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class Category
        {
            public static string FilterName = TagConstants.Filters.Category;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Tag> CreateFilter(string operatorString, TagCategoryEnum value) =>
                FilterFactory.CreateFilter<Tag, TagCategoryEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Tag> EqualTo(TagCategoryEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Tag> NotEqualTo(TagCategoryEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static ComplexFilterBase<Tag> And(params SimpleFilterBase<Tag>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.And, values);

        public static ComplexFilterBase<Tag> Or(params SimpleFilterBase<Tag>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.Or, values);
    }

}