using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using PluginsCommon.Web;
using PluginsCommon;

namespace GamesSizeCalculator.Steam
{
    public interface ICachedFile
    {
        string GetFileContents();
        void RefreshCache();
    }

    public class CachedFileDownloader : ICachedFile
    {
        public string OnlinePath { get; }
        public string LocalPath { get; }
        public TimeSpan MaxCacheAge { get; }
        public Encoding Encoding { get; }
        public string PackagedFallbackPath { get; }

        public CachedFileDownloader(string onlinePath, string localPath, TimeSpan maxCacheAge, Encoding encoding = null, string packagedFallbackPath = null)
        {
            OnlinePath = onlinePath;
            LocalPath = Environment.ExpandEnvironmentVariables(localPath);
            MaxCacheAge = maxCacheAge;
            Encoding = encoding;
            PackagedFallbackPath = packagedFallbackPath;
        }

        private bool CopyFileFromPackagedFallback()
        {
            if (PackagedFallbackPath.IsNullOrWhiteSpace())
            {
                return false;
            }

            FileInfo packagedFallbackFile = new FileInfo(PackagedFallbackPath);

            if (!packagedFallbackFile.Exists)
            {
                return false;
            }

            FileSystem.CopyFile(PackagedFallbackPath, LocalPath, true);
            return true;
        }

        private bool PackagedFallbackIsNewerThan(FileInfo f)
        {
            if (PackagedFallbackPath.IsNullOrWhiteSpace())
            {
                return false;
            }

            FileInfo packagedFallbackFile = new FileInfo(PackagedFallbackPath);
            if (!packagedFallbackFile.Exists || !f.Exists)
            {
                return false;
            }

            return packagedFallbackFile.LastWriteTime > f.LastWriteTime;
        }

        public string GetFileContents()
        {
            var f = new FileInfo(LocalPath);
            if ((!f.Exists || PackagedFallbackIsNewerThan(f)) && CopyFileFromPackagedFallback())
            {
                f.Refresh();
            }

            if (!f.Exists || f.LastWriteTime + MaxCacheAge < DateTime.Now)
            {
                RefreshCache();
            }
            if (Encoding == null)
            {
                return FileSystem.ReadStringFromFile(LocalPath);
            }
            else
            {
                return File.ReadAllText(LocalPath, Encoding);
            }
        }

        public void RefreshCache()
        {
            HttpDownloader.DownloadFile(OnlinePath, LocalPath);
        }
    }
}