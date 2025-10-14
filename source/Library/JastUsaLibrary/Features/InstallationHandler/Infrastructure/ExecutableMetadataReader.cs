using JastUsaLibrary.Features.InstallationHandler.Application;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JastUsaLibrary.Features.InstallationHandler.Infrastructure
{
    public sealed class ExecutableMetadataReader : IExecutableMetadataReader
    {
        public ExecutableMetadata ReadMetadata(string exePath)
        {
            string productName = string.Empty;
            string companyName = string.Empty;
            string fileDescription = string.Empty;
            string assemblyName = string.Empty;
            string assemblyVersion = string.Empty;
            IReadOnlyDictionary<string, string> manifestIdentity = new Dictionary<string, string>();

            // 1) FileVersionInfo
            var info = FileVersionInfo.GetVersionInfo(exePath);
            productName = info.ProductName ?? string.Empty;
            companyName = info.CompanyName ?? string.Empty;
            fileDescription = info.FileDescription ?? string.Empty;

            // 2) Managed Assembly info
            try
            {
                var asmName = AssemblyName.GetAssemblyName(exePath);
                assemblyName = asmName.Name ?? string.Empty;
                assemblyVersion = asmName.Version?.ToString() ?? string.Empty;
            }
            catch
            {
                //Skip for non-managed executables
            }

            // 3) Manifest info
            var xml = ManifestReader.ReadManifestXml(exePath);
            if (!string.IsNullOrEmpty(xml))
            {
                try
                {
                    var doc = XDocument.Parse(xml);
                    var ns = doc.Root?.Name.Namespace;
                    var identity = doc.Root?.Element(ns + "assemblyIdentity");
                    if (identity != null)
                    {
                        manifestIdentity = identity.Attributes()
                            .ToDictionary(a => a.Name.LocalName, a => a.Value, StringComparer.OrdinalIgnoreCase);
                    }
                }
                catch
                {
                    /* Ignore malformed manifest */
                }
            }

            return new ExecutableMetadata(
                productName,
                companyName,
                fileDescription,
                assemblyName,
                assemblyVersion,
                manifestIdentity
            );
        }
    }
}
