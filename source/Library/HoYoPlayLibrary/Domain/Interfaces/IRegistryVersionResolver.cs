﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoYoPlayLibrary.Domain.Interfaces
{
    internal interface IRegistryVersionResolver
    {
        string GetActiveRootKeyPath();
    }
}
