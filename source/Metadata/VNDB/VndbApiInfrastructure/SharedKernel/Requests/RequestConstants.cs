using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApiInfrastructure.SharedKernel.Requests
{
    public static class RequestConstants
    {
        public static class Operators
        {
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
}