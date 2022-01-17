using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SteamDescriptionCleaner
{
    public class SteamDescriptionCleaner : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly Regex descriptionRegex = new Regex(@"<h1>About the Game<\/h1>([\s\S]+)", RegexOptions.Compiled);

        private SteamDescriptionCleanerSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("7793f6ec-83c9-4094-9d1c-e0c2280a0496");

        public SteamDescriptionCleaner(IPlayniteAPI api) : base(api)
        {
            settings = new SteamDescriptionCleanerSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {

            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSteam_Description_Cleaner_MenuItemDescriptionCleanAllGamesDescriptions"),
                    MenuSection = "@Steam Description Cleaner",
                    Action = a => {
                        CleanSteamDescriptions(PlayniteApi.Database.Games);
                    }
                }
            };
        }

        private void CleanSteamDescriptions(IEnumerable<Game> gamesCollection)
        {
            var descriptionChangedCount = 0;
            foreach (Game game in gamesCollection)
            {
                var descriptionChanged = CleanGameSteamDescription(game);
                if (descriptionChanged)
                {
                    descriptionChangedCount++;
                }
            }

            PlayniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCSteam_Description_Cleaner_ResultsMessageGamesDescriptionClean"), descriptionChangedCount), "Steam Description Cleaner");
        }

        private bool CleanGameSteamDescription(Game game)
        {
            if (string.IsNullOrEmpty(game.Description))
            {
                return false;
            }

            var descriptionMatch = descriptionRegex.Match(game.Description);
            if (descriptionMatch.Success)
            {
                game.Description = descriptionMatch.Groups[1].Value;
                PlayniteApi.Database.Games.Update(game);
                return true;
            }
            return false;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SteamDescriptionCleanerSettingsView();
        }
    }
}