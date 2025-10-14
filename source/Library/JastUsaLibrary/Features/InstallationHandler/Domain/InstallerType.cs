using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.InstallationHandler.Domain
{
    public enum InstallerType
    {
        Unknown,
        Msi,
        InnoSetup,
        NullSoft,
        InstallShield,
        Archive,
        SetupFactory
    }
}
