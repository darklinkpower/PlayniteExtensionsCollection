using JastUsaLibrary.Views;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace JastUsaLibrary.ViewModels
{
    public class GameUninstallWindowViewModel : ObservableObject
    {
        private readonly Window _window;

        public Game Game { get; }
        public string InstallDirectory { get; }

        private readonly HashSet<string> nonSavesExtensions;

        public class FileSystemItem : ObservableObject
        {
            public string Name { get; set; }
            public string FullPath { get; set; }
            public bool IsDirectory { get; set; }

            private bool _isChecked;
            public bool IsChecked { get => _isChecked; set => SetValue(ref _isChecked, value); }
        }

        public bool FilesDeleted { get; private set; } = false;

        public ObservableCollection<FileSystemItem> FileSystemItems { get; } = new ObservableCollection<FileSystemItem>();

        public GameUninstallWindowViewModel(Game game, Window window)
        {
            Game = game;
            InstallDirectory = game.InstallDirectory;
            nonSavesExtensions = new HashSet<string>() { ".bat", ".exe", ".url" };
            LoadItems();
            _window = window;
        }

        private void LoadItems()
        {
            FileSystemItems.Clear();
            var rootItems = Directory.GetFileSystemEntries(InstallDirectory)
                .Select(path =>
                {
                    var isDirectory = IsDirectory(path);
                    var name = isDirectory ? Path.GetFileName(path) + "\\" : Path.GetFileName(path);
                    var isSaveItem = IsFileNameSaveItem(name, isDirectory);
                    return new FileSystemItem
                    {
                        Name = name,
                        FullPath = path,
                        IsDirectory = isDirectory,
                        IsChecked = !isSaveItem
                    };
                })
                .OrderBy(item => !item.IsDirectory)
                .ThenBy(item => item.Name);

            foreach (var item in rootItems)
            {
                FileSystemItems.Add(item);
            }
        }

        private bool IsFileNameSaveItem(string fileName, bool isDirectory)
        {
            if (!fileName.Contains("sav", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            
            if (isDirectory)
            {
                return true;
            }

            var extension = Path.GetExtension(fileName);
            if (!extension.IsNullOrEmpty() && nonSavesExtensions.Contains(extension.ToLower()))
            {
                return false;
            }

            return true;
        }

        private static bool IsDirectory(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }

        private bool DeleteItems(GlobalProgressActionArgs progressArgs, CancellationToken cancelToken)
        {
            var finishedDeleting = true;
            var itemsToDelete = FileSystemItems.Where(item => item.IsChecked).ToList();
            foreach (var item in itemsToDelete)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    finishedDeleting = false;
                    break;
                }

                progressArgs.CurrentProgressValue++;
                var itemName = item.IsDirectory ? $"{item.FullPath}\\" : item.FullPath;
                progressArgs.Text = $"{progressArgs.CurrentProgressValue}/{progressArgs.ProgressMaxValue}.\n\n{itemName}";
                if (item.IsDirectory)
                {
                    FileSystem.DeleteDirectory(item.FullPath);
                }
                else
                {
                    FileSystem.DeleteFileSafe(item.FullPath);
                }
            }

            if (FileSystemItems.Count == itemsToDelete.Count)
            {
                FileSystem.DeleteDirectory(InstallDirectory);
            }

            return finishedDeleting;
        }

        public RelayCommand DeleteItemsAndCloseCommand
        {
            get => new RelayCommand(() =>
            {
                var progressOptions = new GlobalProgressOptions(ResourceProvider.GetString("Deleting game files..."), true)
                {
                    IsIndeterminate = false,
                };

                API.Instance.Dialogs.ActivateGlobalProgress((a) =>
                {
                    a.ProgressMaxValue = FileSystemItems.Count;
                    var finishedDeleting = DeleteItems(a, a.CancelToken);
                    FilesDeleted = finishedDeleting;
                }, progressOptions);

                _window?.Close();
            });
        }

        private void SelectAllItems()
        {
            foreach (var item in FileSystemItems)
            {
                if (!item.IsChecked)
                {
                    item.IsChecked = true;
                }
            }

            NotifyCommandsPropertyChanged();
        }

        private void UnselectAllItems()
        {
            foreach (var item in FileSystemItems)
            {
                if (item.IsChecked)
                {
                    item.IsChecked = false;
                }
            }

            NotifyCommandsPropertyChanged();
        }

        private void NotifyCommandsPropertyChanged()
        {
            OnPropertyChanged(nameof(SelectAllItemsCommand));
            OnPropertyChanged(nameof(UnselectAllItemsCommand));
        }

        public RelayCommand CancelCommand
        {
            get => new RelayCommand(() =>
            {
                _window?.Close();
            });
        }

        public RelayCommand SelectAllItemsCommand
        {
            get => new RelayCommand(() =>
            {
                SelectAllItems();
            }, () => FileSystemItems.Any(x => !x.IsChecked));
        }

        public RelayCommand UnselectAllItemsCommand
        {
            get => new RelayCommand(() =>
            {
                UnselectAllItems();
            }, () => FileSystemItems.Any(x => x.IsChecked));
        }
    }
}