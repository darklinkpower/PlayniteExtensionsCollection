using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApiInfrastructure.SharedKernel.Filters
{
    public interface IFilter
    {
        string ToJsonString();
    }
}
