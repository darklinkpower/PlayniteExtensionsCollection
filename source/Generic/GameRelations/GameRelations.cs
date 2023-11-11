using GameRelations.PlayniteControls;
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
using System.Windows.Media.Imaging;
using TemporaryCache;

namespace GameRelations
{
    public class GameRelations : GenericPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private const string _pluginSourceName = "GameRelations";
        private const string _similarGamesControlName = "SimilarGamesControl";
        private const string _sameDeveloperControlName = "SameDeveloperControl";
        private const string _samePublisherControlName = "SamePublisherControl";
        private const string _sameSeriesControlName = "SameSeriesControl";

        public GameRelationsSettingsViewModel Settings { get; private set; }

        public override Guid Id { get; } = Guid.Parse("a4c15d63-9ab4-4d96-9a0c-8f9b35d43a1f");

        public GameRelations(IPlayniteAPI api) : base(api)
        {
            Settings = new GameRelationsSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = _pluginSourceName,
                SettingsRoot = $"{nameof(Settings)}.{nameof(Settings.Settings)}"
            });

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { _similarGamesControlName, _sameDeveloperControlName, _samePublisherControlName, _sameSeriesControlName },
                SourceName = _pluginSourceName
            });
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            switch (args.Name)
            {
                case _similarGamesControlName:
                    return new SimilarGamesControl(PlayniteApi, Settings.Settings, Settings.Settings.SimilarGamesControlSettings);
                case _sameDeveloperControlName:
                    return new SameDeveloperControl(PlayniteApi, Settings.Settings, Settings.Settings.SameDeveloperControlSettings);
                case _samePublisherControlName:
                    return new SamePublisherControl(PlayniteApi, Settings.Settings, Settings.Settings.SamePublisherControlSettings);
                case _sameSeriesControlName:
                    return new SameSeriesControl(PlayniteApi, Settings.Settings, Settings.Settings.SameSeriesControlSettings);
                default:
                    return null;
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new GameRelationsSettingsView();
        }
    }
}