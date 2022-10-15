using LibraryExporter.ViewModels;
using LibraryExporter.Views;
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

namespace LibraryExporter
{
    public class LibraryExporter : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private LibraryExporterSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("54bf64c6-c453-4cbc-92f8-4960b56f930e");

        public LibraryExporter(IPlayniteAPI api) : base(api)
        {
            settings = new LibraryExporterSettingsViewModel(this);
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
                    Description = ResourceProvider.GetString("LibraryExporterAdvanced_MenuItemDescriptionOpenExportWindow"),
                    MenuSection = "@Library Exporter Advanced",
                    Action = _ =>
                    {
                        OpenExportWindow();
                    }
                }
            };
        }

        private void OpenExportWindow()
        {
            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Height = 700;
            window.Width = 450;
            window.Title = "Library Exporter Advanced";

            window.Content = new LibraryExporterView();
            window.DataContext = new LibraryExporterViewModel(PlayniteApi, settings);
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.ShowDialog();

            // Done to save export settings changes done in opened window
            SavePluginSettings(settings.Settings);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new LibraryExporterSettingsView();
        }
    }
}