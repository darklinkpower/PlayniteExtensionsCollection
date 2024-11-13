using FlowHttp;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebViewCore.Domain.Interfaces;

namespace WebViewCore.Infrastructure
{
    public class BookmarksIconRepository : IBookmarksIconRepository
    {
        private readonly string _iconCacheDirectory;
        private readonly string _defaultIconPath;

        public BookmarksIconRepository(string iconCacheDirectory)
        {
            _iconCacheDirectory = iconCacheDirectory;
            _defaultIconPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "icon.png");

            if (!FileSystem.DirectoryExists(_iconCacheDirectory))
            {
                FileSystem.CreateDirectory(_iconCacheDirectory);
            }
        }

        public string CacheIcon(Uri addressUri)
        {
            var fileName = $"{addressUri.Host}.ico";
            var cachedIconPath = Path.Combine(_iconCacheDirectory, fileName);
            if (FileSystem.FileExists(cachedIconPath))
            {
                return fileName;
            }

            var iconDownloadUrl = string.Format(@"http://www.google.com/s2/favicons?domain={0}&sz=128", addressUri.Host);
            var downloadResult = HttpRequestFactory.GetHttpFileRequest()
                .WithUrl(iconDownloadUrl)
                .WithDownloadTo(cachedIconPath)
                .DownloadFile();

            if (downloadResult.IsSuccess && FileSystem.FileExists(cachedIconPath))
            {
                return fileName;
            }

            return string.Empty;
        }

        public bool IconExists(string iconName)
        {
            var iconPath = Path.Combine(_iconCacheDirectory, iconName);
            return FileSystem.FileExists(iconPath);
        }

        public string GetIconPath(string iconName)
        {
            if (IconExists(iconName))
            {
                return Path.Combine(_iconCacheDirectory, iconName);
            }

            return _defaultIconPath;
        }

        public string CopyIconToCache(string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath) || !FileSystem.FileExists(iconPath))
            {
                return string.Empty;
            }

            var cacheFileName = $"{Guid.NewGuid()}{Path.GetExtension(iconPath)}";
            var cacheIconPath = Path.Combine(_iconCacheDirectory, cacheFileName);
            if (FileSystem.CopyFile(iconPath, cacheIconPath))
            {
                return cacheFileName;
            }

            return string.Empty;
        }
    }
}