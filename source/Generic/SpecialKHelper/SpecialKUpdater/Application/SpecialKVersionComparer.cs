using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKUpdater.Application
{
    public static class SpecialKVersionComparer
    {
        public static int Compare(
            string left,
            string right)
        {
            if (string.IsNullOrWhiteSpace(left))
            {
                return string.IsNullOrWhiteSpace(right)
                    ? 0
                    : -1;
            }

            if (string.IsNullOrWhiteSpace(right))
            {
                return 1;
            }

            var leftParts = left.Split('.')
                .Select(ParsePart)
                .ToArray();

            var rightParts = right.Split('.')
                .Select(ParsePart)
                .ToArray();

            var max = Math.Max(
                leftParts.Length,
                rightParts.Length);

            for (var i = 0; i < max; i++)
            {
                var l = i < leftParts.Length
                    ? leftParts[i]
                    : 0;

                var r = i < rightParts.Length
                    ? rightParts[i]
                    : 0;

                if (l > r)
                {
                    return 1;
                }

                if (l < r)
                {
                    return -1;
                }
            }

            return 0;
        }

        private static int ParsePart(string value)
        {
            return int.TryParse(value, out var result)
                ? result
                : 0;
        }
    }
}
