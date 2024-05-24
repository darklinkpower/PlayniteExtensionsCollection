using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApiInfrastructure.SharedKernel.Requests
{
    public abstract class RequestFieldAbstractBase
    {
        protected string GetFullPrefixString(params string[] strings)
        {
            if (strings.Length == 0)
            {
                return string.Empty;
            }
            
            var prefixParts = new List<string>();
            foreach (var str in strings)
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    prefixParts.Add(str.Trim('.'));
                }
            }

            return string.Join(".", prefixParts) + ".";
        }

        public abstract List<string> GetFlagsStringRepresentations(params string[] prefixParts);
    }
}