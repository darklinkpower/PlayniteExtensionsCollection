using Playnite.SDK.Data;
using PluginsCommon;
using SteamShortcuts.Application.Interfaces;
using SteamShortcuts.Application.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamShortcuts.Infrastructure.Persistence
{
    public class JsonSupportedFeaturesCacheRepository : ISupportedFeaturesCacheRepository
    {
        private readonly string _filePath;
        private readonly object _lock = new object();

        private Dictionary<string, SupportedFeaturesCacheEntry> _entries;

        public JsonSupportedFeaturesCacheRepository(string filePath)
        {
            _filePath = filePath;
            _entries = Load();
        }

        public SupportedFeaturesCacheEntry Get(string steamId)
        {
            if (string.IsNullOrEmpty(steamId))
            {
                return null;
            }

            lock (_lock)
            {
                if (_entries.TryGetValue(steamId, out var entry))
                {
                    return entry;
                }

                return null;
            }
        }

        public void Save(SupportedFeaturesCacheEntry entry)
        {
            if (entry is null || entry.SteamId.IsNullOrEmpty())
            {
                return;
            }

            lock (_lock)
            {
                _entries[entry.SteamId] = entry;
                Persist();
            }
        }

        private Dictionary<string, SupportedFeaturesCacheEntry> Load()
        {
            if (!File.Exists(_filePath))
            {
                return new Dictionary<string, SupportedFeaturesCacheEntry>();
            }

            try
            {
                var json = File.ReadAllText(_filePath);
                var entries = Serialization.FromJson<Dictionary<string, SupportedFeaturesCacheEntry>>(json);

                return entries ?? new Dictionary<string, SupportedFeaturesCacheEntry>();
            }
            catch
            {
                // Ignore corrupted or unreadable cache files.
                return new Dictionary<string, SupportedFeaturesCacheEntry>();
            }
        }

        private void Persist()
        {
            var directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrEmpty(directory))
            {
                FileSystem.CreateDirectory(directory);
            }

            var json = Serialization.ToJson(_entries);
            var temporaryFile = _filePath + ".tmp";

            try
            {
                File.WriteAllText(
                    temporaryFile,
                    json,
                    new UTF8Encoding(false));

                if (File.Exists(_filePath))
                {
                    File.Replace(temporaryFile, _filePath, null);
                }
                else
                {
                    File.Move(temporaryFile, _filePath);
                }
            }
            finally
            {
                if (File.Exists(temporaryFile))
                {
                    File.Delete(temporaryFile);
                }
            }
        }
    }
}
