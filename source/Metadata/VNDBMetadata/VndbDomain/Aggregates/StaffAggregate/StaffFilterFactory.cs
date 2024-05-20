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

namespace VNDBMetadata.VndbDomain.Aggregates.StaffAggregate
{
    public static class StaffFilterFactory
    {
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