using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using SplashScreen.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace SplashScreen.ViewModels
{
    class VideoManagerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            var caller = name;


            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        
        private IPlayniteAPI _playniteApi;
        private ICollectionView _videoItemsCollection;
        public ICollectionView VideoItemsCollection
        {
            get => _videoItemsCollection;
            set
            {
                _videoItemsCollection = value;
                OnPropertyChanged();
            }
        }
        public Dictionary<string, SourceCollection> collectionsSourceDict;
        public Dictionary<string, SourceCollection> CollectionsSourceDict
        {
            get => collectionsSourceDict;
            set
            {
                OnPropertyChanged();
            }
        }

        private string _searchString = string.Empty;
        public string SearchString
        {
            get { return _searchString; }
            set
            {
                _searchString = value;
                OnPropertyChanged();
                VideoItemsCollection.Refresh();
            }
        }

        public Uri videoSource;
        public Uri VideoSource
        {
            get { return videoSource; }
            set
            {
                videoSource = value;
                SetButtonsStatuses();
                OnPropertyChanged();
            }
        }

        private SourceCollection _selectedSourceItem;
        public SourceCollection SelectedSourceItem
        {
            get { return _selectedSourceItem; }
            set
            {
                _selectedSourceItem = value;
                VideoItemsCollection.Refresh();
            }
        }

        private VideoManagerItem _selectedItem;
        public VideoManagerItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                if (_selectedItem != null)
                {
                    VideoSource = new Uri(SelectedItem.VideoPath);
                }
                SetButtonsStatuses();
            }
        }

        private bool _addButtonIsEnabled = false;
        private bool _removeButtonIsEnabled = false;

        private void SetButtonsStatuses()
        {
            if (SelectedItem == null)
            {
                _addButtonIsEnabled = false;
                _removeButtonIsEnabled = false;
                return;
            }
            
            if (FileSystem.FileExists(SelectedItem.VideoPath))
            {
                _removeButtonIsEnabled = true;
            }
            else
            {
                _removeButtonIsEnabled = false;
            }
            _addButtonIsEnabled = true;
        }

        public RelayCommand<VideoManagerItem> AddVideoCommand
        {
            get => new RelayCommand<VideoManagerItem>((item) =>
            {
                AddVideo(item);
            }, (item) => _addButtonIsEnabled);
        }

        private bool AddVideo(VideoManagerItem videoManagerItem)
        {
            VideoSource = null;
            var videoDestinationPath = videoManagerItem.VideoPath;
            var videoSourcePath = _playniteApi.Dialogs.SelectFile("mp4|*.mp4");
            if (string.IsNullOrEmpty(videoSourcePath))
            {
                return false;
            }

            // In case source video is the same as target
            if (videoSourcePath.Equals(videoDestinationPath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var directory = Path.GetDirectoryName(videoDestinationPath);
            if (!FileSystem.DirectoryExists(directory))
            {
                FileSystem.CreateDirectory(directory);
            }

            FileSystem.CopyFile(videoSourcePath, videoDestinationPath, true);
            VideoSource = new Uri(videoDestinationPath);
            _playniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSplashScreen_IntroVideoAddedMessage"), "Splash Screen");
            return true;
        }

        public RelayCommand<VideoManagerItem> RemoveVideoCommand
        {
            get => new RelayCommand<VideoManagerItem>((item) =>
            {
                RemoveVideo(item);
            }, (item) => _removeButtonIsEnabled);
        }

        private void RemoveVideo(VideoManagerItem videoManagerItem)
        {
            VideoSource = null;
            FileSystem.DeleteFileSafe(videoManagerItem.VideoPath);
            _playniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSplashScreen_IntroVideoRemovedMessage"), "Splash Screen");
        }

        public VideoManagerViewModel(IPlayniteAPI api)
        {
            _playniteApi = api;

            var videoPathTemplate = Path.Combine(_playniteApi.Paths.ConfigurationPath, "ExtraMetadata", "{0}", "{1}", "VideoIntro.mp4");
            var videoItemsCollection = new List<VideoManagerItem>();

            // Games
            foreach (var game in _playniteApi.MainView.SelectedGames)
            {
                var item = new VideoManagerItem
                {
                    Name = game.Name,
                    VideoPath = string.Format(videoPathTemplate, "games", game.Id.ToString()),
                    SourceCollection = SourceCollection.Game
                };
                videoItemsCollection.Add(item);
            }

            // Sources
            foreach (var source in _playniteApi.Database.Sources)
            {
                var item = new VideoManagerItem
                {
                    Name = source.Name,
                    VideoPath = string.Format(videoPathTemplate, "sources", source.Id.ToString()),
                    SourceCollection = SourceCollection.Source
                };
                videoItemsCollection.Add(item);
            }

            // Sources
            foreach (var platform in _playniteApi.Database.Platforms)
            {
                var item = new VideoManagerItem
                {
                    Name = platform.Name,
                    VideoPath = string.Format(videoPathTemplate, "platforms", platform.Id.ToString()),
                    SourceCollection = SourceCollection.Platform
                };
                videoItemsCollection.Add(item);
            }

            // Playnite Modes
            videoItemsCollection.Add(new VideoManagerItem
            {
                Name = "Desktop",
                VideoPath = string.Format(videoPathTemplate, "playnite", "Desktop"),
                SourceCollection = SourceCollection.PlayniteMode
            });
            videoItemsCollection.Add(new VideoManagerItem
            {
                Name = "Fullscreen",
                VideoPath = string.Format(videoPathTemplate, "playnite", "Fullscreen"),
                SourceCollection = SourceCollection.PlayniteMode
            });

            videoItemsCollection.Sort((x, y) => x.Name.CompareTo(y.Name));
            VideoItemsCollection = CollectionViewSource.GetDefaultView(videoItemsCollection);
            VideoItemsCollection.Filter = CollectionFilter;

            collectionsSourceDict = new Dictionary<string, SourceCollection>
            {
                { ResourceProvider.GetString("LOCSplashScreen_VideoManagerCollectionSelectedGamesLabel"), SourceCollection.Game },
                { ResourceProvider.GetString("LOCSplashScreen_VideoManagerCollectionSourcesLabel"), SourceCollection.Source },
                { ResourceProvider.GetString("LOCSplashScreen_VideoManagerCollectionPlatformsLabel"), SourceCollection.Platform },
                { ResourceProvider.GetString("LOCSplashScreen_VideoManagerCollectionPlayniteModeLabel"), SourceCollection.PlayniteMode }
            };
            SelectedSourceItem = SourceCollection.Game;
        }

        private bool CollectionFilter(object item)
        {
            var videoManagerItem = item as VideoManagerItem;
            if (videoManagerItem.SourceCollection != SelectedSourceItem)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(SearchString))
            {
                var searchStringLower = SearchString.ToLower();
                if (!videoManagerItem.Name.ToLower().Contains(searchStringLower))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
