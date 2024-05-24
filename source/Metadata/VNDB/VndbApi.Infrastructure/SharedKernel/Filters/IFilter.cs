using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApi.Infrastructure.SharedKernel.Filters
{
    public interface IFilter
    {
        string ToJsonString();
    }
}
