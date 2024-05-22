using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBFuze.VndbDomain.Common.Constants
{
    public static class Operators
    {
        public interface IOperator
        {
            string Value { get; }
        }

        public struct OrderingOperator : IOperator
        {
            public static readonly OrderingOperator GreaterThan = new OrderingOperator(">");
            public static readonly OrderingOperator GreaterOrEqual = new OrderingOperator(">=");
            public static readonly OrderingOperator LessThan = new OrderingOperator("<");
            private readonly string _value;
            public string Value => _value;
            private OrderingOperator(string value)
            {
                _value = value;
            }

            public static implicit operator string(OrderingOperator op) => op._value;
        }

        public static class Ordering
        {
            public const string GreaterThan = ">";
            public const string GreaterThanOrEqual = ">=";
            public const string LessThan = "<";
            public const string LessThanOrEqual = "<=";
        }

        public static class Matching
        {
            public const string IsEqual = "=";
            public const string NotEqual = "!=";
        }

        public static class Predicates
        {
            public const string And = "and";
            public const string Or = "or";
        }
    }
}
