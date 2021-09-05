using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
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
                HasSettings = true
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
                    Description = "Launch menu window",
                    MenuSection = "@Steam Game Transfer Utility",
                    Action = a => {
                        WindowMethod();
                    }
                }
            };
        }

        public void WindowMethod()
        {
            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false
            });

            window.Height = 250;
            window.Width = 600;
            window.Title = "Steam Game Transfer Utility";

            // Set content of a window. Can be loaded from xaml, loaded from UserControl or created from code behind
            WindowView windowView = new WindowView();
            windowView.PlayniteApi = PlayniteApi;
            window.Content = windowView;

            // Set owner if you need to create modal dialog window
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Use Show or ShowDialog to show the window
            window.ShowDialog();
        }
    }
}