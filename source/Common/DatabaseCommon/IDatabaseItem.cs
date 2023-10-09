using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseCommon
{
    public interface IDatabaseItem<T>
    {
        Guid Id { get; set; }
        T GetClone();
    }
}