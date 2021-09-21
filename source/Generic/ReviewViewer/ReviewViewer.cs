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
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "ReviewsControl")
            {
                return new ReviewsControl(GetPluginUserDataPath());
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