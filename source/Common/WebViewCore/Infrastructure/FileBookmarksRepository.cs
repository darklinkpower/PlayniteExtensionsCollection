using Playnite.SDK.Data;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebViewCore.Domain.Entities;
using WebViewCore.Domain.Interfaces;

namespace WebViewCore.Infrastructure
{
    public class FileBookmarksRepository : IBookmarksRepository
    {
        private readonly string _cacheFilePath;
        private readonly object _lock = new object();
        public string Id { get; }

        public FileBookmarksRepository(string id, string cacheDirectory)
        {
            _cacheFilePath = Path.Combine(cacheDirectory, $"{id}_Bookmarks.Json");
            Id = id;
        }

        public List<BookmarkInternal> LoadBookmarks()
        {
            lock (_lock)
            {
                if (!FileSystem.FileExists(_cacheFilePath))
                {
                    return new List<BookmarkInternal>();
                }

                try
                {
                    return Serialization.FromJsonFile<List<BookmarkInternal>>(_cacheFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading bookmarks: {ex.Message}");
                    FileSystem.DeleteFileSafe(_cacheFilePath);
                    return new List<BookmarkInternal>();
                }
            }
        }

        public void SaveBookmarks(List<BookmarkInternal> bookmarks)
        {
            lock (_lock)
            {
                try
                {
                    var json = Serialization.ToJson(bookmarks);
                    FileSystem.WriteStringToFile(_cacheFilePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving bookmarks: {ex.Message}");
                }
            }
        }

        public void ClearBookmarks()
        {
            lock (_lock)
            {
                var emptyBookmarks = new List<BookmarkInternal>();
                try
                {
                    var json = Serialization.ToJson(emptyBookmarks);
                    FileSystem.WriteStringToFile(_cacheFilePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error clearing bookmarks: {ex.Message}");
                }
            }
        }
    }

}