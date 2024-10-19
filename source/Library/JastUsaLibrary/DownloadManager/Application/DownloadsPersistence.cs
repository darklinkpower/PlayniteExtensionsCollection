using JastUsaLibrary.DownloadManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.DownloadManager.Application
{
    public class DownloadsPersistence
    {
        private readonly JastUsaLibrarySettingsViewModel _settingsViewModel;

        public DownloadsPersistence(JastUsaLibrarySettingsViewModel settingsViewModel)
        {
            _settingsViewModel = settingsViewModel;
        }

        public void PersistDownloadData(IEnumerable<DownloadData> downloadsData)
        {
            var anyItemAdded = false;
            foreach (var downloadData in downloadsData.ToList())
            {
                var itemAdded = _settingsViewModel.Settings.DownloadsData.AddMissing(downloadData);
                if (itemAdded)
                {
                    anyItemAdded = true;
                }
            }

            if (anyItemAdded)
            {
                _settingsViewModel.SaveSettings();
            }          
        }

        public void PersistDownloadData(DownloadData downloadData)
        {
            if (!_settingsViewModel.Settings.DownloadsData.Contains(downloadData))
            {
                _settingsViewModel.Settings.DownloadsData.AddMissing(downloadData);
                _settingsViewModel.SaveSettings();
            }
        }

        public List<DownloadData> LoadPersistedDownloads()
        {
            return _settingsViewModel.Settings.DownloadsData.ToList();
        }

        public bool ClearPersistedDownloads()
        {
            if (_settingsViewModel.Settings.DownloadsData.HasItems())
            {
                _settingsViewModel.Settings.DownloadsData.Clear();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}