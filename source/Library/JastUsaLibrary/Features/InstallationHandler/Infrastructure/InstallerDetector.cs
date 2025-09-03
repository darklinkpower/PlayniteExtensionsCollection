using JastUsaLibrary.Features.InstallationHandler.Application;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.InstallationHandler.Infrastructure
{
    public class InstallerDetector : IInstallerDetector
    {
        public string ReadFileAsAscii(string filePath, int maxBytes = 1024 * 1024)
        {
            int length = (int)Math.Min(maxBytes, new FileInfo(filePath).Length);
            byte[] buffer = new byte[length];
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buffer, 0, buffer.Length);
                return Encoding.ASCII.GetString(buffer);
            }
        }

        /// <summary>
        /// Detects if the file is a self-extracting archive (ZIP or 7z) using SharpCompress.
        /// Returns true if it can be opened as a ZIP or 7z archive.
        /// </summary>
        public bool IsSfxArchive(string filePath)
        {
            try
            {
                using (var stream = File.OpenRead(filePath))
                {
                    using (var archive = ArchiveFactory.Open(stream))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Could not open file at all
            }

            return false;
        }
    }
}
