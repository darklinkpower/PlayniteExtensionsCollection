using Playnite.SDK.Data;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using VNDBMetadata.ApiConstants;
using FlowHttp;

namespace VNDBMetadata
{
    internal class VndbCacheManager
    {
        private readonly VNDBMetadataSettings settings;
        private readonly string pluginDataPath;
        private readonly VNDBMetadata plugin;
        private Dictionary<int, string> tagsCache = new Dictionary<int, string>();
        private Dictionary<int, string> traitsCache = new Dictionary<int, string>();
        private const string TagsFileName = "tags.json";
        private const string TraitsFileName = "traits.json";
        private const string TagsCompressedFileName = "tags.gz";
        private const string TraitsCompressedFileName = "traits.gz";
        private const int DaysToUpdateDatabase = 5;

        public VndbCacheManager(VNDBMetadata plugin, VNDBMetadataSettings settings, string pluginDataPath)
        {
            this.settings = settings;
            this.pluginDataPath = pluginDataPath;
            this.plugin = plugin;
            this.settings = settings;
            this.pluginDataPath = pluginDataPath;
            this.plugin = plugin;
            if (ShouldUpdateDatabase())
            {
                UpdateDatabase();
            }

            InitializeCaches();
        }

        private bool ShouldUpdateDatabase()
        {
            var tagsFilePath = Path.Combine(pluginDataPath, TagsFileName);
            var traitsFilePath = Path.Combine(pluginDataPath, TraitsFileName);
            var shouldUpdate = (DateTime.Now - settings.LastDatabaseUpdate) > TimeSpan.FromDays(DaysToUpdateDatabase)
                || !FileSystem.FileExists(tagsFilePath)
                || !FileSystem.FileExists(traitsFilePath);

            return shouldUpdate;
        }

        private void UpdateDatabase()
        {
            var tagsCompressedPath = Path.Combine(pluginDataPath, TagsCompressedFileName);
            var traitsCompressedPath = Path.Combine(pluginDataPath, TraitsCompressedFileName);

            var tagsDownloadSuccess = DownloadAndDecompress(DatabaseDumps.TagsUrl, tagsCompressedPath, TagsFileName);
            var traitsDownloadSuccess = DownloadAndDecompress(DatabaseDumps.TraitsUrl, traitsCompressedPath, TraitsFileName);

            if (tagsDownloadSuccess && traitsDownloadSuccess)
            {
                settings.LastDatabaseUpdate = DateTime.Now;
                plugin.SavePluginSettings(settings);
            }
        }

        private bool DownloadAndDecompress(string sourceUrl, string compressedPath, string outputPath)
        {
            var downloadResult = HttpRequestFactory.GetHttpRequest()
                .WithUrl(sourceUrl).WithDownloadTo(compressedPath)
                .DownloadFile();
            if (downloadResult.IsSuccess)
            {
                DecompressGZipFile(compressedPath, outputPath);
                FileSystem.DeleteFile(compressedPath);
                return true;
            }

            return false;
        }

        private void DecompressGZipFile(string compressedPath, string outputPath)
        {
            using (FileStream input = File.OpenRead(compressedPath))
            {
                using (FileStream output = File.Create(outputPath))
                {
                    using (GZipStream decompressor = new GZipStream(input, CompressionMode.Decompress))
                    {
                        decompressor.CopyTo(output);
                    }
                }
            }
        }

        private void InitializeCaches()
        {
            var tagsFilePath = Path.Combine(pluginDataPath, TagsFileName);
            var traitsFilePath = Path.Combine(pluginDataPath, TraitsFileName);

            var tagsList = FileSystem.FileExists(tagsFilePath) ? Serialization.FromJsonFile<List<Models.Tag>>(tagsFilePath) : new List<Models.Tag>();
            var traitsList = FileSystem.FileExists(traitsFilePath) ? Serialization.FromJsonFile<List<Models.Trait>>(traitsFilePath) : new List<Models.Trait>();

            tagsCache = tagsList.ToDictionary(x => x.Id, x => x.Name);
            traitsCache = traitsList.ToDictionary(x => x.Id, x => x.Name);
        }

        public string GetTagNameById(int id)
        {
            if (tagsCache.TryGetValue(id, out var name))
            {
                return name;
            }

            return null;
        }

        public string GetTraitNameById(int id)
        {
            if (traitsCache.TryGetValue(id, out var name))
            {
                return name;
            }

            return null;
        }

    }
}