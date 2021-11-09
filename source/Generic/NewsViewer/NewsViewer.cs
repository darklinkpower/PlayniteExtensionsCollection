using NewsViewer.PluginControls;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
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
                ElementList = new List<string> { "NewsViewerControl" },
                SourceName = "NewsViewer",
            });

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = "NewsViewer",
                SettingsRoot = $"{nameof(settings)}.{nameof(settings.Settings)}"
            });

            steamApiLanguage = GetSteamApiMatchingLanguage(PlayniteApi.ApplicationSettings.Language);
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

        public string GetSteamApiMatchingLanguage(string playniteLanguage)
        {
            // https://partner.steamgames.com/doc/store/localization

            switch (playniteLanguage)
            {
                case "en_US":
                    return "english";
                case "es_ES":
                    return "spanish";
                case "ar_SA":
                    return "ar";
                case "ca_ES":
                    return "spanish";
                case "cs_CZ":
                    return "cs";
                case "de_DE":
                    return "de";
                case "el_GR":
                    return "el";
                case "fa_IR":
                    return "english";
                case "fi_FI":
                    return "fi";
                case "fr_FR":
                    return "fr";
                case "he_IL":
                    return "english";
                case "hr_HR":
                    return "english";
                case "hu_HU":
                    return "hu";
                case "id_ID":
                    return "english";
                case "it_IT":
                    return "it";
                case "ja_JP":
                    return "ja";
                case "ko_KR":
                    return "ko";
                case "lt_LT":
                    return "english";
                case "nl_NL":
                    return "nl";
                case "no_NO":
                    return "no";
                case "pl_PL":
                    return "pl";
                case "pt_BR":
                    return "pt-BR";
                case "pt_PT":
                    return "pt";
                case "ro_RO":
                    return "ro";
                case "ru_RU":
                    return "ru";
                case "sk_SK":
                    return "english";
                case "sr_SP":
                    return "english";
                case "sv_SE":
                    return "sv";
                case "tr_TR":
                    return "tr";
                case "uk_UA":
                    return "english";
                case "zh_CN":
                    return "zh-CN";
                case "zh_TW":
                    return "zh-TW";
                default:
                    return "english";
            }
        }
    }
}