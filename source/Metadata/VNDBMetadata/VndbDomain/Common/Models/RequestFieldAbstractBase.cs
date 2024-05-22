using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.VndbDomain.Common.Models
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
                if (!str.IsNullOrWhiteSpace())
                {
                    prefixParts.Add(str.Trim('.'));
                }
            }

            return string.Join(".", prefixParts) + ".";
        }

        public abstract List<string> GetFlagsStringRepresentations(params string[] prefixParts);
    }
}