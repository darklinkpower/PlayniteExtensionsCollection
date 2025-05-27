using CoverCollageMaker.Domain.ValueObjects;
using CoverCollageMaker.Presentation.ViewModels;
using CoverCollageMaker.Presentation.Views;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CoverCollageMaker
{
    public class CoverCollageMaker : GenericPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly CoverCollageMakerSettingsViewModel _settingsViewModel;

        public override Guid Id { get; } = Guid.Parse("7713ea8c-cdec-46ac-b603-6d666515f3da");

        public CoverCollageMaker(IPlayniteAPI api) : base(api)
        {
            _settingsViewModel = new CoverCollageMakerSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = "Create game covers collage...",
                    MenuSection = "Cover Collage Maker",
                    Action = a =>
                    {
                        CreateCollage(a.Games.Distinct().OrderBy(x => x.Name).ToList());
                    }
                }
            };
        }

        private void CreateCollage(List<Game> games)
        {
            var gamesWithCoverImages = games.Where(x => !x.CoverImage.IsNullOrEmpty())
                .Select(x => new ImageData(PlayniteApi.Database.GetFullFilePath(x.CoverImage), x.Name));
            var gamesWithValidCoverImages = gamesWithCoverImages.Where(x => FileSystem.FileExists(x.Path)).ToList();
            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = true,
                ShowMaximizeButton = true
            });

            window.Height = 700;
            window.Width = 900;
            window.Title = "Images Collage Generator";

            window.Content = new ImagesCollageGeneratorView();
            var viewModel = new ImagesCollageGeneratorViewModel(
                _logger,
                PlayniteApi,
                gamesWithValidCoverImages,
                _settingsViewModel.Settings.ExportDirectory,
                _settingsViewModel.Settings.CollageParameters);
            window.DataContext = viewModel;
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.ShowDialog();
            _settingsViewModel.Settings.ExportDirectory = viewModel.ExportDirectory;
            _settingsViewModel.Settings.CollageParameters = viewModel.CollageParameters;
            this.SavePluginSettings(_settingsViewModel.Settings);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return _settingsViewModel;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new CoverCollageMakerSettingsView();
        }
    }
}