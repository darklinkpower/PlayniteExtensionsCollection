using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Infrastructure.SharedKernel.Filters;
using VndbApi.Domain.TraitAggregate;
using VndbApi.Infrastructure.SharedKernel.Requests;

namespace VndbApi.Infrastructure.TraitAggregate
{

    public static class TraitFilterFactory
    {
        /// <summary>
        /// vndbid
        /// </summary>
        public static class Id
        {
            public static string FilterName = TraitConstants.Filters.Id;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Trait> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Trait>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Trait> EqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Trait> NotEqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Trait> GreaterThanOrEqual(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Trait> GreaterThan(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Trait> LessThanOrEqual(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Trait> LessThan(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// String search.
        /// </summary>
        public static class Search
        {
            public static string FilterName = TraitConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Trait> CreateFilter(string operatorString, string value) =>
                 FilterFactory.CreateFilter<Trait>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Trait> EqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Trait> NotEqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        public static ComplexFilterBase<Trait> And(params SimpleFilterBase<Trait>[] values) =>
            FilterFactory.CreateComplexFilter(RequestConstants.Operators.Predicates.And, values);

        public static ComplexFilterBase<Trait> Or(params SimpleFilterBase<Trait>[] values) =>
            FilterFactory.CreateComplexFilter(RequestConstants.Operators.Predicates.Or, values);
    }

}