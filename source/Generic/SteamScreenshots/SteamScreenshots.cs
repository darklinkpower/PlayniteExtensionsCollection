using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon.Converters;
using SteamScreenshots.ScreenshotsControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SteamScreenshots
{
    public class SteamScreenshots : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly ImageUriToBitmapImageConverter _imageUriToBitmapImageConverter;

        public SteamScreenshotsSettingsViewModel Settings { get; private set; }

        public override Guid Id { get; } = Guid.Parse("8e77fe31-5e62-41e2-8fa2-64844cfd5b6b");
        private const string _pluginExtensionsSource = "SteamScreenshots";
        private const string _vndbVisualNovelViewControlName = "SteamScreenshotsViewControl";

        public SteamScreenshots(IPlayniteAPI api) : base(api)
        {
            Settings = new SteamScreenshotsSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { _vndbVisualNovelViewControlName },
                SourceName = _pluginExtensionsSource,
            });

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = _pluginExtensionsSource,
                SettingsRoot = $"{nameof(Settings)}.{nameof(Settings.Settings)}"
            });

            _imageUriToBitmapImageConverter = new ImageUriToBitmapImageConverter(Path.Combine(GetPluginUserDataPath(), "ScreenshotsCache"), true);
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == _vndbVisualNovelViewControlName)
            {
                return new SteamScreenshotsControl(this, Settings, _imageUriToBitmapImageConverter);
            }

            return null;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new SteamScreenshotsSettingsView();
        }
    }
}