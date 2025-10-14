using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.InstallationHandler.Application
{
    public sealed class ExecutableMetadata
    {
        public string ProductName { get; }
        public string CompanyName { get; }
        public string FileDescription { get; }
        public string AssemblyName { get; }
        public string AssemblyVersion { get; }
        public IReadOnlyDictionary<string, string> ManifestAssemblyIdentity { get; }

        public ExecutableMetadata(
            string productName,
            string companyName,
            string fileDescription,
            string assemblyName,
            string assemblyVersion,
            IReadOnlyDictionary<string, string> manifestAssemblyIdentity)
        {
            ProductName = productName ?? string.Empty;
            CompanyName = companyName ?? string.Empty;
            FileDescription = fileDescription ?? string.Empty;
            AssemblyName = assemblyName ?? string.Empty;
            AssemblyVersion = assemblyVersion ?? string.Empty;
            ManifestAssemblyIdentity = manifestAssemblyIdentity
                ?? new Dictionary<string, string>();
        }

        public override string ToString()
        {
            return $"{ProductName} ({AssemblyName} {AssemblyVersion})";
        }
    }
}
