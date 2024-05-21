using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.TagAggregate;
using VNDBMetadata.VndbDomain.Common;
using VNDBMetadata.VndbDomain.Common.Constants;
using VNDBMetadata.VndbDomain.Common.Enums;
using VNDBMetadata.VndbDomain.Common.Filters;
using VNDBMetadata.VndbDomain.Common.Interfaces;

namespace VNDBMetadata.VndbDomain.Aggregates.VnAggregate
{

    public static class VnFilterFactory
    {
        public static class Id
        {
            public static string FilterName = VnConstants.Filters.Id;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Vn>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Vn> GreaterThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Vn> GreaterThan(string value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Vn> LessThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Vn> LessThan(string value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        public static class Search
        {
            public static string FilterName = VnConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, string value) =>
                 FilterFactory.CreateFilter<Vn>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class Language
        {
            public static string FilterName = VnConstants.Filters.Language;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, LanguageEnum value) =>
                FilterFactory.CreateFilter<Vn, LanguageEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(LanguageEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(LanguageEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class OriginalLanguage
        {
            public static string FilterName = VnConstants.Filters.OriginalLanguage;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, LanguageEnum value) =>
                FilterFactory.CreateFilter<Vn, LanguageEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(LanguageEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(LanguageEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class Platform
        {
            public static string FilterName = VnConstants.Filters.Platform;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, PlatformEnum value) =>
                FilterFactory.CreateFilter<Vn, PlatformEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(PlatformEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(PlatformEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class Length
        {
            public static string FilterName = VnConstants.Filters.Length;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, VnLengthEnum value) =>
                FilterFactory.CreateFilter<Vn, VnLengthEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(VnLengthEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(VnLengthEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Release date.
        /// </summary>
        public static class Released
        {
            public static string FilterName = VnConstants.Filters.Released;
            public static bool CanBeNull { get; } = true;

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, uint? year, uint? month = null, uint? day = null)
            {
                var formattedDateString = GetFormattedDateString(year, month, day);
                return FilterFactory.CreateFilter<Vn>(FilterName, CanBeNull, operatorString, formattedDateString);
            }

            private static string GetFormattedDateString(uint? year, uint? month = null, uint? day = null)
            {
                if (!year.HasValue)
                {
                    return null;
                }
                
                Guard.Against.NotLessThan(year.Value, 1980, "Year must be 1980 or later.");
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

            public static SimpleFilterBase<Vn> EqualTo(uint? year, uint? month = null, uint? day = null) =>
                CreateFilter(Operators.Matching.IsEqual, year, month, day);

            public static SimpleFilterBase<Vn> NotEqualTo(uint? year, uint? month = null, uint? day = null) =>
                CreateFilter(Operators.Matching.NotEqual, year, month, day);

            public static SimpleFilterBase<Vn> GreaterThanOrEqual(uint? year, uint? month = null, uint? day = null) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, year, month, day);

            public static SimpleFilterBase<Vn> GreaterThan(uint? year, uint? month = null, uint? day = null) =>
                CreateFilter(Operators.Ordering.GreaterThan, year, month, day);

            public static SimpleFilterBase<Vn> LessThanOrEqual(uint? year, uint? month = null, uint? day = null) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, year, month, day);

            public static SimpleFilterBase<Vn> LessThan(uint? year, uint? month = null, uint? day = null) =>
                CreateFilter(Operators.Ordering.LessThan, year, month, day);
        }

        public static class Rating
        {
            public static string FilterName = VnConstants.Filters.Rating;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, int value) =>
                FilterFactory.CreateFilter<Vn, int>(FilterName, CanBeNull, operatorString, Guard.Against.NotInRange(value, 10, 100));

            public static SimpleFilterBase<Vn> EqualTo(int value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(int value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Vn> GreaterThanOrEqual(int value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Vn> GreaterThan(int value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Vn> LessThanOrEqual(int value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Vn> LessThan(int value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        public static class VoteCount
        {
            public static string FilterName = VnConstants.Filters.VoteCount;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, uint value) =>
                FilterFactory.CreateFilter<Vn, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(uint value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(uint value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Vn> GreaterThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Vn> GreaterThan(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Vn> LessThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Vn> LessThan(uint value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        public static class HasDescription
        {
            public static string FilterName = VnConstants.Filters.HasDescription;
            private static SimpleFilterBase<Vn> CreateFilter(string operatorString) =>
                 FilterFactory.CreateFilter<Vn, int>(FilterName, true, operatorString, 1);

            public static SimpleFilterBase<Vn> EqualTo() =>
                CreateFilter(Operators.Matching.IsEqual);

            public static SimpleFilterBase<Vn> NotEqualTo() =>
                CreateFilter(Operators.Matching.NotEqual);
        }

        public static class HasAnime
        {
            public static string FilterName = VnConstants.Filters.HasAnime;
            private static SimpleFilterBase<Vn> CreateFilter(string operatorString) =>
                 FilterFactory.CreateFilter<Vn, int>(FilterName, true, operatorString, 1);

            public static SimpleFilterBase<Vn> EqualTo() =>
                CreateFilter(Operators.Matching.IsEqual);

            public static SimpleFilterBase<Vn> NotEqualTo() =>
                CreateFilter(Operators.Matching.NotEqual);
        }

        public static class HasScreenshot
        {
            public static string FilterName = VnConstants.Filters.HasScreenshot;
            private static SimpleFilterBase<Vn> CreateFilter(string operatorString) =>
                 FilterFactory.CreateFilter<Vn, int>(FilterName, true, operatorString, 1);

            public static SimpleFilterBase<Vn> EqualTo() =>
                CreateFilter(Operators.Matching.IsEqual);

            public static SimpleFilterBase<Vn> NotEqualTo() =>
                CreateFilter(Operators.Matching.NotEqual);
        }

        public static class HasReview
        {
            public static string FilterName = VnConstants.Filters.HasReview;
            private static SimpleFilterBase<Vn> CreateFilter(string operatorString) =>
                 FilterFactory.CreateFilter<Vn, int>(FilterName, true, operatorString, 1);

            public static SimpleFilterBase<Vn> EqualTo() =>
                CreateFilter(Operators.Matching.IsEqual);

            public static SimpleFilterBase<Vn> NotEqualTo() =>
                CreateFilter(Operators.Matching.NotEqual);
        }

        public static class DevelopmentStatus
        {
            public static string FilterName = VnConstants.Filters.DevStatus;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, VnDevelopmentStatusEnum value) =>
                FilterFactory.CreateFilter<Vn, VnDevelopmentStatusEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(VnDevelopmentStatusEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(VnDevelopmentStatusEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class Trait
        {
            public static string FilterName = VnConstants.Filters.Tag;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Vn>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(string id) =>
                CreateFilter(Operators.Matching.IsEqual, id);

            public static SimpleFilterBase<Vn> NotEqualTo(string id) =>
                CreateFilter(Operators.Matching.NotEqual, id);


            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, uint id, SpoilerLevelEnum maxSpoilerLevel, double minimumTagLevel) =>
                FilterFactory.CreateFilter<Vn>(FilterName, CanBeNull, operatorString, id, maxSpoilerLevel, Guard.Against.NotInRange(minimumTagLevel, 0, 3) );

            public static SimpleFilterBase<Vn> EqualTo(uint id, SpoilerLevelEnum maxSpoilerLevel, double minimumTagLevel) =>
                CreateFilter(Operators.Matching.IsEqual, id, maxSpoilerLevel, minimumTagLevel);

            public static SimpleFilterBase<Vn> NotEqualTo(uint id, SpoilerLevelEnum maxSpoilerLevel, double minimumTagLevel) =>
                CreateFilter(Operators.Matching.NotEqual, id, maxSpoilerLevel, minimumTagLevel);
        }

        public static class DirectTag
        {
            public static string FilterName = VnConstants.Filters.DirectTag;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Vn>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(string id) =>
                CreateFilter(Operators.Matching.IsEqual, id);

            public static SimpleFilterBase<Vn> NotEqualTo(string id) =>
                CreateFilter(Operators.Matching.NotEqual, id);


            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, uint id, SpoilerLevelEnum maxSpoilerLevel, double minimumTagLevel) =>
                FilterFactory.CreateFilter<Vn>(FilterName, CanBeNull, operatorString, id, maxSpoilerLevel, Guard.Against.NotInRange(minimumTagLevel, 0, 3));

            public static SimpleFilterBase<Vn> EqualTo(uint id, SpoilerLevelEnum maxSpoilerLevel, double minimumTagLevel) =>
                CreateFilter(Operators.Matching.IsEqual, id, maxSpoilerLevel, minimumTagLevel);

            public static SimpleFilterBase<Vn> NotEqualTo(uint id, SpoilerLevelEnum maxSpoilerLevel, double minimumTagLevel) =>
                CreateFilter(Operators.Matching.NotEqual, id, maxSpoilerLevel, minimumTagLevel);
        }

        public static class AnimeId
        {
            public static string FilterName = VnConstants.Filters.AnimeId;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, uint value) =>
                 FilterFactory.CreateFilter<Vn, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(uint id) =>
                CreateFilter(Operators.Matching.IsEqual, id);

            public static SimpleFilterBase<Vn> NotEqualTo(uint id) =>
                CreateFilter(Operators.Matching.NotEqual, id);
        }

        public static class Label
        {
            public static string FilterName = VnConstants.Filters.Label;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, string strValue, uint value) =>
                 FilterFactory.CreateFilter<Vn>(FilterName, CanBeNull, operatorString, strValue, value);

            public static SimpleFilterBase<Vn> EqualTo(string userId, uint labelId) =>
                CreateFilter(Operators.Matching.IsEqual, userId, labelId);

            public static SimpleFilterBase<Vn> NotEqualTo(string userId, uint labelId) =>
                CreateFilter(Operators.Matching.NotEqual, userId, labelId);
        }

        public static class Release
        {
            public static string FilterName = VnConstants.Filters.Release;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, SimpleFilterBase<ReleaseAggregate.Release> value) =>
                FilterFactory.CreateFilter<Vn, SimpleFilterBase<ReleaseAggregate.Release>>(FilterName, CanBeNull, operatorString, value);

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, ComplexFilterBase<ReleaseAggregate.Release> value) =>
                FilterFactory.CreateFilter<Vn, ComplexFilterBase<ReleaseAggregate.Release>>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(SimpleFilterBase<ReleaseAggregate.Release> value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(SimpleFilterBase<ReleaseAggregate.Release> value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Vn> EqualTo(ComplexFilterBase<ReleaseAggregate.Release> value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(ComplexFilterBase<ReleaseAggregate.Release> value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class Character
        {
            public static string FilterName = VnConstants.Filters.Character;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, SimpleFilterBase<CharacterAggregate.Character> value) =>
                FilterFactory.CreateFilter<Vn, SimpleFilterBase<CharacterAggregate.Character>>(FilterName, CanBeNull, operatorString, value);

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, ComplexFilterBase<CharacterAggregate.Character> value) =>
                FilterFactory.CreateFilter<Vn, ComplexFilterBase<CharacterAggregate.Character>>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(SimpleFilterBase<CharacterAggregate.Character> value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(SimpleFilterBase<CharacterAggregate.Character> value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Vn> EqualTo(ComplexFilterBase<CharacterAggregate.Character> value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(ComplexFilterBase<CharacterAggregate.Character> value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class Staff
        {
            public static string FilterName = VnConstants.Filters.Staff;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, SimpleFilterBase<StaffAggregate.Staff> value) =>
                FilterFactory.CreateFilter<Vn, SimpleFilterBase<StaffAggregate.Staff>>(FilterName, CanBeNull, operatorString, value);

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, ComplexFilterBase<StaffAggregate.Staff> value) =>
                FilterFactory.CreateFilter<Vn, ComplexFilterBase<StaffAggregate.Staff>>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(SimpleFilterBase<StaffAggregate.Staff> value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(SimpleFilterBase<StaffAggregate.Staff> value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Vn> EqualTo(ComplexFilterBase<StaffAggregate.Staff> value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(ComplexFilterBase<StaffAggregate.Staff> value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class Developer
        {
            public static string FilterName = VnConstants.Filters.Developer;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, SimpleFilterBase<ProducerAggregate.Producer> value) =>
                FilterFactory.CreateFilter<Vn, SimpleFilterBase<ProducerAggregate.Producer>>(FilterName, CanBeNull, operatorString, value);

            private static SimpleFilterBase<Vn> CreateFilter(string operatorString, ComplexFilterBase<ProducerAggregate.Producer> value) =>
                FilterFactory.CreateFilter<Vn, ComplexFilterBase<ProducerAggregate.Producer>>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Vn> EqualTo(SimpleFilterBase<ProducerAggregate.Producer> value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(SimpleFilterBase<ProducerAggregate.Producer> value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Vn> EqualTo(ComplexFilterBase<ProducerAggregate.Producer> value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Vn> NotEqualTo(ComplexFilterBase<ProducerAggregate.Producer> value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static ComplexFilterBase<Vn> And(params SimpleFilterBase<Vn>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.And, values);

        public static ComplexFilterBase<Vn> Or(params SimpleFilterBase<Vn>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.Or, values);
    }

}