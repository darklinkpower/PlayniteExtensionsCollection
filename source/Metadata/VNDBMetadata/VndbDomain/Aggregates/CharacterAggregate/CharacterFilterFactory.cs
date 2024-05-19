using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.StaffAggregate;
using VNDBMetadata.VndbDomain.Common.Constants;
using VNDBMetadata.VndbDomain.Common.Enums;
using VNDBMetadata.VndbDomain.Common.Filters;
using VNDBMetadata.VndbDomain.Common.Interfaces;

namespace VNDBMetadata.VndbDomain.Aggregates.CharacterAggregate
{
    public class CharacterFilter : SimpleFilterBase
    {
        internal CharacterFilter(string filterName, string filterOperator, object value) : base(filterName, filterOperator, value)
        {

        }

        internal CharacterFilter(string filterName, string filterOperator, params object[] values) : base(filterName, filterOperator, values)
        {

        }
    }

    public class CharacterComplexFilter : ComplexFilterBase
    {
        internal CharacterComplexFilter(string filterOperator, params IFilter[] value) : base(filterOperator, value)
        {

        }
    }

    public static class CharacterFilterFactory
    {
        public static class Id
		{
			public static string FilterName = CharacterConstants.Filters.Id;
			public static bool CanBeNull { get; } = false;
			public static CharacterFilter EqualTo(string value) =>  new CharacterFilter(
				FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static CharacterFilter NotEqualTo(string value) => new CharacterFilter(
				FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static CharacterFilter GreaterThanOrEqual(string value) => new CharacterFilter(
				FilterName, Operators.Ordering.GreaterThanOrEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static CharacterFilter GreaterThan(string value) => new CharacterFilter(
				FilterName, Operators.Ordering.GreaterThan, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static CharacterFilter LessThanOrEqual(string value) => new CharacterFilter(
				FilterName, Operators.Ordering.LessThanOrEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
			public static CharacterFilter LessThan(string value) => new CharacterFilter(
				FilterName, Operators.Ordering.LessThan, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
		}

        public static class Search
        {
            public static string FilterName = CharacterConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            public static CharacterFilter EqualTo(string value) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
            public static CharacterFilter NotEqualTo(string value) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.NullOrWhiteSpace(value) : value);
        }

        /// <summary>
        /// String, see vns.role field. If this filter is used when nested inside a visual novel filter, then this matches the role of the particular visual novel. Otherwise, this matches the role of any linked visual novel.
        /// </summary>
        public static class Role
        {
            public static string FilterName = CharacterConstants.Filters.Role;
            public static bool CanBeNull { get; } = false;
            public static CharacterFilter EqualTo(CharacterRoleEnum value) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<CharacterRoleEnum>(value) : value);
            public static CharacterFilter NotEqualTo(CharacterRoleEnum value) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<CharacterRoleEnum>(value) : value);
        }

        /// <summary>
        /// String.
        /// </summary>
        public static class BloodType
        {
            public static string FilterName = CharacterConstants.Filters.BloodType;
            public static bool CanBeNull { get; } = false;
            public static CharacterFilter EqualTo(CharacterBloodTypeEnum value) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<CharacterBloodTypeEnum>(value) : value);
            public static CharacterFilter NotEqualTo(CharacterBloodTypeEnum value) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<CharacterBloodTypeEnum>(value) : value);
        }

        /// <summary>
        /// String.
        /// </summary>
        public static class Sex
        {
            public static string FilterName = CharacterConstants.Filters.Sex;
            public static bool CanBeNull { get; } = false;
            public static CharacterFilter EqualTo(CharacterSexEnum value) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<CharacterSexEnum>(value) : value);
            public static CharacterFilter NotEqualTo(CharacterSexEnum value) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<CharacterSexEnum>(value) : value);
        }

        public static class Height
        {
            public static string FilterName = CharacterConstants.Filters.Height;
            public static bool CanBeNull { get; } = true;
            public static CharacterFilter EqualTo(uint value) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter NotEqualTo(uint value) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter GreaterThanOrEqual(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.GreaterThanOrEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter GreaterThan(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.GreaterThan, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter LessThanOrEqual(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.LessThanOrEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter LessThan(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.LessThan, CanBeNull ? Guard.Against.Null<uint>(value) : value);
        }

        public static class Weight
        {
            public static string FilterName = CharacterConstants.Filters.Weight;
            public static bool CanBeNull { get; } = true;
            public static CharacterFilter EqualTo(uint value) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter NotEqualTo(uint value) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter GreaterThanOrEqual(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.GreaterThanOrEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter GreaterThan(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.GreaterThan, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter LessThanOrEqual(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.LessThanOrEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter LessThan(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.LessThan, CanBeNull ? Guard.Against.Null<uint>(value) : value);
        }

        public static class Bust
        {
            public static string FilterName = CharacterConstants.Filters.Bust;
            public static bool CanBeNull { get; } = true;
            public static CharacterFilter EqualTo(uint value) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter NotEqualTo(uint value) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter GreaterThanOrEqual(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.GreaterThanOrEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter GreaterThan(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.GreaterThan, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter LessThanOrEqual(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.LessThanOrEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter LessThan(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.LessThan, CanBeNull ? Guard.Against.Null<uint>(value) : value);
        }

        public static class Waist
        {
            public static string FilterName = CharacterConstants.Filters.Waist;
            public static bool CanBeNull { get; } = true;
            public static CharacterFilter EqualTo(uint value) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter NotEqualTo(uint value) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter GreaterThanOrEqual(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.GreaterThanOrEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter GreaterThan(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.GreaterThan, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter LessThanOrEqual(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.LessThanOrEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter LessThan(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.LessThan, CanBeNull ? Guard.Against.Null<uint>(value) : value);
        }

        public static class Hips
        {
            public static string FilterName = CharacterConstants.Filters.Hips;
            public static bool CanBeNull { get; } = true;
            public static CharacterFilter EqualTo(uint value) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter NotEqualTo(uint value) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter GreaterThanOrEqual(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.GreaterThanOrEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter GreaterThan(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.GreaterThan, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter LessThanOrEqual(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.LessThanOrEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter LessThan(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.LessThan, CanBeNull ? Guard.Against.Null<uint>(value) : value);
        }

        public static class Cup
        {
            public static string FilterName = CharacterConstants.Filters.Cup;
            public static bool CanBeNull { get; } = false;
            public static CharacterFilter EqualTo(CharacterCupSizeEnum value) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<CharacterCupSizeEnum>(value) : value);
            public static CharacterFilter NotEqualTo(CharacterCupSizeEnum value) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<CharacterCupSizeEnum>(value) : value);
        }

        public static class Age
        {
            public static string FilterName = CharacterConstants.Filters.Age;
            public static bool CanBeNull { get; } = false;
            public static CharacterFilter EqualTo(uint value) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter NotEqualTo(uint value) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter GreaterThanOrEqual(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.GreaterThanOrEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter GreaterThan(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.GreaterThan, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter LessThanOrEqual(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.LessThanOrEqual, CanBeNull ? Guard.Against.Null<uint>(value) : value);
            public static CharacterFilter LessThan(uint value) => new CharacterFilter(
                FilterName, Operators.Ordering.LessThan, CanBeNull ? Guard.Against.Null<uint>(value) : value);
        }

        public static class Trait
        {
            public static string FilterName = CharacterConstants.Filters.Trait;
            public static bool CanBeNull { get; } = false;

            public static CharacterFilter EqualTo(uint traitId, SpoilerLevel maxSpoilerLevel = SpoilerLevel.None) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual,
                CanBeNull ? Guard.Against.Null<uint>(traitId) : traitId,
                CanBeNull ? Guard.Against.Null<SpoilerLevel>(maxSpoilerLevel) : maxSpoilerLevel);

            public static CharacterFilter NotEqualTo(uint traitId, SpoilerLevel maxSpoilerLevel = SpoilerLevel.None) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual,
                CanBeNull ? Guard.Against.Null<uint>(traitId) : traitId,
                CanBeNull ? Guard.Against.Null<SpoilerLevel>(maxSpoilerLevel) : maxSpoilerLevel);
        }

        public static class DirectTrait
        {
            public static string FilterName = CharacterConstants.Filters.DirectTrait;
            public static bool CanBeNull { get; } = false;

            public static CharacterFilter EqualTo(uint traitId, SpoilerLevel maxSpoilerLevel = SpoilerLevel.None) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual,
                CanBeNull ? Guard.Against.Null<uint>(traitId) : traitId,
                CanBeNull ? Guard.Against.Null<SpoilerLevel>(maxSpoilerLevel) : maxSpoilerLevel);

            public static CharacterFilter NotEqualTo(uint traitId, SpoilerLevel maxSpoilerLevel = SpoilerLevel.None) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual,
                CanBeNull ? Guard.Against.Null<uint>(traitId) : traitId,
                CanBeNull ? Guard.Against.Null<SpoilerLevel>(maxSpoilerLevel) : maxSpoilerLevel);
        }

        public static class Birthday
        {
            public static string FilterName = CharacterConstants.Filters.Birthday;
            public static bool CanBeNull { get; } = false;

            public static CharacterFilter EqualTo(uint month, uint day = 0) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual,
                CanBeNull ? Guard.Against.Null<uint>(month) : month,
                CanBeNull ? Guard.Against.Null<uint>(day) : day);

            public static CharacterFilter NotEqualTo(uint month, uint day = 0) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual,
                CanBeNull ? Guard.Against.Null<uint>(month) : month,
                CanBeNull ? Guard.Against.Null<uint>(day) : day);
        }

        public static class Seiyuu
        {
            public static string FilterName = CharacterConstants.Filters.Seiyuu;
            public static bool CanBeNull { get; } = false;

            public static CharacterFilter EqualTo(StaffFilter staffFilter) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null(staffFilter) : staffFilter);

            public static CharacterFilter NotEqualTo(StaffFilter staffFilter) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null(staffFilter) : staffFilter);

            public static CharacterFilter EqualTo(StaffComplexFilter staffFilter) => new CharacterFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null(staffFilter) : staffFilter);

            public static CharacterFilter NotEqualTo(StaffComplexFilter staffFilter) => new CharacterFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null(staffFilter) : staffFilter);
        }

        //public static class VisualNovel
        //{
        //    public static string FilterName = CharacterConstants.Filters.VisualNovel;
        //    public static bool CanBeNull { get; } = false;

        //}

        public static CharacterComplexFilter And(params CharacterFilter[] value) => new CharacterComplexFilter(
            Operators.Predicates.And, Guard.Against.Null(value));

        public static CharacterComplexFilter Or(params CharacterFilter[] value) => new CharacterComplexFilter(
            Operators.Predicates.Or, Guard.Against.Null(value));
    }

}