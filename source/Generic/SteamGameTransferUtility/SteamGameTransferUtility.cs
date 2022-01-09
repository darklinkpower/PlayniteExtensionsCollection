using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using SteamGameTransferUtility.ViewModels;
using SteamGameTransferUtility.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SteamGameTransferUtility
{
    public class SteamGameTransferUtility : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private SteamGameTransferUtilitySettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("c2dac2df-44c9-4f47-8555-c8d134c4f400");

        public SteamGameTransferUtility(IPlayniteAPI api) : base(api)
        {
            settings = new SteamGameTransferUtilitySettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SteamGameTransferUtilitySettingsView();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCSteam_Game_Transfer_Utility_MenuItemLaunchWindowDescription"),
                    MenuSection = "@Steam Game Transfer Utility",
                    Action = a => {
                        ShowTransferWindow();
                    }
                }
            };
        }

        public void ShowTransferWindow()
        {
            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false
            });

            window.Height = 600;
            window.Width = 600;
            window.Title = "Steam Game Transfer Utility";
            window.Content = new TransferWindow();
            window.DataContext = new TransferWindowViewModel(PlayniteApi);
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            window.ShowDialog();
        }
    }
}