using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VNDB.ApiConstants;
using VNDBMetadata.VNDB.Enums;

namespace VNDBMetadata.VNDB.Models
{
    public interface IStandardPredicateBuilder<T>
    {
        SimplePredicate EqualTo(T value);
        SimplePredicate NotEqualTo(T value);
    }

    public class StandardPredicateBuilder<T> : IStandardPredicateBuilder<T>
    {
        protected readonly string _name;

        public StandardPredicateBuilder(string name)
        {
            _name = name;
        }

        public SimplePredicate EqualTo(T value)
        {
            var predicateOperator = Operators.Matching.IsEqual;
            return new SimplePredicate(_name, predicateOperator, value);
        }

        public SimplePredicate NotEqualTo(T value)
        {
            var predicateOperator = Operators.Matching.NotEqual;
            return new SimplePredicate(_name, predicateOperator, value);
        }
    }

    public class OrderingPredicateBuilder<T> : StandardPredicateBuilder<T>
    {

        public OrderingPredicateBuilder(string name) : base(name)
        {

        }

        public SimplePredicate GreaterThan(T value)
        {
            var predicateOperator = Operators.Ordering.GreaterThan;
            return new SimplePredicate(_name, predicateOperator, value);
        }

        public SimplePredicate GreaterOrEqual(T value)
        {
            var predicateOperator = Operators.Ordering.GreaterThanOrEqual;
            return new SimplePredicate(_name, predicateOperator, value);
        }

        public SimplePredicate LessThan(T value)
        {
            var predicateOperator = Operators.Ordering.LessThan;
            return new SimplePredicate(_name, predicateOperator, value);
        }

        public SimplePredicate LessThanOrEqual(T value)
        {
            var predicateOperator = Operators.Ordering.LessThanOrEqual;
            return new SimplePredicate(_name, predicateOperator, value);
        }
    }

    public class StandardPredicateBuilder<TFirst, TSecond>
    {
        protected readonly string _name;
        public StandardPredicateBuilder(string name)
        {
            _name = name;
        }

        public SimplePredicate EqualTo(TFirst first, TSecond second)
        {
            var predicateOperator = Operators.Matching.IsEqual;
            return new SimplePredicate(_name, predicateOperator, new object[] { first, second });
        }

        public SimplePredicate NotEqualTo(TFirst first, TSecond second)
        {
            var predicateOperator = Operators.Matching.NotEqual;
            return new SimplePredicate(_name, predicateOperator, new object[] { first, second });
        }
    }

    public class OrderingPredicateBuilder<TFirst, TSecond> : StandardPredicateBuilder<TFirst, TSecond>
    {
        public OrderingPredicateBuilder(string name) : base(name)
        {

        }

        public SimplePredicate GreaterThan(TFirst first, TSecond second)
        {
            var predicateOperator = Operators.Ordering.GreaterThan;
            return new SimplePredicate(_name, predicateOperator, new object[] { first, second });
        }

        public SimplePredicate GreaterOrEqual(TFirst first, TSecond second)
        {
            var predicateOperator = Operators.Ordering.GreaterThanOrEqual;
            return new SimplePredicate(_name, predicateOperator, new object[] { first, second });
        }

        public SimplePredicate LessThan(TFirst first, TSecond second)
        {
            var predicateOperator = Operators.Ordering.LessThan;
            return new SimplePredicate(_name, predicateOperator, new object[] { first, second });
        }

        public SimplePredicate LessThanOrEqual(TFirst first, TSecond second)
        {
            var predicateOperator = Operators.Ordering.LessThanOrEqual;
            return new SimplePredicate(_name, predicateOperator, new object[] { first, second });
        }
    }

    public class StandardPredicateBuilder<TFirst, TSecond, TThird>
    {
        protected readonly string _name;
        public StandardPredicateBuilder(string name)
        {
            _name = name;
        }

        public SimplePredicate EqualTo(TFirst first, TSecond second, TThird third)
        {
            
            var predicateOperator = Operators.Matching.IsEqual;
            object[] values = PredicatesBuildersHelper.ConvertValuesToArray(first, second, third);
            return new SimplePredicate(_name, predicateOperator, values);
        }

        public SimplePredicate NotEqualTo(TFirst first, TSecond second, TThird third)
        {
            var predicateOperator = Operators.Matching.NotEqual;
            return new SimplePredicate(_name, predicateOperator, new object[] { first, second, third });
        }
    }

    public class OrderingPredicateBuilder<TFirst, TSecond, TThird> : StandardPredicateBuilder<TFirst, TSecond, TThird>
    {
        public OrderingPredicateBuilder(string name) : base(name)
        {

        }

        public SimplePredicate GreaterThan(TFirst first, TSecond second, TThird third)
        {
            var predicateOperator = Operators.Ordering.GreaterThan;
            return new SimplePredicate(_name, predicateOperator, new object[] { first, second, third });
        }

        public SimplePredicate GreaterOrEqual(TFirst first, TSecond second, TThird third)
        {
            var predicateOperator = Operators.Ordering.GreaterThanOrEqual;
            return new SimplePredicate(_name, predicateOperator, new object[] { first, second, third });
        }

        public SimplePredicate LessThan(TFirst first, TSecond second, TThird third)
        {
            var predicateOperator = Operators.Ordering.LessThan;
            return new SimplePredicate(_name, predicateOperator, new object[] { first, second, third });
        }

        public SimplePredicate LessThanOrEqual(TFirst first, TSecond second, TThird third)
        {
            var predicateOperator = Operators.Ordering.LessThanOrEqual;
            return new SimplePredicate(_name, predicateOperator, new object[] { first, second, third });
        }
    }

    public static class PredicatesBuildersHelper
    {
        public static object[] ConvertValuesToArray(params object[] values)
        {
            return values.Select(x => ConvertValue(x)).ToArray();
        }

        private static object ConvertValue(object value)
        {
            if (value is Enum enumValue)
            {
                return ConvertEnumValueToRepresentation(enumValue);
            }

            return value;
        }

        private static object ConvertEnumValueToRepresentation(Enum enumValue)
        {
            if (Enum.GetUnderlyingType(enumValue.GetType()) == typeof(int))
            {
                int intValue = (int)(object)enumValue;
                return intValue;
            }
            else
            {
                var memberInfo = enumValue.GetType().GetMember(enumValue.ToString());
                var attribute = memberInfo[0].GetCustomAttribute(typeof(StringRepresentationAttribute)) as StringRepresentationAttribute;
                if (attribute != null)
                {
                    return attribute.Value;
                }
            }

            throw new InvalidOperationException("No value could be obtained for the enum.");
        }
    }
}