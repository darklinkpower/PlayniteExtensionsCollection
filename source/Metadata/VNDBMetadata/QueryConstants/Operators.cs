using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.QueryConstants
{
    public static class Operators
    {
        public static class Ordering
        {
            public const string GreaterThan = ">";
            public const string GreaterOrEqual = ">=";
            public const string LessThan = "<";
        }

        public static class Matching
        {
            public const string IsEqual = "=";
        }

        public static class Inverting
        {
            public const string NotEqual = "!=";
            public const string LessThanOrEqual = "<=";
        }

        public static class Predicates
        {
            public const string And = "and";
            public const string Or = "or";
        }
    }
}
