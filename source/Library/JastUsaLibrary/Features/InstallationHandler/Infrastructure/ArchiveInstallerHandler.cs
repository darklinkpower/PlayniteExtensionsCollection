using JastUsaLibrary.Features.InstallationHandler.Application;
using JastUsaLibrary.Features.InstallationHandler.Domain;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.InstallationHandler.Infrastructure
{
    public class ArchiveInstallerHandler : IInstallerHandler
    {
        private readonly IInstallerDetector _detector;
        public InstallerType Type => InstallerType.Archive;

        public ArchiveInstallerHandler(IInstallerDetector detector)
        {
            _detector = detector;
        }

        public bool CanHandle(string filePath, string content)
        {
            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            if (fileExtension == ".zip" || fileExtension == ".7z")
            {
                return true;
            }

            if (fileExtension == ".exe" && _detector.IsSfxArchive(filePath))
            {
                return true;
            }

            return false;
        }

        public void Install(InstallRequest request)
        {
            var outputDir = request.TargetDirectory ?? Path.Combine(Path.GetDirectoryName(request.FilePath), Path.GetFileNameWithoutExtension(request.FilePath));
            Directory.CreateDirectory(outputDir);
            if (!TryExtractArchive(request.FilePath, outputDir))
            {
                throw new InvalidOperationException("Cannot extract archive: " + request.FilePath);
            }
        }

        private bool TryExtractArchive(string filePath, string outputDir)
        {
            try
            {
                using (var stream = File.OpenRead(filePath))
                {
                    using (var archive = ArchiveFactory.Open(stream))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (!entry.IsDirectory)
                            {
                                entry.WriteToDirectory(outputDir, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                            }
                        }

                        return true;
                    }
                }
            }
            catch
            {
                // Failed to extract (not recognized or corrupt)
                return false;
            }
        }

    }
}
