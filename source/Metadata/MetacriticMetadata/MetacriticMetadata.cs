using MetacriticMetadata.Services;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MetacriticMetadata
{
    public class MetacriticMetadata : MetadataPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly MetacriticService metacriticService;

        private MetacriticMetadataSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("841bb91d-dd55-4f40-9d5a-f82a45267394");

        public override List<MetadataField> SupportedFields { get; } = new List<MetadataField>
        {
            MetadataField.CriticScore
        };

        public override string Name => "Metacritic";

        public MetacriticMetadata(IPlayniteAPI api) : base(api)
        {
            settings = new MetacriticMetadataSettingsViewModel(this);
            Properties = new MetadataPluginProperties
            {
                HasSettings = true
            };

            metacriticService = new MetacriticService(settings.Settings);
            Searches = new List<SearchSupport>
            {
                new SearchSupport("mc",
                    "Metacritic",
                    new MetacriticSearchContext(metacriticService))
            };
        }

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            return new MetacriticMetadataProvider(options, this, metacriticService);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new MetacriticMetadataSettingsView();
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (settings.Settings.ApiKey.IsNullOrEmpty())
            {
                PlayniteApi.Notifications.Add(
                    new NotificationMessage(
                        "MetacriticApiKeyNotConfigured",
                        "Metacritic Metadata:\n" + "API Key has not been configured",
                        NotificationType.Info,
                        () => OpenSettingsView())
                );
            }
        }
    }
}