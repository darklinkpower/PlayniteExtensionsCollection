using NewsViewer.PluginControls;
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
using System.Windows.Media;

namespace NewsViewer
{
    public class NewsViewer : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly string steamApiLanguage;

        public NewsViewerSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("15e03ffe-90f6-4e8e-bd4d-94514777481d");

        public NewsViewer(IPlayniteAPI api) : base(api)
        {
            settings = new NewsViewerSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { "NewsViewerControl", "PlayersInGameViewerControl" },
                SourceName = "NewsViewer",
            });

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "NewsViewer",
                SettingsRoot = $"{nameof(settings)}.{nameof(settings.Settings)}"
            });

            steamApiLanguage = Steam.GetSteamApiMatchingLanguage(PlayniteApi.ApplicationSettings.Language);
        }

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            yield return new SidebarItem
            {
                Title = ResourceProvider.GetString("LOC_NewsViewer_SidebarItemDescription_SteamNews"),
                Type = SiderbarItemType.Button,
                Icon = new TextBlock
                {
                    FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily,
                    Text = "\uefa7",
                },
                Activated = () => {
                    var webView = PlayniteApi.WebViews.CreateView(1024, 700);
                    webView.Navigate(@"https://store.steampowered.com/news/");
                    webView.OpenDialog();
                    webView.Dispose();
                }
            };
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "NewsViewerControl")
            {
                return new NewsViewerControl(PlayniteApi, settings, steamApiLanguage);
            }
            else if (args.Name == "PlayersInGameViewerControl")
            {
                return new PlayersInGameViewerControl(PlayniteApi, settings);
            }

            return null;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new NewsViewerSettingsView();
        }
    }
}