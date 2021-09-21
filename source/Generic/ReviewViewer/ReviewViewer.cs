using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using ReviewViewer.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ReviewViewer
{
    public class ReviewViewer : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private string steamApiLanguage;
        private string pluginInstallationPath;

        private ReviewViewerSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("d9abd792-3a46-4b08-92be-dd411e1b471c");

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

            pluginInstallationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            steamApiLanguage = "english";
            if (settings.Settings.UseMatchingSteamApiLang)
            {
                steamApiLanguage = GetSteamApiMatchingLanguage(PlayniteApi.ApplicationSettings.Language);
            }
        }

        public string GetSteamApiMatchingLanguage(string playniteLanguage)
        {
            // From https://partner.steamgames.com/doc/store/localization

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

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "ReviewsControl")
            {
                return new ReviewsControl(GetPluginUserDataPath(), steamApiLanguage);
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
    }
}