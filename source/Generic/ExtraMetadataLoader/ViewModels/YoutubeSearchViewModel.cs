using ExtraMetadataLoader.Common;
using ExtraMetadataLoader.Models;
using ExtraMetadataLoader.Services;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.ViewModels
{
    class YoutubeSearchViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private List<YoutubeSearchItem> searchItems { get; set; }
        public List<YoutubeSearchItem> SearchItems
        {
            get => searchItems;
            set
            {
                searchItems = value;
                OnPropertyChanged();
            }
        }

        private bool isItemSelected { get; set; }
        public bool IsItemSelected
        {
            get => isItemSelected;
            set
            {
                isItemSelected = value;
                OnPropertyChanged();
            }
        }

        private YoutubeSearchItem selectedItem { get; set; }
        public YoutubeSearchItem SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                if (selectedItem != null)
                {
                    IsItemSelected = true;
                }
                else
                {
                    IsItemSelected = false;
                }
                OnPropertyChanged();
            }
        }

        private string searchterm { get; set; }
        public string SearchTerm
        {
            get => searchterm;
            set
            {
                searchterm = value;
                OnPropertyChanged();
            }
        }

        private IPlayniteAPI PlayniteApi { get; }

        private readonly VideosDownloader videosDownloader;
        private readonly Game game;

        public YoutubeSearchViewModel(IPlayniteAPI PlayniteApi, Game game, VideosDownloader videosDownloader)
        {
            this.PlayniteApi = PlayniteApi;
            this.videosDownloader = videosDownloader;
            this.game = game;
            var initialSearch = $"{game.Name} trailer";
            var platform = game.Platforms?.FirstOrDefault(x => x.Name != null);
            if (platform != null)
            {
                initialSearch = $"{game.Name} {platform.Name} trailer";
            }

            // Using PC only in the search provides better results
            SearchTerm = initialSearch.Replace("PC (Windows)", "PC");
            InvokeSearch();
        }

        public RelayCommand<object> InvokeSearchCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                InvokeSearch();
            });
        }

        public void InvokeSearch()
        {
            SearchItems = YoutubeCommon.GetYoutubeSearchResults(SearchTerm);
        }

        public RelayCommand<object> DownloadSelectedVideoCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                DownloadSelectedVideo();
            }, (a) => IsItemSelected);
        }

        public void DownloadSelectedVideo()
        {
            var success = false;
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                success = videosDownloader.DownloadYoutubeVideoById(game, SelectedItem.VideoId, true);
            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDownloadingVideoYoutube")));
            if (success)
            {
                PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
            }
            else
            {
                PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDownloadingVideoYoutubeError"), "Extra Metadata Loader");
            }
        }

        public RelayCommand<object> ViewSelectedVideoCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ViewSelectedVideo();
            }, (a) => IsItemSelected);
        }

        public void ViewSelectedVideo()
        {
            var youtubeLink = string.Format("https://www.youtube.com/embed/{0}", selectedItem.VideoId);
            var html = string.Format(@"
                    <head>
                        <title>Extra Metadata</title>
                        <meta http-equiv='refresh' content='0; url={0}'>
                    </head>
                    <body style='margin:0'>
                    </body>", youtubeLink);
            var webView = PlayniteApi.WebViews.CreateView(1280, 750);

            // Age restricted videos can only be seen in the full version while logged in
            // so it's needed to redirect to the full YouTube site to view them
            var embedLoaded = false;
            webView.LoadingChanged += async (s, e) =>
            {
                if (!embedLoaded)
                {
                    if (webView.GetCurrentAddress().StartsWith(@"https://www.youtube.com/embed/"))
                    {
                        var source = await webView.GetPageSourceAsync();
                        if (source.Contains("<div class=\"ytp-error-content-wrap\"><div class=\"ytp-error-content-wrap-reason\">"))
                        {
                            webView.Navigate($"https://www.youtube.com/watch?v={selectedItem.VideoId}");
                        }
                        embedLoaded = true;
                    }
                }
            };

            webView.Navigate("data:text/html," + html);
            webView.OpenDialog();
            webView.Dispose();
        }

    }
}
