using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.ProducerAggregate;
using VNDBMetadata.VndbDomain.Common;
using VNDBMetadata.VndbDomain.Common.Constants;
using VNDBMetadata.VndbDomain.Common.Enums;
using VNDBMetadata.VndbDomain.Common.Filters;
using VNDBMetadata.VndbDomain.Common.Interfaces;

namespace VNDBMetadata.VndbDomain.Aggregates.ReleaseAggregate
{
    public static class ReleaseFilterFactory
    {
        public static class Id
		{
			public static string FilterName = ReleaseConstants.Filters.Id;
			public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Release> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Release>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Release> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Release> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Release> GreaterThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Release> GreaterThan(string value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Release> LessThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Release> LessThan(string value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        public static class Search
        {
            public static string FilterName = ReleaseConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Release> CreateFilter(string operatorString, string value) =>
                 FilterFactory.CreateFilter<Release>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Release> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Release> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class Language
        {
            public static string FilterName = ReleaseConstants.Filters.Lang;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Release> CreateFilter(string operatorString, LanguageEnum value) =>
                FilterFactory.CreateFilter<Release, LanguageEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Release> EqualTo(LanguageEnum value) => CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Release> NotEqualTo(LanguageEnum value) => CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Match on available platforms.
        /// </summary>
        public static class Platform
        {
            public static string FilterName = ReleaseConstants.Filters.Platform;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Release> CreateFilter(string operatorString, PlatformEnum value) =>
                FilterFactory.CreateFilter<Release, PlatformEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Release> EqualTo(PlatformEnum value) => CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Release> NotEqualTo(PlatformEnum value) => CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Release date.
        /// </summary>
        public static class Released
        {
            public static string FilterName = ReleaseConstants.Filters.Released;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Release> CreateFilter(string operatorString, uint year, uint? month = null, uint? day = null)
            {
                if (!CanBeNull)
                {
                    Guard.Against.Null(year);
                }

                var formattedDateString = GetFormattedDateString(year, month, day);
                return FilterFactory.CreateFilter<Release>(FilterName, CanBeNull, operatorString, formattedDateString);
            }

            private static string GetFormattedDateString(uint year, uint? month = null, uint? day = null)
            {
                Guard.Against.NotLessThan(year, 1980, "Year must be 1980 or later.");
                if (month.HasValue)
                {
                    Guard.Against.NotInRange(month.Value, 1, 12, "Month must be between 1 and 12.");
                    if (day.HasValue)
                    {
                        Guard.Against.NotInRange(day.Value, 1, 31, "Day must be between 1 and 31.");
                        return $"{year:D4}-{month.Value:D2}-{day.Value:D2}";
                    }

                    return $"{year:D4}-{month.Value:D2}";
                }

                return $"{year:D4}";
            }

            public static SimpleFilterBase<Release> EqualTo(uint year, uint? month = null, uint? day = null) =>
                CreateFilter(Operators.Matching.IsEqual, year, month, day);

            public static SimpleFilterBase<Release> NotEqualTo(uint year, uint? month = null, uint? day = null) =>
                CreateFilter(Operators.Matching.NotEqual, year, month, day);

            public static SimpleFilterBase<Release> GreaterThanOrEqual(uint year, uint? month = null, uint? day = null) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, year, month, day);

            public static SimpleFilterBase<Release> GreaterThan(uint year, uint? month = null, uint? day = null) =>
                CreateFilter(Operators.Ordering.GreaterThan, year, month, day);

            public static SimpleFilterBase<Release> LessThanOrEqual(uint year, uint? month = null, uint? day = null) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, year, month, day);

            public static SimpleFilterBase<Release> LessThan(uint year, uint? month = null, uint? day = null) =>
                CreateFilter(Operators.Ordering.LessThan, year, month, day);
        }

        /// <summary>
        /// Match on the image resolution, in pixels.
        /// </summary>
        public static class Resolution
        {
            public static string FilterName = ReleaseConstants.Filters.Resolution;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Release> CreateFilter(string operatorString, params uint[] values) =>
                FilterFactory.CreateFilter<Release>(FilterName, CanBeNull, operatorString, values);

            public static SimpleFilterBase<Release> EqualTo(uint width, uint height) =>
                CreateFilter(Operators.Matching.IsEqual, width, height);

            public static SimpleFilterBase<Release> NotEqualTo(uint width, uint height) =>
                CreateFilter(Operators.Matching.NotEqual, width, height);

            public static SimpleFilterBase<Release> GreaterThanOrEqual(uint width, uint height) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, width, height);

            public static SimpleFilterBase<Release> GreaterThan(uint width, uint height) =>
                CreateFilter(Operators.Ordering.GreaterThan, width, height);

            public static SimpleFilterBase<Release> LessThanOrEqual(uint width, uint height) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, width, height);

            public static SimpleFilterBase<Release> LessThan(uint width, uint height) =>
                CreateFilter(Operators.Ordering.LessThan, width, height);
        }

        /// <summary>
        /// Same as the resolution filter, but additionally requires that the aspect ratio matches that of the given resolution.
        /// </summary>
        //public static class ResolutionAspect
        //{
        //    public static string FilterName = ReleaseConstants.Filters.ResolutionAspect;
        //    public static bool CanBeNull { get; } = false;

        //    public static SimpleFilterBase<Release> EqualTo(int[] value) => new SimpleFilterBase<Release>(
        //        FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null(value) : value);

        //    public static SimpleFilterBase<Release> NotEqualTo(int[] value) => new SimpleFilterBase<Release>(
        //        FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null(value) : value);

        //    public static SimpleFilterBase<Release> GreaterThanOrEqual(int[] value) => new SimpleFilterBase<Release>(
        //        FilterName, Operators.Ordering.GreaterThanOrEqual, CanBeNull ? Guard.Against.Null(value) : value);

        //    public static SimpleFilterBase<Release> GreaterThan(int[] value) => new SimpleFilterBase<Release>(
        //        FilterName, Operators.Ordering.GreaterThan, CanBeNull ? Guard.Against.Null(value) : value);

        //    public static SimpleFilterBase<Release> LessThanOrEqual(int[] value) => new SimpleFilterBase<Release>(
        //        FilterName, Operators.Ordering.LessThanOrEqual, CanBeNull ? Guard.Against.Null(value) : value);

        //    public static SimpleFilterBase<Release> LessThan(int[] value) => new SimpleFilterBase<Release>(
        //        FilterName, Operators.Ordering.LessThan, CanBeNull ? Guard.Against.Null(value) : value);
        //}

        /// <summary>
        /// Integer (0-18), age rating.
        /// </summary>
        public static class MinAge
        {
            public static string FilterName = ReleaseConstants.Filters.MinAge;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Release> CreateFilter(string operatorString, uint value)
            {
                if (!CanBeNull)
                {
                    Guard.Against.Null(value);
                }

                Guard.Against.NotInRange(value, 0, 18, "Age must be in the 0-18 range.");
                return FilterFactory.CreateFilter<Release, uint>(FilterName, true, operatorString, value);
            }

            public static SimpleFilterBase<Release> EqualTo(uint value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Release> NotEqualTo(uint value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Release> GreaterThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Release> GreaterThan(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Release> LessThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Release> LessThan(uint value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// String.
        /// </summary>
        public static class Medium
        {
            public static string FilterName = ReleaseConstants.Filters.Medium;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Release> CreateFilter(string operatorString, string value) =>
                 FilterFactory.CreateFilter<Release>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Release> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Release> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Integer, see voiced field.
        /// </summary>
        public static class Voiced
        {
            public static string FilterName = ReleaseConstants.Filters.Voiced;
            public static bool CanBeNull { get; } = true;

            private static SimpleFilterBase<Release> CreateFilter(string operatorString, VoicedEnum value) =>
                FilterFactory.CreateFilter<Release, VoicedEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Release> EqualTo(VoicedEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Release> NotEqualTo(VoicedEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// String.
        /// </summary>
        public static class Engine
        {
            public static string FilterName = ReleaseConstants.Filters.Engine;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Release> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Release>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Release> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Release> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// String, see vns.rtype field.
        /// </summary>
        public static class ReleaseType
        {
            public static string FilterName = ReleaseConstants.Filters.RType;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Release> CreateFilter(string operatorString, ReleaseTypeEnum value) =>
                FilterFactory.CreateFilter<Release, ReleaseTypeEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Release> EqualTo(ReleaseTypeEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Release> NotEqualTo(ReleaseTypeEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class ExtLink
        {
            public static string FilterName = ReleaseConstants.Filters.ExtLink;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Release> CreateFilter(string operatorString, ExtLinksEnum value) =>
                FilterFactory.CreateFilter<Release>(FilterName, CanBeNull, operatorString, value);

            private static SimpleFilterBase<Release> CreateFilter(string operatorString, ExtLinksEnum value, string str) =>
                FilterFactory.CreateFilter<Release>(FilterName, CanBeNull, operatorString, value, str);

            private static SimpleFilterBase<Release> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Release>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Release> EqualTo(ExtLinksEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Release> NotEqualTo(ExtLinksEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Release> EqualTo(ExtLinksEnum value, string siteIdentifier) =>
                CreateFilter(Operators.Matching.IsEqual, value, siteIdentifier);

            public static SimpleFilterBase<Release> NotEqualTo(ExtLinksEnum value, string siteIdentifier) =>
                CreateFilter(Operators.Matching.NotEqual, value, siteIdentifier);

            public static SimpleFilterBase<Release> EqualTo(string extSiteUrl) =>
                CreateFilter(Operators.Matching.IsEqual, extSiteUrl);

            public static SimpleFilterBase<Release> NotEqualTo(string extSiteUrl) =>
                CreateFilter(Operators.Matching.NotEqual, extSiteUrl);
        }

        /// <summary>
        /// Integer, only accepts the value 1.
        /// </summary>
        public static class Patch
        {
            public static string FilterName = ReleaseConstants.Filters.Patch;

            private static SimpleFilterBase<Release> CreateFilter(string operatorString) =>
                FilterFactory.CreateFilter<Release, int>(FilterName, true, operatorString, 1);

            public static SimpleFilterBase<Release> EqualTo() =>
                CreateFilter(Operators.Matching.IsEqual);

            public static SimpleFilterBase<Release> NotEqualTo() =>
                CreateFilter(Operators.Matching.NotEqual);
        }

        /// <summary>
        /// See patch.
        /// </summary>
        public static class Freeware
        {
            public static string FilterName = ReleaseConstants.Filters.Freeware;
            private static SimpleFilterBase<Release> CreateFilter(string operatorString) =>
                FilterFactory.CreateFilter<Release, int>(FilterName, true, operatorString, 1);

            public static SimpleFilterBase<Release> EqualTo() =>
                CreateFilter(Operators.Matching.IsEqual);

            public static SimpleFilterBase<Release> NotEqualTo() =>
                CreateFilter(Operators.Matching.NotEqual);
        }

        /// <summary>
        /// See patch.
        /// </summary>
        public static class Uncensored
        {
            public static string FilterName = ReleaseConstants.Filters.Uncensored;
            private static SimpleFilterBase<Release> CreateFilter(string operatorString) =>
                FilterFactory.CreateFilter<Release, int>(FilterName, true, operatorString, 1);

            public static SimpleFilterBase<Release> EqualTo() =>
                CreateFilter(Operators.Matching.IsEqual);

            public static SimpleFilterBase<Release> NotEqualTo() =>
                CreateFilter(Operators.Matching.NotEqual);
        }

        /// <summary>
        /// See patch.
        /// </summary>
        public static class Official
        {
            public static string FilterName = ReleaseConstants.Filters.Official;
            private static SimpleFilterBase<Release> CreateFilter(string operatorString) =>
                FilterFactory.CreateFilter<Release, int>(FilterName, true, operatorString, 1);

            public static SimpleFilterBase<Release> EqualTo() =>
                CreateFilter(Operators.Matching.IsEqual);

            public static SimpleFilterBase<Release> NotEqualTo() =>
                CreateFilter(Operators.Matching.NotEqual);
        }

        /// <summary>
        /// See patch.
        /// </summary>
        public static class HasEro
        {
            public static string FilterName = ReleaseConstants.Filters.HasEro;
            private static SimpleFilterBase<Release> CreateFilter(string operatorString) =>
                FilterFactory.CreateFilter<Release, int>(FilterName, true, operatorString, 1);

            public static SimpleFilterBase<Release> EqualTo() =>
                CreateFilter(Operators.Matching.IsEqual);

            public static SimpleFilterBase<Release> NotEqualTo() =>
                CreateFilter(Operators.Matching.NotEqual);
        }

        /// <summary>
        /// Match releases that are linked to at least one visual novel matching the given visual novel filters.
        /// </summary>
        //public static class Vn
        //{
        //    public static string FilterName = ReleaseConstants.Filters.Vn;
        //    public static bool CanBeNull { get; } = false;

        //    public static SimpleFilterBase<Release> EqualTo(string value) => new SimpleFilterBase<Release>(
        //        FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);

        //    public static SimpleFilterBase<Release> NotEqualTo(string value) => new SimpleFilterBase<Release>(
        //        FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
        //}

        /// <summary>
        /// Match releases that have at least one producer matching the given producer filters.
        /// </summary>
        public static class Producer
        {
            public static string FilterName = ReleaseConstants.Filters.Producer;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Release> CreateFilter(string operatorString, SimpleFilterBase<ProducerAggregate.Producer> value) =>
                FilterFactory.CreateFilter<Release, SimpleFilterBase<ProducerAggregate.Producer>>(FilterName, CanBeNull, operatorString, value);

            private static SimpleFilterBase<Release> CreateFilter(string operatorString, ComplexFilterBase<ProducerAggregate.Producer> value) =>
                FilterFactory.CreateFilter<Release, ComplexFilterBase<ProducerAggregate.Producer>>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Release> EqualTo(SimpleFilterBase<ProducerAggregate.Producer> value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Release> NotEqualTo(SimpleFilterBase<ProducerAggregate.Producer> value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Release> EqualTo(ComplexFilterBase<ProducerAggregate.Producer> value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Release> NotEqualTo(ComplexFilterBase<ProducerAggregate.Producer> value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static ComplexFilterBase<Release> And(params SimpleFilterBase<Release>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.And, values);

        public static ComplexFilterBase<Release> Or(params SimpleFilterBase<Release>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.Or, values);
    }

}