using Playnite.SDK;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace InstallationStatusUpdater.Application
{
    public class DirectoryWatcherService : IDisposable
    {
        private readonly ILogger _logger;
        private readonly InstallationStatusUpdaterSettingsViewModel _settings;
        private readonly NotifyFilters _notifyFilters;
        private readonly List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();

        /// <summary>
        /// Called when a directory change is detected.
        /// </summary>
        public Action OnTrigger { get; set; }

        public DirectoryWatcherService(
            ILogger logger,
            InstallationStatusUpdaterSettingsViewModel settings,
            NotifyFilters? notifyFilters = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _notifyFilters = notifyFilters ?? (NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Size);
        }

        public void StartWatching()
        {
            DisposeWatchers();
            if (!_settings.Settings.UpdateStatusOnDirChanges || !_settings.Settings.DetectionDirectories.Any())
            {
                return;
            }

            foreach (var directory in _settings.Settings.DetectionDirectories)
            {
                if (!directory.Enabled)
                {
                    continue;
                }

                if (!FileSystem.DirectoryExists(directory.DirectoryPath))
                {
                    _logger.Warn($"[DirectoryWatcher] Directory does not exist: {directory.DirectoryPath}");
                    continue;
                }

                var watcher = new FileSystemWatcher(directory.DirectoryPath)
                {
                    NotifyFilter = _notifyFilters,
                    Filter = "*.*",
                    IncludeSubdirectories = directory.ScanSubDirs,
                    EnableRaisingEvents = true
                };

                watcher.Changed += (_, e) => OnWatcherEvent(e.FullPath, e);
                watcher.Created += (_, e) => OnWatcherEvent(e.FullPath, e);
                watcher.Deleted += (_, e) => OnWatcherEvent(e.FullPath, e);
                watcher.Renamed += (_, e) => OnWatcherEvent(e.FullPath, e);

                watchers.Add(watcher);
            }
        }

        private void OnWatcherEvent(string path, FileSystemEventArgs e)
        {
            _logger.Info($"[DirectoryWatcher] Change detected at path: {path}, ChangeType: {e.ChangeType}");
            OnTrigger?.Invoke();
        }

        public void DisposeWatchers()
        {
            foreach (var watcher in watchers)
            {
                watcher.Dispose();
            }

            watchers.Clear();
        }

        public void Dispose()
        {
            DisposeWatchers();
        }
    }
}