using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDB.ApiConstants;
using VNDBMetadata.Interfaces;
using VNDBMetadata.Models;
using VNDBMetadata.VNDB.Enums;

namespace VNDBMetadata.Filters
{
    public class ProducerFilter : SimpleFilterBase
    {
        internal ProducerFilter(string filterName, string filterOperator, object value) : base(filterName, filterOperator, value)
        {

        }
    }

    public class ProducerComplexFilter : ComplexFilterBase
    {
        internal ProducerComplexFilter(string filterOperator, params IFilter[] value) : base(filterOperator, value)
        {

        }
    }

    public static class ProducerFilterFactory
	{
        public static class Id
		{
			public static string FilterName = ProducerConstants.Filters.Id;
			public static bool CanBeNull { get; } = false;
			public static ProducerFilter EqualTo(string value) =>  new ProducerFilter(
				FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null(value) : value);
			public static ProducerFilter NotEqualTo(string value) => new ProducerFilter(
				FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null(value) : value);
			public static ProducerFilter GreaterThanOrEqual(string value) => new ProducerFilter(
				FilterName, Operators.Ordering.GreaterThanOrEqual, CanBeNull ? Guard.Against.Null(value) : value);
			public static ProducerFilter GreaterThan(string value) => new ProducerFilter(
				FilterName, Operators.Ordering.GreaterThan, CanBeNull ? Guard.Against.Null(value) : value);
			public static ProducerFilter LessThanOrEqual(string value) => new ProducerFilter(
				FilterName, Operators.Ordering.LessThanOrEqual, CanBeNull ? Guard.Against.Null(value) : value);
			public static ProducerFilter LessThan(string value) => new ProducerFilter(
				FilterName, Operators.Ordering.LessThan, CanBeNull ? Guard.Against.Null(value) : value);
		}

        public static class Search
        {
            public static string FilterName = ProducerConstants.Filters.Search;
            public static bool CanBeNull { get; } = false;
            public static ProducerFilter EqualTo(string value) => new ProducerFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null(value) : value);
            public static ProducerFilter NotEqualTo(string value) => new ProducerFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null(value) : value);
        }

        public static class Language
        {
            public static string FilterName = ProducerConstants.Filters.Lang;
            public static bool CanBeNull { get; } = false;
            public static ProducerFilter EqualTo(LanguageEnums value) => new ProducerFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<LanguageEnums>(value) : value);
            public static ProducerFilter NotEqualTo(LanguageEnums value) => new ProducerFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<LanguageEnums>(value) : value);
        }

        public static class Type
        {
            public static string FilterName = ProducerConstants.Filters.Type;
            public static bool CanBeNull { get; } = false;
            public static ProducerFilter EqualTo(ProducerTypeEnum value) => new ProducerFilter(
                FilterName, Operators.Matching.IsEqual, CanBeNull ? Guard.Against.Null<ProducerTypeEnum>(value) : value);
            public static ProducerFilter NotEqualTo(ProducerTypeEnum value) => new ProducerFilter(
                FilterName, Operators.Matching.NotEqual, CanBeNull ? Guard.Against.Null<ProducerTypeEnum>(value) : value);
        }

        public static ProducerComplexFilter And(params ProducerFilter[] value) => new ProducerComplexFilter(
            Operators.Predicates.And, Guard.Against.Null(value));

        public static ProducerComplexFilter Or(params ProducerFilter[] value) => new ProducerComplexFilter(
            Operators.Predicates.Or, Guard.Against.Null(value));
    }

}