using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.StaffAggregate;
using VNDBMetadata.VndbDomain.Common;
using VNDBMetadata.VndbDomain.Common.Constants;
using VNDBMetadata.VndbDomain.Common.Enums;
using VNDBMetadata.VndbDomain.Common.Filters;
using VNDBMetadata.VndbDomain.Common.Interfaces;

namespace VNDBMetadata.VndbDomain.Aggregates.CharacterAggregate
{
    public static class CharacterFilterFactory
    {
        public static class Id
		{
			public static string FilterName = CharacterConstants.Filters.Id;
			public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, string value) =>
                FilterFactory.CreateFilter<Character>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> GreaterThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Character> GreaterThan(string value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Character> LessThanOrEqual(string value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Character> LessThan(string value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        public static class Search
        {
            public static string FilterName = CharacterConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, string value) =>
                 FilterFactory.CreateFilter<Character>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(string value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(string value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// String, see vns.role field. If this filter is used when nested inside a visual novel filter, then this matches the role of the particular visual novel. Otherwise, this matches the role of any linked visual novel.
        /// </summary>
        public static class Role
        {
            public static string FilterName = CharacterConstants.Filters.Role;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, CharacterRoleEnum value) =>
                FilterFactory.CreateFilter<Character, CharacterRoleEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(CharacterRoleEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(CharacterRoleEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// String.
        /// </summary>
        public static class BloodType
        {
            public static string FilterName = CharacterConstants.Filters.BloodType;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, CharacterBloodTypeEnum value) =>
                FilterFactory.CreateFilter<Character, CharacterBloodTypeEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(CharacterBloodTypeEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(CharacterBloodTypeEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        /// <summary>
        /// String.
        /// </summary>
        public static class Sex
        {
            public static string FilterName = CharacterConstants.Filters.Sex;
            public static bool CanBeNull { get; } = false;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, CharacterSexEnum value) =>
                FilterFactory.CreateFilter<Character, CharacterSexEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(CharacterSexEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(CharacterSexEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class Height
        {
            public static string FilterName = CharacterConstants.Filters.Height;
            public static bool CanBeNull { get; } = true;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint value) =>
                FilterFactory.CreateFilter<Character, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(uint value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(uint value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> GreaterThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Character> GreaterThan(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Character> LessThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Character> LessThan(uint value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        public static class Weight
        {
            public static string FilterName = CharacterConstants.Filters.Weight;
            public static bool CanBeNull { get; } = true;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint value) =>
                FilterFactory.CreateFilter<Character, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(uint value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(uint value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> GreaterThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Character> GreaterThan(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Character> LessThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Character> LessThan(uint value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        public static class Bust
        {
            public static string FilterName = CharacterConstants.Filters.Bust;
            public static bool CanBeNull { get; } = true;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint value) =>
                FilterFactory.CreateFilter<Character, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(uint value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(uint value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> GreaterThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Character> GreaterThan(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Character> LessThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Character> LessThan(uint value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        public static class Waist
        {
            public static string FilterName = CharacterConstants.Filters.Waist;
            public static bool CanBeNull { get; } = true;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint value) =>
                FilterFactory.CreateFilter<Character, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(uint value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(uint value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> GreaterThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Character> GreaterThan(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Character> LessThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Character> LessThan(uint value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        public static class Hips
        {
            public static string FilterName = CharacterConstants.Filters.Hips;
            public static bool CanBeNull { get; } = true;
            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint value) =>
                FilterFactory.CreateFilter<Character, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(uint value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(uint value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> GreaterThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Character> GreaterThan(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Character> LessThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Character> LessThan(uint value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        public static class Cup
        {
            public static string FilterName = CharacterConstants.Filters.Cup;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, CharacterCupSizeEnum value) =>
                FilterFactory.CreateFilter<Character, CharacterCupSizeEnum>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(CharacterCupSizeEnum value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(CharacterCupSizeEnum value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        public static class Age
        {
            public static string FilterName = CharacterConstants.Filters.Age;
            public static bool CanBeNull { get; } = true;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint value) =>
                FilterFactory.CreateFilter<Character, uint>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(uint value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(uint value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> GreaterThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThanOrEqual, value);

            public static SimpleFilterBase<Character> GreaterThan(uint value) =>
                CreateFilter(Operators.Ordering.GreaterThan, value);

            public static SimpleFilterBase<Character> LessThanOrEqual(uint value) =>
                CreateFilter(Operators.Ordering.LessThanOrEqual, value);

            public static SimpleFilterBase<Character> LessThan(uint value) =>
                CreateFilter(Operators.Ordering.LessThan, value);
        }

        public static class Trait
        {
            public static string FilterName = CharacterConstants.Filters.Trait;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint traitId, SpoilerLevel maxSpoilerLevel) =>
                FilterFactory.CreateFilter<Character>(FilterName, CanBeNull, operatorString, traitId, maxSpoilerLevel);

            public static SimpleFilterBase<Character> EqualTo(uint traitId, SpoilerLevel maxSpoilerLevel = SpoilerLevel.None) =>
                CreateFilter(Operators.Matching.IsEqual, traitId, maxSpoilerLevel);

            public static SimpleFilterBase<Character> NotEqualTo(uint traitId, SpoilerLevel maxSpoilerLevel = SpoilerLevel.None) =>
                CreateFilter(Operators.Matching.NotEqual, traitId, maxSpoilerLevel);
        }

        public static class DirectTrait
        {
            public static string FilterName = CharacterConstants.Filters.DirectTrait;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint traitId, SpoilerLevel maxSpoilerLevel) =>
                FilterFactory.CreateFilter<Character>(FilterName, CanBeNull, operatorString, traitId, maxSpoilerLevel);

            public static SimpleFilterBase<Character> EqualTo(uint traitId, SpoilerLevel maxSpoilerLevel = SpoilerLevel.None) =>
                CreateFilter(Operators.Matching.IsEqual, traitId, maxSpoilerLevel);

            public static SimpleFilterBase<Character> NotEqualTo(uint traitId, SpoilerLevel maxSpoilerLevel = SpoilerLevel.None) =>
                CreateFilter(Operators.Matching.NotEqual, traitId, maxSpoilerLevel);
        }

        public static class Birthday
        {
            public static string FilterName = CharacterConstants.Filters.Birthday;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, uint month, uint day) =>
                FilterFactory.CreateFilter<Character, uint>(FilterName, CanBeNull, operatorString, month, day);

            public static SimpleFilterBase<Character> EqualTo(uint month, uint day = 0) =>
                CreateFilter(Operators.Matching.IsEqual, month, day);

            public static SimpleFilterBase<Character> NotEqualTo(uint month, uint day = 0) =>
                CreateFilter(Operators.Matching.NotEqual, month, day);
        }

        public static class Seiyuu
        {
            public static string FilterName = CharacterConstants.Filters.Seiyuu;
            public static bool CanBeNull { get; } = false;

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, SimpleFilterBase<Staff> value) =>
                FilterFactory.CreateFilter<Character, SimpleFilterBase<Staff>>(FilterName, CanBeNull, operatorString, value);

            private static SimpleFilterBase<Character> CreateFilter(string operatorString, ComplexFilterBase<Staff> value) =>
                FilterFactory.CreateFilter<Character, ComplexFilterBase<Staff>>(FilterName, CanBeNull, operatorString, value);

            public static SimpleFilterBase<Character> EqualTo(SimpleFilterBase<Staff> value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(SimpleFilterBase<Staff> value) =>
                CreateFilter(Operators.Matching.NotEqual, value);

            public static SimpleFilterBase<Character> EqualTo(ComplexFilterBase<Staff> value) =>
                CreateFilter(Operators.Matching.IsEqual, value);

            public static SimpleFilterBase<Character> NotEqualTo(ComplexFilterBase<Staff> value) =>
                CreateFilter(Operators.Matching.NotEqual, value);
        }

        //public static class VisualNovel
        //{
        //    public static string FilterName = CharacterConstants.Filters.VisualNovel;
        //    public static bool CanBeNull { get; } = false;

        //}

        public static ComplexFilterBase<Character> And(params SimpleFilterBase<Character>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.And, values);

        public static ComplexFilterBase<Character> Or(params SimpleFilterBase<Character>[] values) =>
            FilterFactory.CreateComplexFilter(Operators.Predicates.Or, values);
    }

}