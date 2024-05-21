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

namespace VNDBMetadata.VndbDomain.Aggregates.TraitAggregate
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
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Trait> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Trait> GreaterThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Trait> GreaterThan(string value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Trait> LessThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Trait> LessThan(string value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
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
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Trait> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static ComplexFilterBase<Trait> And(params SimpleFilterBase<Trait>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.And, values);

        public static ComplexFilterBase<Trait> Or(params SimpleFilterBase<Trait>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.Or, values);
    }

}