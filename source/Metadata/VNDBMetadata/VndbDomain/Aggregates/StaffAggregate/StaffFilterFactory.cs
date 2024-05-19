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

namespace VNDBMetadata.VndbDomain.Aggregates.StaffAggregate
{
    public class StaffFilter : SimpleFilterBase
    {
        internal StaffFilter(string filterName, string filterOperator, object value) : base(filterName, filterOperator, value)
        {

        }

        internal StaffFilter(string filterName, string filterOperator, params object[] values) : base(filterName, filterOperator, values)
        {

        }
    }

    public class StaffComplexFilter : ComplexFilterBase
    {
        internal StaffComplexFilter(string filterOperator, params IFilter[] value) : base(filterOperator, value)
        {

        }
    }

    public static class StaffFilterFactory
    {
        public static class Id
		{
			public static string FilterName = StaffConstants.Filters.Id;
			public static bool CanBeNull { get; } = false;
			public static StaffFilter EqualTo(string value) =>  new StaffFilter(
				FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null(value) : value);
			public static StaffFilter NotEqualTo(string value) => new StaffFilter(
				FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null(value) : value);
			public static StaffFilter GreaterThanOrEqual(string value) => new StaffFilter(
				FilterName, Operators.Ordering.GreaterThanOrEqual, CanBeNull ? Guard.Against.Null(value) : value);
			public static StaffFilter GreaterThan(string value) => new StaffFilter(
				FilterName, Operators.Ordering.GreaterThan, CanBeNull ? Guard.Against.Null(value) : value);
			public static StaffFilter LessThanOrEqual(string value) => new StaffFilter(
				FilterName, Operators.Ordering.LessThanOrEqual, CanBeNull ? Guard.Against.Null(value) : value);
			public static StaffFilter LessThan(string value) => new StaffFilter(
				FilterName, Operators.Ordering.LessThan, CanBeNull ? Guard.Against.Null(value) : value);
		}

        public static class Aid
        {
            public static string FilterName = StaffConstants.Filters.Aid;
            public static bool CanBeNull { get; } = false;
            public static StaffFilter EqualTo(int value) => new StaffFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<int>(value) : value);
            public static StaffFilter NotEqualTo(int value) => new StaffFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<int>(value) : value);
        }

        public static class Search
        {
            public static string FilterName = StaffConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            public static StaffFilter EqualTo(string value) => new StaffFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
            public static StaffFilter NotEqualTo(string value) => new StaffFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
        }

        public static class Language
        {
            public static string FilterName = StaffConstants.Filters.Lang;
            public static bool CanBeNull { get; } = false;
            public static StaffFilter EqualTo(LanguageEnum value) => new StaffFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<LanguageEnum>(value) : value);
            public static StaffFilter NotEqualTo(LanguageEnum value) => new StaffFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<LanguageEnum>(value) : value);
        }

        public static class Gender
        {
            public static string FilterName = StaffConstants.Filters.Gender;
            public static bool CanBeNull { get; } = false;
            public static StaffFilter EqualTo(StaffGenderEnum value) => new StaffFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<StaffGenderEnum>(value) : value);
            public static StaffFilter NotEqualTo(StaffGenderEnum value) => new StaffFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<StaffGenderEnum>(value) : value);
        }

        public static class Role
        {
            public static string FilterName = StaffConstants.Filters.Role;
            public static bool CanBeNull { get; } = false;
            public static StaffFilter EqualTo(StaffRoleEnum value) => new StaffFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<StaffRoleEnum>(value) : value);
            public static StaffFilter NotEqualTo(StaffRoleEnum value) => new StaffFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<StaffRoleEnum>(value) : value);
        }

        public static class ExtLink
        {
            public static string FilterName = StaffConstants.Filters.ExtLink;
            public static bool CanBeNull { get; } = false;
            public static StaffFilter EqualTo(ExtLinksEnum value) => new StaffFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<ExtLinksEnum>(value) : value);
            public static StaffFilter NotEqualTo(ExtLinksEnum value) => new StaffFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<ExtLinksEnum>(value) : value);

            public static StaffFilter EqualTo(ExtLinksEnum value, string siteIdentifier) => new StaffFilter(
                FilterName, Operators.Matching.IsEqual,
                CanBeNull ? Guard.Against.Null<ExtLinksEnum>(value) : value,
                CanBeNull ? Guard.Against.NullOrWhiteSpace(siteIdentifier) : siteIdentifier);
            public static StaffFilter NotEqualTo(ExtLinksEnum value, string siteIdentifier) => new StaffFilter(
                FilterName, Operators.Matching.NotEqual,
                CanBeNull ? Guard.Against.Null<ExtLinksEnum>(value) : value,
                CanBeNull ? Guard.Against.NullOrWhiteSpace(siteIdentifier) : siteIdentifier);

            public static StaffFilter EqualTo(string extSiteUrl) => new StaffFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(extSiteUrl) : extSiteUrl);
            public static StaffFilter NotEqualTo(string extSiteUrl) => new StaffFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(extSiteUrl) : extSiteUrl);
        }

        public static class IsMain
        {
            public static string FilterName = StaffConstants.Filters.IsMain;
            public static StaffFilter True() => new StaffFilter(
                FilterName, Operators.Matching.IsEqual, 1);
            public static StaffFilter False() => new StaffFilter(
                FilterName, Operators.Matching.NotEqual, 1);
        }

        public static StaffComplexFilter And(params StaffFilter[] value) => new StaffComplexFilter(
            Operators.Predicates.And, Guard.Against.Null(value));

        public static StaffComplexFilter Or(params StaffFilter[] value) => new StaffComplexFilter(
            Operators.Predicates.Or, Guard.Against.Null(value));
    }

}