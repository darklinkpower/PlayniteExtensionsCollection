using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.SharedKernel;
using VndbApiDomain.ProducerAggregate;
using VndbApiInfrastructure.SharedKernel.Filters;
using VndbApiInfrastructure.SharedKernel.Requests;

namespace VndbApiInfrastructure.ProducerAggregate
{
    public static class ProducerFilterFactory
	{
        /// <summary>
        /// vndbid
        /// </summary>
        public static class Id
        {
            public static string FilterName = ProducerConstants.Filters.Id;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Producer> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Producer>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Producer> EqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Producer> NotEqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Producer> GreaterThanOrEqual(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Producer> GreaterThan(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Producer> LessThanOrEqual(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Producer> LessThan(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// String search.
        /// </summary>
        public static class Search
        {
            public static string FilterName = ProducerConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Producer> CreateFilter(string operatorString, string value) =>
                 FilterFactory.CreateFilter<Producer>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Producer> EqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Producer> NotEqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Language.
        /// </summary>
        public static class Language
        {
            public static string FilterName = ProducerConstants.Filters.Lang;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Producer> CreateFilter(string operatorString, LanguageEnum value) =>
                FilterFactory.CreateFilter<Producer, LanguageEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Producer> EqualTo(LanguageEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Producer> NotEqualTo(LanguageEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Producer type.
        /// </summary>
        public static class Type
        {
            public static string FilterName = ProducerConstants.Filters.Type;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Producer> CreateFilter(string operatorString, ProducerTypeEnum value) =>
                FilterFactory.CreateFilter<Producer, ProducerTypeEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Producer> EqualTo(ProducerTypeEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Producer> NotEqualTo(ProducerTypeEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        public static ComplexFilterBase<Producer> And(params SimpleFilterBase<Producer>[] values) =>
            FilterFactory.CreateComplexFilter(RequestConstants.Operators.Predicates.And, values);

        public static ComplexFilterBase<Producer> Or(params SimpleFilterBase<Producer>[] values) =>
            FilterFactory.CreateComplexFilter(RequestConstants.Operators.Predicates.Or, values);
    }

}