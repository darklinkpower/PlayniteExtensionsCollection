using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginsCommon
{
    // Based on https://github.com/JosefNemec/Playnite
    internal enum FileSystemItem
    {
        File,
        Directory
    }

    internal static class FileSystem
    {
        [DllImport("kernel32.dll")]
        static extern uint GetCompressedFileSizeW([In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
        [Out, MarshalAs(UnmanagedType.U4)] out uint lpFileSizeHigh);

        [DllImport("kernel32.dll", SetLastError = true, PreserveSig = true)]
        static extern int GetDiskFreeSpaceW([In, MarshalAs(UnmanagedType.LPWStr)] string lpRootPathName,
        out uint lpSectorsPerCluster, out uint lpBytesPerSector, out uint lpNumberOfFreeClusters,
        out uint lpTotalNumberOfClusters);

        private static ILogger logger = LogManager.GetLogger();
        private const string longPathPrefix = @"\\?\";
        private const string longPathUncPrefix = @"\\?\UNC\";

        internal static void CreateDirectory(string path)
        {
            CreateDirectory(path, false);
        }

        internal static void CreateDirectory(string path, bool clean)
        {
            var directory = FixPathLength(path);
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }

            if (Directory.Exists(directory))
            {
                if (clean)
                {
                    Directory.Delete(directory, true);
                }
                else
                {
                    return;
                }
            }

            Directory.CreateDirectory(directory);
        }

        internal static void PrepareSaveFile(string path, bool deleteFile = true)
        {
            path = FixPathLength(path);
            CreateDirectory(Path.GetDirectoryName(path));
            if (deleteFile)
            {
                DeleteFile(path, true);
            }
        }

        internal static bool IsDirectoryEmpty(string path)
        {
            path = FixPathLength(path);
            if (Directory.Exists(path))
            {
                return !Directory.EnumerateFileSystemEntries(path).Any();
            }
            else
            {
                return true;
            }
        }

        internal static void DeleteFile(string path, bool includeReadonly = false)
        {
            path = FixPathLength(path);
            if (!File.Exists(path))
            {
                return;
            }

            if (includeReadonly)
            {
                var attr = File.GetAttributes(path);
                if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(path, attr ^ FileAttributes.ReadOnly);
                }
            }

            File.Delete(path);
        }

        internal static void CreateFile(string path)
        {
            path = FixPathLength(path);
            File.Create(path).Dispose();
        }

        internal static bool CopyFile(string sourcePath, string targetPath, bool overwrite = true)
        {
            sourcePath = FixPathLength(sourcePath);
            targetPath = FixPathLength(targetPath);

            try
            {
                logger.Debug($"Copying file {sourcePath} to {targetPath}");
                PrepareSaveFile(targetPath);
                File.Copy(sourcePath, targetPath, overwrite);
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error copying file {sourcePath} to {targetPath}");
                return false;
            }
        }

        internal static bool MoveFile(string sourcePath, string targetPath)
        {
            sourcePath = FixPathLength(sourcePath);
            targetPath = FixPathLength(targetPath);
            logger.Debug($"Moving file {sourcePath} to {targetPath}");
            if (sourcePath.Equals(targetPath, StringComparison.OrdinalIgnoreCase))
            {
                logger.Debug($"Source path and target path are the same: {sourcePath}");
                return false;
            }

            if (!File.Exists(sourcePath))
            {
                logger.Debug($"Source doesn't exists: {sourcePath}");
                return false;
            }

            var targetDir = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            else if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }

            File.Move(sourcePath, targetPath);
            return true;
        }

        internal static void ClearDirectory(string path)
        {
            path = FixPathLength(path);
            if (!Directory.Exists(path))
            {
                return;
            }

            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (FileInfo file in dir.GetFiles())
            {
                DeleteFile(file.FullName);
            }

            foreach (DirectoryInfo directory in dir.GetDirectories())
            {
                ClearDirectory(directory.FullName);
                DeleteDirectory(directory.FullName);
            }
        }

        internal static void DeleteDirectory(string path, bool includeReadonly = true)
        {
            path = FixPathLength(path);
            if (!Directory.Exists(path))
            {
                return;
            }

            if (includeReadonly)
            {
                foreach (var s in Directory.GetDirectories(path))
                {
                    DeleteDirectory(s, true);
                }

                foreach (var f in Directory.GetFiles(path))
                {
                    DeleteFile(f, true);
                }

                var dirAttr = File.GetAttributes(path);
                if ((dirAttr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(path, dirAttr ^ FileAttributes.ReadOnly);
                }

                Directory.Delete(path, false);
            }
            else
            {
                DeleteDirectory(path);
            }
        }

        internal static bool CanWriteToFolder(string folder)
        {
            folder = FixPathLength(folder);
            try
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                using (var stream = File.Create(Path.Combine(folder, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose))
                {
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static string ReadFileAsStringSafe(string path, int retryAttempts = 5)
        {
            path = FixPathLength(path);
            IOException ioException = null;
            for (int i = 0; i < retryAttempts; i++)
            {
                try
                {
                    return File.ReadAllText(path);
                }
                catch (IOException exc)
                {
                    logger.Debug($"Can't read from file, trying again. {path}");
                    ioException = exc;
                    Task.Delay(500).Wait();
                }
            }

            throw new IOException($"Failed to read {path}", ioException);
        }

        internal static byte[] ReadFileAsBytesSafe(string path, int retryAttempts = 5)
        {
            path = FixPathLength(path);
            IOException ioException = null;
            for (int i = 0; i < retryAttempts; i++)
            {
                try
                {
                    return File.ReadAllBytes(path);
                }
                catch (IOException exc)
                {
                    logger.Debug($"Can't read from file, trying again. {path}");
                    ioException = exc;
                    Task.Delay(500).Wait();
                }
            }

            throw new IOException($"Failed to read {path}", ioException);
        }

        internal static Stream CreateWriteFileStreamSafe(string path, int retryAttempts = 5)
        {
            path = FixPathLength(path);
            IOException ioException = null;
            for (int i = 0; i < retryAttempts; i++)
            {
                try
                {
                    return new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
                }
                catch (IOException exc)
                {
                    logger.Debug($"Can't open write file stream, trying again. {path}");
                    ioException = exc;
                    Task.Delay(500).Wait();
                }
            }

            throw new IOException($"Failed to read {path}", ioException);
        }

        internal static Stream OpenReadFileStreamSafe(string path, int retryAttempts = 5)
        {
            path = FixPathLength(path);
            IOException ioException = null;
            for (int i = 0; i < retryAttempts; i++)
            {
                try
                {
                    return new FileStream(path, FileMode.Open, FileAccess.Read);
                }
                catch (IOException exc)
                {
                    logger.Debug($"Can't open read file stream, trying again. {path}");
                    ioException = exc;
                    Task.Delay(500).Wait();
                }
            }

            throw new IOException($"Failed to read {path}", ioException);
        }

        internal static void WriteStringToFile(string path, string content, bool useUtf8 = false)
        {
            path = FixPathLength(path);
            PrepareSaveFile(path);
            if (useUtf8)
            {
                File.WriteAllText(path, content, Encoding.UTF8);
            }
            else
            {
                File.WriteAllText(path, content);
            }
        }

        internal static string ReadStringFromFile(string path, bool useUtf8 = false)
        {
            path = FixPathLength(path);
            if (useUtf8)
            {
                return File.ReadAllText(path, Encoding.UTF8);
            }
            else
            {
                return File.ReadAllText(path);
            }
        }

        internal static void WriteStringToFileSafe(string path, string content, int retryAttempts = 5)
        {
            path = FixPathLength(path);
            IOException ioException = null;
            for (int i = 0; i < retryAttempts; i++)
            {
                try
                {
                    PrepareSaveFile(path);
                    File.WriteAllText(path, content);
                    return;
                }
                catch (IOException exc)
                {
                    logger.Error(exc, $"Can't write to a file, trying again. {path}");
                    ioException = exc;
                    Task.Delay(500).Wait();
                }
            }

            //throw new IOException($"Failed to write to {path}", ioException);
        }

        internal static void DeleteFileSafe(string path, int retryAttempts = 5)
        {
            path = FixPathLength(path);
            if (!File.Exists(path))
            {
                return;
            }

            IOException ioException = null;
            for (int i = 0; i < retryAttempts; i++)
            {
                try
                {
                    File.Delete(path);
                    return;
                }
                catch (IOException exc)
                {
                    logger.Debug($"Can't delete file, trying again. {path}");
                    ioException = exc;
                    Task.Delay(500).Wait();
                }
                catch (UnauthorizedAccessException exc)
                {
                    logger.Error(exc, $"Can't delete file, UnauthorizedAccessException. {path}");
                    return;
                }
            }

            throw new IOException($"Failed to delete {path}", ioException);
        }

        internal static long GetFileSize(string path)
        {
            path = FixPathLength(path);
            return new FileInfo(path).Length;
        }

        
        internal static long GetFileSizeOnDisk(string path)
        {
            return GetFileSizeOnDisk(new FileInfo(FixPathLength(path)));
        }

        
        internal static long GetFileSizeOnDisk(FileInfo info)
        {
            // From https://stackoverflow.com/a/3751135
            uint dummy, sectorsPerCluster, bytesPerSector;
            int result = GetDiskFreeSpaceW(info.Directory.Root.FullName, out sectorsPerCluster, out bytesPerSector, out dummy, out dummy);
            if (result == 0) throw new System.ComponentModel.Win32Exception();
            uint clusterSize = sectorsPerCluster * bytesPerSector;
            uint hosize;
            uint losize = GetCompressedFileSizeW(info.FullName, out hosize);
            long size;
            size = (long)hosize << 32 | losize;
            return ((size + clusterSize - 1) / clusterSize) * clusterSize;
        }

        internal static long GetDirectorySize(string path)
        {
            return GetDirectorySize(new DirectoryInfo(FixPathLength(path)));
        }

        private static long GetDirectorySize(DirectoryInfo dir)
        {
            try
            {
                long size = 0;
                // Add file sizes.
                FileInfo[] fis = dir.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    size += fi.Length;
                }

                // Add subdirectory sizes.
                DirectoryInfo[] dis = dir.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                    size += GetDirectorySize(di);
                }
                return size;
            }
            catch
            {
                return 0;
            }
        }

        internal static long GetDirectorySizeOnDisk(string path)
        {
            return GetDirectorySizeOnDisk(new DirectoryInfo(FixPathLength(path)));
        }

        internal static long GetDirectorySizeOnDisk(DirectoryInfo dirInfo)
        {
            long size = 0;

            // Add file sizes.
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                size += GetFileSizeOnDisk(file);
            }

            // Add subdirectory sizes.
            foreach (DirectoryInfo directory in dirInfo.GetDirectories())
            {
                size += GetDirectorySizeOnDisk(directory);
            }

            return size;
        }

        internal static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs = true, bool overwrite = true)
        {
            sourceDirName = FixPathLength(sourceDirName);
            destDirName = FixPathLength(destDirName);
            var dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            var files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, overwrite);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        internal static bool DirectoryExists(string path)
        {
            return Directory.Exists(FixPathLength(path));
        }

        internal static bool FileExists(string path)
        {
            return File.Exists(FixPathLength(path));
        }

        internal static string FixPathLength(string path)
        {
            // Relative paths don't support long paths
            // https://docs.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation?tabs=cmd
            if (!Paths.IsFullPath(path))
            {
                return path;
            }

            if (path.Length >= 258 && !path.StartsWith(longPathPrefix))
            {
                if (path.StartsWith(@"\\"))
                {
                    return longPathUncPrefix + path.Substring(2);
                }
                else
                {
                    return longPathPrefix + path;
                }
            }

            return path;
        }
    }
}
