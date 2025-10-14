using PluginsCommon;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JastUsaLibrary
{
    public static class CompressionUtility
    {
        public class ExtractionResult
        {
            public bool Success { get; set; }
            public List<string> ExtractedFiles { get; } = new List<string>();
        }


        public static ExtractionResult ExtractZipFile(
            string filePath,
            string extractDirectory,
            CancellationToken cancellationToken = default)
        {
            if (!FileSystem.DirectoryExists(extractDirectory))
            {
                FileSystem.CreateDirectory(extractDirectory);
            }

            var result = new ExtractionResult { Success = true };

            using (var archive = ZipArchive.Open(filePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        result.Success = false;
                        break;
                    }

                    if (!entry.IsDirectory)
                    {
                        var destinationPath = Path.Combine(extractDirectory, entry.Key);
                        var parentDirectory = Path.GetDirectoryName(destinationPath);
                        if (!FileSystem.DirectoryExists(parentDirectory))
                        {
                            FileSystem.CreateDirectory(parentDirectory);
                        }

                        entry.WriteToFile(destinationPath);
                        result.ExtractedFiles.Add(destinationPath);
                    }
                    else
                    {
                        var directoryPath = Path.Combine(extractDirectory, entry.Key);
                        if (!FileSystem.DirectoryExists(directoryPath))
                        {
                            FileSystem.CreateDirectory(directoryPath);
                        }
                    }
                }
            }

            return result;
        }

        public static ExtractionResult ExtractRarFile(
            string downloadPath,
            string extractDirectory,
            CancellationToken cancellationToken = default)
        {
            if (!FileSystem.DirectoryExists(extractDirectory))
            {
                FileSystem.CreateDirectory(extractDirectory);
            }

            var result = new ExtractionResult { Success = true };

            using (var archive = RarArchive.Open(downloadPath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        result.Success = false;
                        break;
                    }

                    if (!entry.IsDirectory)
                    {
                        var destinationPath = Path.Combine(extractDirectory, entry.Key);
                        var parentDirectory = Path.GetDirectoryName(destinationPath);
                        if (!FileSystem.DirectoryExists(parentDirectory))
                        {
                            FileSystem.CreateDirectory(parentDirectory);
                        }

                        entry.WriteToFile(destinationPath);
                        result.ExtractedFiles.Add(destinationPath);
                    }
                    else
                    {
                        var directoryPath = Path.Combine(extractDirectory, entry.Key);
                        if (!FileSystem.DirectoryExists(directoryPath))
                        {
                            FileSystem.CreateDirectory(directoryPath);
                        }
                    }
                }
            }

            return result;
        }


    }
}