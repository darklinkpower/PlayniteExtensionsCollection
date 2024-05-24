using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApiDomain.ProducerAggregate
{
    public static class ProducerConstants
    {

        public static class ProducerType
        {
            /// <summary>
            /// Producer type: Company
            /// </summary>
            public const string Company = "co";

            /// <summary>
            /// Producer type: Individual
            /// </summary>
            public const string Individual = "in";

            /// <summary>
            /// Producer type: Amateur Group
            /// </summary>
            public const string AmateurGroup = "ng";
        }
    }
}
