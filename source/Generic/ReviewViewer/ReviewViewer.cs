using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon;
using PluginsCommon.Web;
using ReviewViewer.Controls;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ReviewViewer
{
    public class ReviewViewer : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private string steamApiLanguage;

        private ReviewViewerSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("ca24e37a-76d9-49bf-89ab-d3cba4a54bd1");

        public ReviewViewer(IPlayniteAPI api) : base(api)
        {
            settings = new ReviewViewerSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                SourceName = "ReviewViewer",
                ElementList = new List<string> { "ReviewsControl" }
            });

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "ReviewViewer",
                SettingsRoot = $"{nameof(settings)}.{nameof(settings.Settings)}"
            });

            steamApiLanguage = "english";
            if (settings.Settings.UseMatchingSteamApiLang)
            {
                steamApiLanguage = Steam.GetSteamApiMatchingLanguage(PlayniteApi.ApplicationSettings.Language);
            }
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "ReviewsControl")
            {
                return new ReviewsControl(GetPluginUserDataPath(), steamApiLanguage, settings, PlayniteApi);
            }

            return null;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ReviewViewerSettingsView();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCReview_Viewer_MenuItemUpdateDataDescription"),
                    MenuSection = "Review Viewer",
                    Action = a => {
                       RefreshGameData(args.Games);
                    }
                }
            };
        }

        public void RefreshGameData(List<Game> games)
        {
            var userOverwriteChoice = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCReview_Viewer_DialogOverwriteChoiceMessage"), "Review Viewer", MessageBoxButton.YesNo);
            var reviewSearchTypes = new string[] { "all", "positive", "negative" };
            var pluginDataPath = GetPluginUserDataPath();
            var reviewsApiMask = @"https://store.steampowered.com/appreviews/{0}?json=1&purchase_type=all&language={1}&review_type={2}&playtime_filter_min=0&filter=summary";

            var progressTitle = ResourceProvider.GetString("LOCReview_Viewer_DialogDataUpdateProgressMessage");
            var progressOptions = new GlobalProgressOptions(progressTitle, true);
            progressOptions.IsIndeterminate = false;
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                a.ProgressMaxValue = games.Count();
                foreach (Game game in games)
                {
                    if (a.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                    a.CurrentProgressValue++;
                    a.Text = $"{progressTitle}\n\n{a.CurrentProgressValue}/{a.ProgressMaxValue}\n{game.Name}";
                    var steamId = Steam.GetGameSteamId(game, true);
                    if (steamId.IsNullOrEmpty())
                    {
                        steamId = game.GameId;
                    }

                    foreach (string reviewSearchType in reviewSearchTypes)
                    {
                        var gameDataPath = Path.Combine(pluginDataPath, $"{game.Id}_{reviewSearchType}.json");
                        if (FileSystem.FileExists(gameDataPath) && userOverwriteChoice != MessageBoxResult.Yes)
                        {
                            continue;
                        }
                        var uri = string.Format(reviewsApiMask, steamId, steamApiLanguage, reviewSearchType);

                        // To prevent being rate limited
                        Thread.Sleep(200);
                        HttpDownloader.DownloadJsonFileAsync(uri, gameDataPath).GetAwaiter().GetResult();
                    }
                }
            }, progressOptions);

            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCReview_Viewer_DialogResultsDataRefreshFinishedMessage"), "Review Viewer");
        }

    }
}