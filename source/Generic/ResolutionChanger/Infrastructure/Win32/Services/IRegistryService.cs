using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Infrastructure.Win32.Services
{
    public interface IRegistryService
    {
        object GetValue(
            string path,
            string valueName);

        byte[] GetBinaryValue(
            string path,
            string valueName);

        IReadOnlyList<string> GetSubKeyNames(
            string path);
    }
}
