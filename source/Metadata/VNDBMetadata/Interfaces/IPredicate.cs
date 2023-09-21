using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.Interfaces
{
    public interface IPredicate
    {
        string ToJsonString();
    }
}
