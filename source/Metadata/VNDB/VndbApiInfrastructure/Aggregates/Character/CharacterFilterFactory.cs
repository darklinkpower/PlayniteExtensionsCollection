using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.StaffAggregate;
using VndbApiDomain.SharedKernel;
using VndbApiDomain.CharacterAggregate;
using VndbApiInfrastructure.SharedKernel.Requests;
using VndbApiInfrastructure.SharedKernel.Filters;

namespace VndbApiInfrastructure.CharacterAggregate
{
    public static class CharacterFilterFactory
    {
        /// <summary>
        /// vndbid
        /// </summary>
        public static class Id
		{
			public static string FilterName = CharacterConstants.Filters.Id;
			public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Character>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> GreaterThanOrEqual(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Character> GreaterThan(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Character> LessThanOrEqual(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Character> LessThan(string value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// String search.
        /// </summary>
        public static class Search
        {
            public static string FilterName = CharacterConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, string value) =>
                 FilterFactory.CreateFilter<Character>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(string value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// String, see vns.role field. If this filter is used when nested inside a visual novel filter, then this matches the role of the particular visual novel. Otherwise, this matches the role of any linked visual novel.
        /// "main" for protagonist, "primary" for main characters, "side" or "appears". 
        /// </summary>
        public static class Role
        {
            public static string FilterName = CharacterConstants.Filters.Role;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, CharacterRoleEnum value) =>
                FilterFactory.CreateFilter<Character, CharacterRoleEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(CharacterRoleEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(CharacterRoleEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Character blood type.
        /// </summary>
        public static class BloodType
        {
            public static string FilterName = CharacterConstants.Filters.BloodType;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, CharacterBloodTypeEnum value) =>
                FilterFactory.CreateFilter<Character, CharacterBloodTypeEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(CharacterBloodTypeEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(CharacterBloodTypeEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Character sex.
        /// </summary>
        public static class Sex
        {
            public static string FilterName = CharacterConstants.Filters.Sex;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, CharacterSexEnum value) =>
                FilterFactory.CreateFilter<Character, CharacterSexEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(CharacterSexEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(CharacterSexEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Character spoiler sex.
        /// </summary>
        public static class SexSpoiler
        {
            public static string FilterName = CharacterConstants.Filters.SexSpoil;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, CharacterSexEnum value) =>
                FilterFactory.CreateFilter<Character, CharacterSexEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(CharacterSexEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(CharacterSexEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Height value.
        /// </summary>
        public static class Height
        {
            public static string FilterName = CharacterConstants.Filters.Height;
            public static bool CanBeNull { get; } = true;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint value) =>
                FilterFactory.CreateFilter<Character, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(uint value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(uint value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> GreaterThanOrEqual(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Character> GreaterThan(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Character> LessThanOrEqual(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Character> LessThan(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// Waist size.
        /// </summary>
        public static class Weight
        {
            public static string FilterName = CharacterConstants.Filters.Weight;
            public static bool CanBeNull { get; } = true;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint value) =>
                FilterFactory.CreateFilter<Character, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(uint value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(uint value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> GreaterThanOrEqual(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Character> GreaterThan(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Character> LessThanOrEqual(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Character> LessThan(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// Bust size.
        /// </summary>
        public static class Bust
        {
            public static string FilterName = CharacterConstants.Filters.Bust;
            public static bool CanBeNull { get; } = true;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint value) =>
                FilterFactory.CreateFilter<Character, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(uint value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(uint value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> GreaterThanOrEqual(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Character> GreaterThan(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Character> LessThanOrEqual(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Character> LessThan(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// Waist Size.
        /// </summary>
        public static class Waist
        {
            public static string FilterName = CharacterConstants.Filters.Waist;
            public static bool CanBeNull { get; } = true;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint value) =>
                FilterFactory.CreateFilter<Character, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(uint value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(uint value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> GreaterThanOrEqual(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Character> GreaterThan(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Character> LessThanOrEqual(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Character> LessThan(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// Hips size.
        /// </summary>
        public static class Hips
        {
            public static string FilterName = CharacterConstants.Filters.Hips;
            public static bool CanBeNull { get; } = true;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint value) =>
                FilterFactory.CreateFilter<Character, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(uint value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(uint value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> GreaterThanOrEqual(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Character> GreaterThan(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Character> LessThanOrEqual(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Character> LessThan(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// Cup size.
        /// </summary>
        public static class Cup
        {
            public static string FilterName = CharacterConstants.Filters.Cup;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, CharacterCupSizeEnum value) =>
                FilterFactory.CreateFilter<Character, CharacterCupSizeEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(CharacterCupSizeEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(CharacterCupSizeEnum value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Age.
        /// </summary>
        public static class Age
        {
            public static string FilterName = CharacterConstants.Filters.Age;
            public static bool CanBeNull { get; } = true;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint value) =>
                FilterFactory.CreateFilter<Character, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(uint value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(uint value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> GreaterThanOrEqual(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Character> GreaterThan(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Character> LessThanOrEqual(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Character> LessThan(uint value) =>
                CreateFilter(RequestConstants.Operators.Ordering.LessThan, value);
        }

        /// <summary>
        /// Traits applied to this character, also matches parent traits. See below for more details.
        /// </summary>
        public static class Trait
        {
            public static string FilterName = CharacterConstants.Filters.Trait;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint traitId, SpoilerLevelEnum maxSpoilerLevel) =>
                FilterFactory.CreateFilter<Character>(FilterName, CanBeNull, operatorString, traitId, maxSpoilerLevel);

            public static SimpleFilterBase<Character> EqualTo(uint traitId, SpoilerLevelEnum maxSpoilerLevel = SpoilerLevelEnum.None) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, traitId, maxSpoilerLevel);

            public static SimpleFilterBase<Character> NotEqualTo(uint traitId, SpoilerLevelEnum maxSpoilerLevel = SpoilerLevelEnum.None) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, traitId, maxSpoilerLevel);
        }

        /// <summary>
        /// Traits applied directly to this character, does not match parent traits. See below for details.
        /// </summary>
        public static class DirectTrait
        {
            public static string FilterName = CharacterConstants.Filters.DirectTrait;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint traitId, SpoilerLevelEnum maxSpoilerLevel) =>
                FilterFactory.CreateFilter<Character>(FilterName, CanBeNull, operatorString, traitId, maxSpoilerLevel);

            public static SimpleFilterBase<Character> EqualTo(uint traitId, SpoilerLevelEnum maxSpoilerLevel = SpoilerLevelEnum.None) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, traitId, maxSpoilerLevel);

            public static SimpleFilterBase<Character> NotEqualTo(uint traitId, SpoilerLevelEnum maxSpoilerLevel = SpoilerLevelEnum.None) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, traitId, maxSpoilerLevel);
        }

        /// <summary>
        /// Array of two integers, month and day. Day may be 0 to find characters whose birthday is in a given month.
        /// </summary>
        public static class Birthday
        {
            public static string FilterName = CharacterConstants.Filters.Birthday;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint month, uint day) =>
                FilterFactory.CreateFilter<Character, uint>(FilterName, CanBeNull, operatorString, month, day);

            public static SimpleFilterBase<Character> EqualTo(uint month, uint day = 0) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, month, day);

            public static SimpleFilterBase<Character> NotEqualTo(uint month, uint day = 0) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, month, day);
        }

        /// <summary>
        /// Match characters that are voiced by the matching staff filters.
        /// Voice actor information is actually specific to visual novels, but this filter does not (currently) correlate against the parent entry when nested inside a visual novel filter.
        /// </summary>
        public static class Seiyuu
        {
            public static string FilterName = CharacterConstants.Filters.Seiyuu;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, SimpleFilterBase<Staff> value) =>
                FilterFactory.CreateFilter<Character, SimpleFilterBase<Staff>>(FilterName, CanBeNull, operatorString, value);

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, ComplexFilterBase<Staff> value) =>
                FilterFactory.CreateFilter<Character, ComplexFilterBase<Staff>>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(SimpleFilterBase<Staff> value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(SimpleFilterBase<Staff> value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> EqualTo(ComplexFilterBase<Staff> value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(ComplexFilterBase<Staff> value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// Match characters linked to visual novels described by visual novel filters.
        /// </summary>
        /// <returns></returns>
        public static class VisualNovel
        {
            public static string FilterName = CharacterConstants.Filters.VisualNovel;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, SimpleFilterBase<VndbApiDomain.VisualNovelAggregate.VisualNovel> value) =>
                FilterFactory.CreateFilter<Character, SimpleFilterBase<VndbApiDomain.VisualNovelAggregate.VisualNovel>>(FilterName, CanBeNull, operatorString, value);

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, ComplexFilterBase<VndbApiDomain.VisualNovelAggregate.VisualNovel> value) =>
                FilterFactory.CreateFilter<Character, ComplexFilterBase<VndbApiDomain.VisualNovelAggregate.VisualNovel>>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(SimpleFilterBase<VndbApiDomain.VisualNovelAggregate.VisualNovel> value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(SimpleFilterBase<VndbApiDomain.VisualNovelAggregate.VisualNovel> value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> EqualTo(ComplexFilterBase<VndbApiDomain.VisualNovelAggregate.VisualNovel> value) =>
                CreateFilter(RequestConstants.Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(ComplexFilterBase<VndbApiDomain.VisualNovelAggregate.VisualNovel> value) =>
                CreateFilter(RequestConstants.Operators.Matching.NotEqual, value);
        }

        public static ComplexFilterBase<Character> And(params SimpleFilterBase<Character>[] values) =>
            FilterFactory.CreateComplexFilter(RequestConstants.Operators.Predicates.And, values);

        public static ComplexFilterBase<Character> Or(params SimpleFilterBase<Character>[] values) =>
            FilterFactory.CreateComplexFilter(RequestConstants.Operators.Predicates.Or, values);
    }

}