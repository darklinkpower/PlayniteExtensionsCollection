using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApiDomain.SharedKernel.Entities
{
    public interface IAggregateRoot
    {
        string Id { get; set; }
    }
}