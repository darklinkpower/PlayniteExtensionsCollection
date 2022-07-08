using ExtraMetadataLoader.Models;
using ExtraMetadataLoader.Services;
using Playnite.SDK;
using Playnite.SDK.Data;
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
    class GoogleImageDownloaderViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

        private bool searchTransparent { get; set; }
        public bool SearchTransparent
        {
            get => searchTransparent;
            set
            {
                searchTransparent = value;
                OnPropertyChanged();
            }
        }

        private List<GoogleImage> searchItems { get; set; }
        public List<GoogleImage> SearchItems
        {
            get => searchItems;
            set
            {
                searchItems = value;
                OnPropertyChanged();
            }
        }

        private GoogleImage selectedItem { get; set; }
        public GoogleImage SelectedItem
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

        private IPlayniteAPI PlayniteApi { get; }

        private readonly LogosDownloader logosDownloader;
        private readonly Game game;

        public GoogleImageDownloaderViewModel(IPlayniteAPI PlayniteApi, Game game, LogosDownloader logosDownloader)
        {
            this.PlayniteApi = PlayniteApi;
            this.logosDownloader = logosDownloader;
            this.game = game;
            SearchTransparent = true;
            SearchTerm = $"{game.Name} logo";

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
            var images = new List<GoogleImage>();
            var webViewSettings = new WebViewSettings
            {
                JavaScriptEnabled = true
            };

            var escapedSearchTerm = Uri.EscapeDataString(SearchTerm);
            var url = string.Format(@"https://www.google.com/search?tbm=isch&client=firefox-b-d&source=lnt&q={0}", escapedSearchTerm);
            if (SearchTransparent)
            {
                url += "&tbs=ic:trans";
            }
            var webView = PlayniteApi.WebViews.CreateOffscreenView(webViewSettings);
            webView.NavigateAndWait(url);

            if (webView.GetCurrentAddress().StartsWith(@"https://consent.google.com", StringComparison.OrdinalIgnoreCase))
            {
                // This rejects Google's consent form for cookies
                RejectGoogleCookiesConsent(webView).GetAwaiter().GetResult();
                webView.NavigateAndWait(url);
            }

            var pageSource = webView.GetPageSource();
            pageSource = Regex.Replace(pageSource, @"\r\n?|\n", string.Empty);
            var matches = Regex.Matches(pageSource, @"\[""(https:\/\/encrypted-[^,]+?)"",\d+,\d+\],\[""(http.+?)"",(\d+),(\d+)\]");
            foreach (Match match in matches)
            {
                if (images.Count == 30)
                {
                    break;
                }
                var data = Serialization.FromJson<List<List<object>>>($"[{match.Value}]");
                images.Add(new GoogleImage
                {
                    ThumbUrl = data[0][0].ToString(),
                    ImageUrl = data[1][0].ToString(),
                    Height = uint.Parse(data[1][1].ToString()),
                    Width = uint.Parse(data[1][2].ToString())
                });
            }

            SearchItems = images;
        }

        private static async Task RejectGoogleCookiesConsent(IWebView webView)
        {
            await webView.EvaluateScriptAsync(@"document.getElementsByTagName('form')[0].submit();");
            await Task.Delay(3000);
        }

        public RelayCommand<object> DownloadSelectedImageCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                DownloadSelectedImage();
            }, (a) => IsItemSelected);
        }

        public void DownloadSelectedImage()
        {
            var success = logosDownloader.DownloadGoogleImage(game, SelectedItem.ImageUrl, true);
            if (success)
            {
                PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDone"), "Extra Metadata Loader");
            }
            else
            {
                PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogMessageDownloadingGoogleLogoError"), "Extra Metadata Loader");
            }
        }
    }
}
