using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using PlayniteUtilitiesCommon;
using PluginsCommon.Web;
using PurchaseDateImporter.Models;
using PurchaseDateImporter.ViewModels;
using PurchaseDateImporter.Views;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace PurchaseDateImporter
{
    public class PurchaseDateImporter : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private PurchaseDateImporterSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("eb9abc51-93a4-4db6-b2c9-159eb531b0f2");

        public PurchaseDateImporter(IPlayniteAPI api) : base(api)
        {
            settings = new PurchaseDateImporterSettingsViewModel(this);
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
            return new PurchaseDateImporterSettingsView();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = "Open Purchase Date Importer window",
                    MenuSection = "@Purchase Date Importer",
                    Action = a => {
                        OpenDateImporterWindow();
                    }
                }
            };
        }

        private void OpenDateImporterWindow()
        {
            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Height = 330;
            window.Width = 450;
            window.Title = "Purchase Date Importer";

            window.Content = new PurchaseDateImporterWindowView();
            window.DataContext = new PurchaseDateImporterWindowViewModel(PlayniteApi);
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.ShowDialog();
        }

    }
}