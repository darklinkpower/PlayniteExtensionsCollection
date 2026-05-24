using DisplayHelper.Domain.Displays.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Tests.Fixtures
{
    public static class DisplayTransactionTestHelper
    {
        public static List<DisplayConfiguration> Order(IReadOnlyList<DisplayConfiguration> input)
        {
            var list = new List<DisplayConfiguration>(input);
            list.Sort((a, b) =>
            {
                if (a.SetAsPrimary == b.SetAsPrimary)
                {
                    return 0;
                }

                return a.SetAsPrimary ? 1 : -1;
            });

            return list;
        }
    }
}
