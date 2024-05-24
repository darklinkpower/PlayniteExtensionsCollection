using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.TagAggregate;
using VndbApi.Domain.SharedKernel;
using VndbApi.Domain.VisualNovelAggregate;
using VndbApi.Infrastructure.SharedKernel.Filters;
using VndbApi.Infrastructure.SharedKernel.Requests;
using PluginsCommon;
using VndbApi.Domain.ProducerAggregate;

namespace VndbApi.Infrastructure.VisualNovelAggregate
{
    public static class VisualNovelFilterFactory
    {
        /// <summary>
        /// vndbid
        /// </summary>
        public static class Id
        {
            public static string FilterName = VisualNovelConstants.Filters.Id;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<VisualNovel>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<VisualNovel> GreaterThanOrEqual(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<VisualNovel> GreaterThan(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<VisualNovel> LessThanOrEqual(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<VisualNovel> LessThan(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// String search, matches on the VN titles, aliases and release titles. The search algorithm is the same as used on the site.
        /// </summary>
        public static class Search
        {
            public static string FilterName = VisualNovelConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, string value) =>
                 FilterFactory.CreateFilter<VisualNovel>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Language availability.
        /// </summary>
        public static class Language
        {
            public static string FilterName = VisualNovelConstants.Filters.Language;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, LanguageEnum value) =>
                FilterFactory.CreateFilter<VisualNovel, LanguageEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(LanguageEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(LanguageEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Original language.
        /// </summary>
        public static class OriginalLanguage
        {
            public static string FilterName = VisualNovelConstants.Filters.OriginalLanguage;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, LanguageEnum value) =>
                FilterFactory.CreateFilter<VisualNovel, LanguageEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(LanguageEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(LanguageEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Platform availability.
        /// </summary>
        public static class Platform
        {
            public static string FilterName = VisualNovelConstants.Filters.Platform;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, PlatformEnum value) =>
                FilterFactory.CreateFilter<VisualNovel, PlatformEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(PlatformEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(PlatformEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Play time estimate, between Very short and Very long).
        /// This filter uses the length votes average when available but falls back to the entries’ length field when there are no votes.
        /// </summary>
        public static class Length
        {
            public static string FilterName = VisualNovelConstants.Filters.Length;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, VnLengthEnum value) =>
                FilterFactory.CreateFilter<VisualNovel, VnLengthEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(VnLengthEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(VnLengthEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Release date.
        /// </summary>
        public static class ReleaseDate
        {
            public static string FilterName = VisualNovelConstants.Filters.Released;
            public static bool CanBeNull { get; } = true;

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, uint? year, uint? month = null, uint? day = null)
            {
                var formattedDateString = GetFormattedDateString(year, month, day);
                return FilterFactory.CreateFilter<VisualNovel>(FilterName, CanBeNull, operatorString, formattedDateString);
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

            public static SimpleFilterBase<VisualNovel> EqualTo(uint? year, uint? month = null, uint? day = null) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, year, month, day);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(uint? year, uint? month = null, uint? day = null) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, year, month, day);

            public static SimpleFilterBase<VisualNovel> GreaterThanOrEqual(uint? year, uint? month = null, uint? day = null) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThanOrEqual, year, month, day);

            public static SimpleFilterBase<VisualNovel> GreaterThan(uint? year, uint? month = null, uint? day = null) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThan, year, month, day);

            public static SimpleFilterBase<VisualNovel> LessThanOrEqual(uint? year, uint? month = null, uint? day = null) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThanOrEqual, year, month, day);

            public static SimpleFilterBase<VisualNovel> LessThan(uint? year, uint? month = null, uint? day = null) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThan, year, month, day);
        }

        /// <summary>
        /// Bayesian rating, integer between 10 and 100.
        /// </summary>
        public static class Rating
        {
            public static string FilterName = VisualNovelConstants.Filters.Rating;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, int value) =>
                FilterFactory.CreateFilter<VisualNovel, int>(FilterName, CanBeNull, operatorString, Guard.Against.NotInRange(value, 10, 100));

            public static SimpleFilterBase<VisualNovel> EqualTo(int value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(int value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<VisualNovel> GreaterThanOrEqual(int value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<VisualNovel> GreaterThan(int value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<VisualNovel> LessThanOrEqual(int value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<VisualNovel> LessThan(int value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// Integer, number of votes.
        /// </summary>
        public static class VoteCount
        {
            public static string FilterName = VisualNovelConstants.Filters.VoteCount;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, uint value) =>
                FilterFactory.CreateFilter<VisualNovel, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(uint value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(uint value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<VisualNovel> GreaterThanOrEqual(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<VisualNovel> GreaterThan(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<VisualNovel> LessThanOrEqual(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<VisualNovel> LessThan(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// If visual novel has description available.
        /// </summary>
        public static class HasDescription
        {
            public static string FilterName = VisualNovelConstants.Filters.HasDescription;
            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString) =>
                 FilterFactory.CreateFilter<VisualNovel, int>(FilterName, true, operatorString, 1);

            public static SimpleFilterBase<VisualNovel> EqualTo() =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual);

            public static SimpleFilterBase<VisualNovel> NotEqualTo() =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual);
        }

        /// <summary>
        /// If visual novel has anime available.
        /// </summary>
        public static class HasAnime
        {
            public static string FilterName = VisualNovelConstants.Filters.HasAnime;
            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString) =>
                 FilterFactory.CreateFilter<VisualNovel, int>(FilterName, true, operatorString, 1);

            public static SimpleFilterBase<VisualNovel> EqualTo() =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual);

            public static SimpleFilterBase<VisualNovel> NotEqualTo() =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual);
        }

        /// <summary>
        /// If visual novel has screenshots available.
        /// </summary>
        public static class HasScreenshot
        {
            public static string FilterName = VisualNovelConstants.Filters.HasScreenshot;
            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString) =>
                 FilterFactory.CreateFilter<VisualNovel, int>(FilterName, true, operatorString, 1);

            public static SimpleFilterBase<VisualNovel> EqualTo() =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual);

            public static SimpleFilterBase<VisualNovel> NotEqualTo() =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual);
        }

        /// <summary>
        /// If visual novel has reviews available.
        /// </summary>
        public static class HasReview
        {
            public static string FilterName = VisualNovelConstants.Filters.HasReview;
            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString) =>
                 FilterFactory.CreateFilter<VisualNovel, int>(FilterName, true, operatorString, 1);

            public static SimpleFilterBase<VisualNovel> EqualTo() =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual);

            public static SimpleFilterBase<VisualNovel> NotEqualTo() =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual);
        }

        /// <summary>
        /// Development status.
        /// </summary>
        public static class DevelopmentStatus
        {
            public static string FilterName = VisualNovelConstants.Filters.DevStatus;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, VisualNovelDevelopmentStatusEnum value) =>
                FilterFactory.CreateFilter<VisualNovel, VisualNovelDevelopmentStatusEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(VisualNovelDevelopmentStatusEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(VisualNovelDevelopmentStatusEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Tags applied directly to this VN, does not match parent tags.
        /// The tag and dtag filters accept either a plain tag ID or a three-element array containing the tag ID, maximum spoiler level (0, 1 or 2) and minimum tag level (number between 0 and 3, inclusive), for example ["tag","=",["g505",2,1.2]] matches all visual novels that have a Donkan Protagonist with a vote of at least 1.2 at any spoiler level.
        /// If only an ID is given, 0 is assumed for both the spoiler and tag levels.
        /// For example, ["tag","=","g505"] is equivalent to ["tag","=",["g505",0,0]].
        /// </summary>
        public static class Tag
        {
            public static string FilterName = VisualNovelConstants.Filters.Tag;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<VisualNovel>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(string id) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, id);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(string id) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, id);


            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, uint id, SpoilerLevelEnum maxSpoilerLevel, double minimumTagLevel) =>
                FilterFactory.CreateFilter<VisualNovel>(FilterName, CanBeNull, operatorString, id, maxSpoilerLevel, Guard.Against.NotInRange(minimumTagLevel, 0, 3));

            public static SimpleFilterBase<VisualNovel> EqualTo(uint id, SpoilerLevelEnum maxSpoilerLevel, double minimumTagLevel) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, id, maxSpoilerLevel, minimumTagLevel);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(uint id, SpoilerLevelEnum maxSpoilerLevel, double minimumTagLevel) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, id, maxSpoilerLevel, minimumTagLevel);
        }

        /// <summary>
        /// Tags applied directly to this VN, does not match parent tags.
        /// The tag and dtag filters accept either a plain tag ID or a three-element array containing the tag ID, maximum spoiler level (0, 1 or 2) and minimum tag level (number between 0 and 3, inclusive), for example ["tag","=",["g505",2,1.2]] matches all visual novels that have a Donkan Protagonist with a vote of at least 1.2 at any spoiler level.
        /// If only an ID is given, 0 is assumed for both the spoiler and tag levels.
        /// For example, ["tag","=","g505"] is equivalent to ["tag","=",["g505",0,0]].
        /// </summary>
        public static class DirectTag
        {
            public static string FilterName = VisualNovelConstants.Filters.DirectTag;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<VisualNovel>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(string id) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, id);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(string id) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, id);


            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, uint id, SpoilerLevelEnum maxSpoilerLevel, double minimumTagLevel) =>
                FilterFactory.CreateFilter<VisualNovel>(FilterName, CanBeNull, operatorString, id, maxSpoilerLevel, Guard.Against.NotInRange(minimumTagLevel, 0, 3));

            public static SimpleFilterBase<VisualNovel> EqualTo(uint id, SpoilerLevelEnum maxSpoilerLevel, double minimumTagLevel) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, id, maxSpoilerLevel, minimumTagLevel);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(uint id, SpoilerLevelEnum maxSpoilerLevel, double minimumTagLevel) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, id, maxSpoilerLevel, minimumTagLevel);
        }

        /// <summary>
        /// Integer, AniDB anime identifier.
        /// </summary>
        public static class AnimeId
        {
            public static string FilterName = VisualNovelConstants.Filters.AnimeId;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, uint value) =>
                 FilterFactory.CreateFilter<VisualNovel, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(uint id) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, id);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(uint id) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, id);
        }

        /// <summary>
        /// User labels applied to this VN. Accepts a two-element array containing a user ID and label ID.
        /// When authenticated or if the "user" request parameter has been set, then it also accepts just a label ID.
        /// </summary>
        public static class Label
        {
            public static string FilterName = VisualNovelConstants.Filters.Label;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, string strValue, uint value) =>
                 FilterFactory.CreateFilter<VisualNovel>(FilterName, CanBeNull, operatorString, strValue, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(string userId, uint labelId) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, userId, labelId);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(string userId, uint labelId) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, userId, labelId);

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, string labelId) =>
                FilterFactory.CreateFilter<VisualNovel>(FilterName, CanBeNull, operatorString, labelId);

            public static SimpleFilterBase<VisualNovel> EqualTo(string labelId) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, labelId);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(string labelId) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, labelId);
        }

        /// <summary>
        /// Match visual novels that have at least one release matching the given release filters.
        /// </summary>
        public static class Release
        {
            public static string FilterName = VisualNovelConstants.Filters.Release;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, SimpleFilterBase<Domain.ReleaseAggregate.Release> value) =>
                FilterFactory.CreateFilter<VisualNovel, SimpleFilterBase<Domain.ReleaseAggregate.Release>>(FilterName, CanBeNull, operatorString, value);

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, ComplexFilterBase<Domain.ReleaseAggregate.Release> value) =>
                FilterFactory.CreateFilter<VisualNovel, ComplexFilterBase<Domain.ReleaseAggregate.Release>>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(SimpleFilterBase<Domain.ReleaseAggregate.Release> value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(SimpleFilterBase<Domain.ReleaseAggregate.Release> value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(ComplexFilterBase<Domain.ReleaseAggregate.Release> value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(ComplexFilterBase<Domain.ReleaseAggregate.Release> value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Match visual novels that have at least one character matching the given character filters.
        /// </summary>
        public static class Character
        {
            public static string FilterName = VisualNovelConstants.Filters.Character;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, SimpleFilterBase<Domain.CharacterAggregate.Character> value) =>
                FilterFactory.CreateFilter<VisualNovel, SimpleFilterBase<Domain.CharacterAggregate.Character>>(FilterName, CanBeNull, operatorString, value);

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, ComplexFilterBase<Domain.CharacterAggregate.Character> value) =>
                FilterFactory.CreateFilter<VisualNovel, ComplexFilterBase<Domain.CharacterAggregate.Character>>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(SimpleFilterBase<Domain.CharacterAggregate.Character> value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(SimpleFilterBase<Domain.CharacterAggregate.Character> value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(ComplexFilterBase<Domain.CharacterAggregate.Character> value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(ComplexFilterBase<Domain.CharacterAggregate.Character> value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Match visual novels that have at least one staff member matching the given staff filters.
        /// </summary>
        public static class Staff
        {
            public static string FilterName = VisualNovelConstants.Filters.Staff;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, SimpleFilterBase<Domain.StaffAggregate.Staff> value) =>
                FilterFactory.CreateFilter<VisualNovel, SimpleFilterBase<Domain.StaffAggregate.Staff>>(FilterName, CanBeNull, operatorString, value);

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, ComplexFilterBase<Domain.StaffAggregate.Staff> value) =>
                FilterFactory.CreateFilter<VisualNovel, ComplexFilterBase<Domain.StaffAggregate.Staff>>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(SimpleFilterBase<Domain.StaffAggregate.Staff> value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(SimpleFilterBase<Domain.StaffAggregate.Staff> value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(ComplexFilterBase<Domain.StaffAggregate.Staff> value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(ComplexFilterBase<Domain.StaffAggregate.Staff> value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Match visual novels developed by the given producer filters.
        /// </summary>
        public static class Developer
        {
            public static string FilterName = VisualNovelConstants.Filters.Developer;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, SimpleFilterBase<Producer> value) =>
                FilterFactory.CreateFilter<VisualNovel, SimpleFilterBase<Producer>>(FilterName, CanBeNull, operatorString, value);

            private static SimpleFilterBase<VisualNovel> CreateFilter(string operatorString, ComplexFilterBase<Producer> value) =>
                FilterFactory.CreateFilter<VisualNovel, ComplexFilterBase<Producer>>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(SimpleFilterBase<Producer> value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(SimpleFilterBase<Producer> value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<VisualNovel> EqualTo(ComplexFilterBase<Producer> value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<VisualNovel> NotEqualTo(ComplexFilterBase<Producer> value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        public static ComplexFilterBase<VisualNovel> And(params SimpleFilterBase<VisualNovel>[] values) =>
            FilterFactory.CreateComplexFilter(RequestConstants.Operators.Predicates.And, values);

        public static ComplexFilterBase<VisualNovel> Or(params SimpleFilterBase<VisualNovel>[] values) =>
            FilterFactory.CreateComplexFilter(RequestConstants.Operators.Predicates.Or, values);
    }

}