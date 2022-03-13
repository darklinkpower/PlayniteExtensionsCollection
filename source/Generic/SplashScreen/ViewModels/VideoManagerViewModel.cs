using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using SplashScreen.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
        
        private IPlayniteAPI PlayniteApi;
        private ICollectionView videoItemsCollection;
        public ICollectionView VideoItemsCollection
        {
            get => videoItemsCollection;
            set
            {
                videoItemsCollection = value;
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

        private string searchString = string.Empty;
        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value;
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

        private SourceCollection selectedSourceItem;
        public SourceCollection SelectedSourceItem
        {
            get { return selectedSourceItem; }
            set
            {
                selectedSourceItem = value;
                VideoItemsCollection.Refresh();
            }
        }

        private VideoManagerItem selectedItem;
        public VideoManagerItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                if (selectedItem != null)
                {
                    VideoSource = new Uri(SelectedItem.VideoPath);
                }
                SetButtonsStatuses();
            }
        }

        private bool AddButtonIsEnabled = false;
        private bool RemoveButtonIsEnabled = false;

        private void SetButtonsStatuses()
        {
            if (SelectedItem == null)
            {
                AddButtonIsEnabled = false;
                RemoveButtonIsEnabled = false;
                return;
            }
            
            if (FileSystem.FileExists(SelectedItem.VideoPath))
            {
                RemoveButtonIsEnabled = true;
            }
            else
            {
                RemoveButtonIsEnabled = false;
            }
            AddButtonIsEnabled = true;
        }

        public RelayCommand<VideoManagerItem> AddVideoCommand
        {
            get => new RelayCommand<VideoManagerItem>((item) =>
            {
                AddVideo(item);
            }, (item) => AddButtonIsEnabled);
        }

        private bool AddVideo(VideoManagerItem videoManagerItem)
        {
            VideoSource = null;
            var videoDestinationPath = videoManagerItem.VideoPath;
            var videoSourcePath = PlayniteApi.Dialogs.SelectFile("mp4|*.mp4");
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
            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSplashScreen_IntroVideoAddedMessage"), "Splash Screen");
            return true;
        }

        public RelayCommand<VideoManagerItem> RemoveVideoCommand
        {
            get => new RelayCommand<VideoManagerItem>((item) =>
            {
                RemoveVideo(item);
            }, (item) => RemoveButtonIsEnabled);
        }

        private void RemoveVideo(VideoManagerItem videoManagerItem)
        {
            VideoSource = null;
            FileSystem.DeleteFileSafe(videoManagerItem.VideoPath);
            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSplashScreen_IntroVideoRemovedMessage"), "Splash Screen");
        }

        public VideoManagerViewModel(IPlayniteAPI api)
        {
            PlayniteApi = api;

            string videoPathTemplate = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "{0}", "{1}", "VideoIntro.mp4");
            List<VideoManagerItem> videoItemsCollection = new List<VideoManagerItem>();

            // Games
            foreach (Game game in PlayniteApi.MainView.SelectedGames)
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
            foreach (GameSource source in PlayniteApi.Database.Sources)
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
            foreach (Platform platform in PlayniteApi.Database.Platforms)
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
            VideoManagerItem videoManagerItem = item as VideoManagerItem;
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
