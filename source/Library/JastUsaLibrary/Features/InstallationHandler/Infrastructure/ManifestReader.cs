using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JastUsaLibrary.Features.InstallationHandler.Infrastructure
{
    internal static class ManifestReader
    {
        // Win32 APIs
        private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr FindResource(IntPtr hModule, IntPtr lpID, IntPtr lpType);

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr LockResource(IntPtr hResData);

        [DllImport("kernel32", SetLastError = true)]
        private static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        // Constants for resource type RT_MANIFEST = 24
        private static readonly IntPtr RT_MANIFEST = new IntPtr(24);

        // Common manifest resource id is 1 but could be other ids; we'll try 1 then any numeric resource.
        public static string ReadManifestXml(string exePath)
        {
            IntPtr hModule = IntPtr.Zero;
            try
            {
                hModule = LoadLibraryEx(exePath, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
                if (hModule == IntPtr.Zero)
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "LoadLibraryEx failed");
                }

                // Try resource id 1 first (very common)
                IntPtr hResInfo = FindResource(hModule, new IntPtr(1), RT_MANIFEST);

                // If not found, try enumerating numeric resource IDs 1..10 as fallback
                if (hResInfo == IntPtr.Zero)
                {
                    for (int id = 1; id <= 10; id++)
                    {
                        hResInfo = FindResource(hModule, new IntPtr(id), RT_MANIFEST);
                        if (hResInfo != IntPtr.Zero)
                        {
                            break;
                        }
                    }
                }

                if (hResInfo == IntPtr.Zero)
                {
                    // Try to find any manifest by searching common names isn't possible via FindResource with string names here.
                    return null; // no manifest found
                }

                uint size = SizeofResource(hModule, hResInfo);
                if (size == 0)
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "SizeofResource failed");
                }

                IntPtr hResData = LoadResource(hModule, hResInfo);
                if (hResData == IntPtr.Zero)
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "LoadResource failed");
                }

                IntPtr pLocked = LockResource(hResData);
                if (pLocked == IntPtr.Zero)
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "LockResource failed");
                }

                // Copy bytes
                byte[] buffer = new byte[size];
                Marshal.Copy(pLocked, buffer, 0, (int)size);

                // Manifest usually UTF-8 or UTF-16 — let's try detect BOM first
                string xml;
                if (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                {
                    xml = Encoding.UTF8.GetString(buffer);
                }
                else if (buffer.Length >= 2 && buffer[0] == 0xFF && buffer[1] == 0xFE)
                {
                    xml = Encoding.Unicode.GetString(buffer);
                }
                else if (buffer.Length >= 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
                {
                    xml = Encoding.BigEndianUnicode.GetString(buffer);
                }
                else
                {
                    // assume UTF-8 by default
                    xml = Encoding.UTF8.GetString(buffer);
                }

                return xml;
            }
            finally
            {
                if (hModule != IntPtr.Zero)
                {
                    FreeLibrary(hModule);
                }
            }
        }

        public static void PrintManifestFields(string exePath)
        {
            var xml = ReadManifestXml(exePath);
            if (string.IsNullOrEmpty(xml))
            {
                Console.WriteLine("No embedded manifest found.");
                return;
            }

            Console.WriteLine("Raw manifest length: " + xml.Length);
            // Parse with LINQ to XML (XDocument)
            try
            {
                var doc = XDocument.Parse(xml);
                // assemblyIdentity
                var ns = doc.Root?.Name.Namespace; // manifest uses default namespace often
                var assemblyIdentity = doc.Root?.Element(ns + "assemblyIdentity");
                if (assemblyIdentity != null)
                {
                    foreach (var attr in assemblyIdentity.Attributes())
                    {
                        Console.WriteLine($"assemblyIdentity @{attr.Name.LocalName} = {attr.Value}");
                    }
                }
                // Look for product name in common places: <description> or custom <assembly><trustInfo>... or <assembly><description>
                var description = doc.Root?.Element(ns + "description");
                if (description != null) Console.WriteLine("description: " + description.Value);

                // some manifests use <assembly><assemblyIdentity .../> <description> or nested tags.
                // "productname" might be custom in a <assembly> or in a resource; search whole document for element named productname
                var prod = doc.Descendants().FirstOrDefault(e => string.Equals(e.Name.LocalName, "productname", StringComparison.OrdinalIgnoreCase));
                if (prod != null) Console.WriteLine("productname element: " + prod.Value);

                // Print full manifest root name for debugging
                Console.WriteLine("Manifest root element: " + (doc.Root?.Name.ToString() ?? "<none>"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to parse manifest XML: " + ex.Message);
            }
        }
    }
}
