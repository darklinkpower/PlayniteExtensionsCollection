using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common;
using VNDBFuze.VndbDomain.Common.Constants;
using VNDBFuze.VndbDomain.Common.Enums;
using VNDBFuze.VndbDomain.Common.Filters;
using VNDBFuze.VndbDomain.Common.Interfaces;

namespace VNDBFuze.VndbDomain.Aggregates.StaffAggregate
{
    public static class StaffFilterFactory
    {
        /// <summary>
        /// vndbid
        /// </summary>
        public static class Id
		{
			public static string FilterName = StaffConstants.Filters.Id;
			public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Staff> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Staff>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Staff> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Staff> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Staff> GreaterThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Staff> GreaterThan(string value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Staff> LessThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Staff> LessThan(string value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// integer, alias identifier
        /// </summary>
        public static class Aid
        {
            public static string FilterName = StaffConstants.Filters.Aid;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Staff> CreateFilter(string operatorString, int value) =>
                FilterFactory.CreateFilter<Staff, int>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Staff> EqualTo(int value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Staff> NotEqualTo(int value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// String search.
        /// </summary>
        public static class Search
        {
            public static string FilterName = StaffConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Staff> CreateFilter(string operatorString, string value) =>
                 FilterFactory.CreateFilter<Staff>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Staff> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Staff> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Language.
        /// </summary>
        public static class Language
        {
            public static string FilterName = StaffConstants.Filters.Lang;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Staff> CreateFilter(string operatorString, LanguageEnum value) =>
                FilterFactory.CreateFilter<Staff, LanguageEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Staff> EqualTo(LanguageEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Staff> NotEqualTo(LanguageEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Gender.
        /// </summary>
        public static class Gender
        {
            public static string FilterName = StaffConstants.Filters.Gender;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Staff> CreateFilter(string operatorString, StaffGenderEnum value) =>
                FilterFactory.CreateFilter<Staff, StaffGenderEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Staff> EqualTo(StaffGenderEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Staff> NotEqualTo(StaffGenderEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// String, can either be "seiyuu" or one of the values from enums.staff_role in the schema JSON. If this filter is used when nested inside a visual novel filter, then this matches the role of the particular visual novel. Otherwise, this matches the role of any linked visual novel.
        /// </summary>
        public static class Role
        {
            public static string FilterName = StaffConstants.Filters.Role;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Staff> CreateFilter(string operatorString, StaffRoleEnum value) =>
                FilterFactory.CreateFilter<Staff, StaffRoleEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Staff> EqualTo(StaffRoleEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Staff> NotEqualTo(StaffRoleEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Match on external links, works similar to the exlink filter for releases.
        /// </summary>
        public static class ExtLink
        {
            public static string FilterName = StaffConstants.Filters.ExtLink;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Staff> CreateFilter(string operatorString, ExtLinksEnum value) =>
                FilterFactory.CreateFilter<Staff>(FilterName, CanBeNull, operatorString, value);

            private static SimpleFilterBase<Staff> CreateFilter(string operatorString, ExtLinksEnum value, string str) =>
                FilterFactory.CreateFilter<Staff>(FilterName, CanBeNull, operatorString, value, str);

            private static SimpleFilterBase<Staff> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Staff>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Staff> EqualTo(ExtLinksEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Staff> NotEqualTo(ExtLinksEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Staff> EqualTo(ExtLinksEnum value, string siteIdentifier) =>
                CreateFilter(Operators.Matching.IsEqual, value, siteIdentifier);

            public static SimpleFilterBase<Staff> NotEqualTo(ExtLinksEnum value, string siteIdentifier) =>
                CreateFilter(Operators.Matching.NotEqual, value, siteIdentifier);

            public static SimpleFilterBase<Staff> EqualTo(string extSiteUrl) =>
                CreateFilter(Operators.Matching.IsEqual, extSiteUrl);

            public static SimpleFilterBase<Staff> NotEqualTo(string extSiteUrl) =>
                CreateFilter(Operators.Matching.NotEqual, extSiteUrl);
        }

        /// <summary>
        /// Only accepts a single value, integer 1.
        /// </summary>
        public static class IsMain
        {
            public static string FilterName = StaffConstants.Filters.IsMain;
            private static SimpleFilterBase<Staff> CreateFilter(string operatorString) =>
                FilterFactory.CreateFilter<Staff, int>(FilterName, true, operatorString, 1);

            public static SimpleFilterBase<Staff> EqualTo() =>
                CreateFilter(Operators.Matching.IsEqual);

            public static SimpleFilterBase<Staff> NotEqualTo() =>
                CreateFilter(Operators.Matching.NotEqual);
        }

        public static ComplexFilterBase<Staff> And(params SimpleFilterBase<Staff>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.And, values);

        public static ComplexFilterBase<Staff> Or(params SimpleFilterBase<Staff>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.Or, values);
    }

}