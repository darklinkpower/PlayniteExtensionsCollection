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
        public static bool ExtractZipFile(string filePath, string extractDirectory, CancellationToken cancellationToken = default)
        {
            if (!FileSystem.DirectoryExists(extractDirectory))
            {
                FileSystem.CreateDirectory(extractDirectory);
            }
            
            var extractionFinished = true;
            using (var archive = ZipArchive.Open(filePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        extractionFinished = false;
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

            return extractionFinished;
        }

        public static bool ExtractRarFile(string downloadPath, string extractDirectory, CancellationToken cancellationToken = default)
        {
            if (!FileSystem.DirectoryExists(extractDirectory))
            {
                FileSystem.CreateDirectory(extractDirectory);
            }

            var extractionFinished = true;
            using (var archive = RarArchive.Open(downloadPath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        extractionFinished = false;
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

            return extractionFinished;
        }


    }
}