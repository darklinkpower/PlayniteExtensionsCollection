using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Infrastructure.Win32.Services
{
    public sealed class RegistryService : IRegistryService
    {
        public object GetValue(
            string path,
            string valueName)
        {
            using (var key =
                   Registry.LocalMachine.OpenSubKey(path))
            {
                return key?.GetValue(valueName);
            }
        }

        public byte[] GetBinaryValue(
            string path,
            string valueName)
        {
            return GetValue(path, valueName) as byte[];
        }

        public IReadOnlyList<string> GetSubKeyNames(
            string path)
        {
            using (var key =
                   Registry.LocalMachine.OpenSubKey(path))
            {
                return key?
                           .GetSubKeyNames()
                           .ToList()
                       ?? new List<string>();
            }
        }
    }
}
