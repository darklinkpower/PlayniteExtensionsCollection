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

namespace VNDBMetadata.VndbDomain.Aggregates.ProducerAggregate
{
    public static class ProducerFilterFactory
	{
        public static class Id
        {
            public static string FilterName = ProducerConstants.Filters.Id;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Producer> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Producer>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Producer> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Producer> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Producer> GreaterThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Producer> GreaterThan(string value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Producer> LessThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Producer> LessThan(string value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        public static class Search
        {
            public static string FilterName = ProducerConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Producer> CreateFilter(string operatorString, string value) =>
                 FilterFactory.CreateFilter<Producer>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Producer> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Producer> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class Language
        {
            public static string FilterName = ProducerConstants.Filters.Lang;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Producer> CreateFilter(string operatorString, LanguageEnum value) =>
                FilterFactory.CreateFilter<Producer, LanguageEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Producer> EqualTo(LanguageEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Producer> NotEqualTo(LanguageEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class Type
        {
            public static string FilterName = ProducerConstants.Filters.Type;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Producer> CreateFilter(string operatorString, ProducerTypeEnum value) =>
                FilterFactory.CreateFilter<Producer, ProducerTypeEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Producer> EqualTo(ProducerTypeEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Producer> NotEqualTo(ProducerTypeEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static ComplexFilterBase<Producer> And(params SimpleFilterBase<Producer>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.And, values);

        public static ComplexFilterBase<Producer> Or(params SimpleFilterBase<Producer>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.Or, values);
    }

}