using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SteamSearch
{
    public class SteamSearch : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private SteamSearchSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("78c7912a-32bb-4a42-8485-d348d10023ac");

        public SteamSearch(IPlayniteAPI api) : base(api)
        {
            settings = new SteamSearchSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            Searches = new List<SearchSupport>
            {
                new SearchSupport("st",
                    ResourceProvider.GetString("LOCSteam_Search_SearchNameSearchOnSteam"),
                    new SteamSearcher(settings))
            };

            PlayniteApi.Database.Games.ItemCollectionChanged += (sender, ItemCollectionChangedArgs) =>
            {
                foreach (var removedGame in ItemCollectionChangedArgs.RemovedItems)
                {
                    if (Steam.IsGameSteamGame(removedGame))
                    {
                        settings.Settings.SteamIdsInLibrary.Remove(removedGame.GameId);
                    }
                }

                foreach (var addedGame in ItemCollectionChangedArgs.AddedItems)
                {
                    if (Steam.IsGameSteamGame(addedGame))
                    {
                        settings.Settings.SteamIdsInLibrary.Add(addedGame.GameId);
                    }
                }
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SteamSearchSettingsView();
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            foreach (var game in PlayniteApi.Database.Games)
            {
                if (Steam.IsGameSteamGame(game))
                {
                    settings.Settings.SteamIdsInLibrary.Add(game.GameId);
                }
            }
        }
    }
}