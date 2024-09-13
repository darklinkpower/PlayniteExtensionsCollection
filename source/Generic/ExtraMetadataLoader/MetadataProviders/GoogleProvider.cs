using ExtraMetadataLoader.ViewModels;
using ExtraMetadataLoader.Views;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ExtraMetadataLoader.MetadataProviders
{
    public class GoogleProvider : ILogoProvider
    {
        private readonly IPlayniteAPI playniteApi;
        private readonly ExtraMetadataLoaderSettings settings;

        public string Id => "googleProvider";

        public GoogleProvider(IPlayniteAPI playniteApi, ExtraMetadataLoaderSettings settings)
        {
            this.playniteApi = playniteApi;
            this.settings = settings;
        }

        public string GetLogoUrl(Game game, LogoDownloadOptions downloadOptions, CancellationToken cancelToken = default)
        {
            if (downloadOptions.IsBackgroundDownload || cancelToken.IsCancellationRequested)
            {
                return null;
            }

            var window = playniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false
            });

            window.Height = 600;
            window.Width = 840;
            window.Title = string.Format(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogCaptionSelectLogo"), game.Name);
            window.Content = new GoogleImageDownloaderView();
            var viewModel = new GoogleImageDownloaderViewModel(playniteApi, game);
            window.DataContext = viewModel;
            window.Owner = playniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();

            return viewModel.LogoUrl;
        }
    }
}
