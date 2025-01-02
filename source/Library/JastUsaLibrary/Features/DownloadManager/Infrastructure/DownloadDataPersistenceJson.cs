using JastUsaLibrary.DownloadManager.Domain.Entities;
using JastUsaLibrary.Features.DownloadManager.Application;
using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.DownloadManager.Infrastructure
{
    public class DownloadDataPersistenceJson : IDownloadDataPersistence
    {
        private readonly ILogger _logger;
        private readonly string _jsonFilePath;

        public DownloadDataPersistenceJson(ILogger logger, string storageDirectory)
        {
            _logger = logger;
            _jsonFilePath = Path.Combine(storageDirectory, "downloadsData.json");
            if (!FileSystem.DirectoryExists(storageDirectory))
            {
                FileSystem.CreateDirectory(storageDirectory);
            }
        }

        public void PersistDownloadData(IEnumerable<DownloadData> downloadsData)
        {
            SaveData(downloadsData.ToList());
        }

        public void PersistDownloadData(DownloadData downloadData)
        {
            var currentData = LoadPersistedDownloads();
            var existingData = currentData.FirstOrDefault(x => x.Id == downloadData.Id);
            if (existingData != null)
            {
                currentData.Remove(existingData);
            }

            currentData.Add(downloadData);
            SaveData(currentData);
        }

        public List<DownloadData> LoadPersistedDownloads()
        {
            if (FileSystem.FileExists(_jsonFilePath))
            {
                try
                {
                    return Serialization.FromJsonFile<List<DownloadData>>(_jsonFilePath);
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Error while deserializing {_jsonFilePath}");
                    FileSystem.DeleteFileSafe(_jsonFilePath);
                }
            }

            return new List<DownloadData>();
        }

        public bool ClearPersistedDownloads()
        {
            var currentData = LoadPersistedDownloads();

            if (currentData.Any())
            {
                currentData.Clear();
                SaveData(currentData);
                return true;
            }

            return false;
        }

        private void SaveData(List<DownloadData> downloadsData)
        {
            try
            {
                var serializedData = Serialization.ToJson(downloadsData);
                FileSystem.WriteStringToFile(_jsonFilePath, serializedData, true);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error while storing serialized data to {_jsonFilePath}");
                throw;
            }
        }
    }
}
