using GOGSecondClassGameWatcher.Domain.ValueObjects;
using GOGSecondClassGameWatcher.Presentation;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GOGSecondClassGameWatcher.Application
{
    public class GogSecondClassGameWindowCreator
    {
        private readonly IPlayniteAPI _playniteApi;

        public GogSecondClassGameWindowCreator(IPlayniteAPI playniteApi)
        {
            _playniteApi = playniteApi;
        }

        public void OpenWindow(GogSecondClassGame gogSecondClassGame, Game game = null)
        {
            string imagePath = null;
            if (game != null && !game.CoverImage.IsNullOrEmpty())
            {
                var coverPath = _playniteApi.Database.GetFullFilePath(game.CoverImage);
                if (FileSystem.FileExists(coverPath))
                {
                    imagePath = coverPath;
                }
            }

            var window = _playniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Height = 700;
            window.Width = 700;
            window.SizeToContent = SizeToContent.Height;
            window.Title = "GOG Second Class Game Watcher";

            window.Content = new GogSecondClassGameDisplayView();
            window.DataContext = new GogSecondClassGameDisplayViewModel(gogSecondClassGame, imagePath);
            window.Owner = _playniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.ShowDialog();
        }
    }
}