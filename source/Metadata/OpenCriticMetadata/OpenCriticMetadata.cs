using OpenCriticMetadata.Services;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OpenCriticMetadata
{
    public class OpenCriticMetadata : MetadataPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private OpenCriticMetadataSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("c29e6c13-089b-43d5-a916-514d85e10486");
        private readonly OpenCriticService openCriticService = new OpenCriticService();

        public override List<MetadataField> SupportedFields { get; } = new List<MetadataField>
        {
            MetadataField.CriticScore
        };

        public override string Name => "OpenCritic Metadata";

        public OpenCriticMetadata(IPlayniteAPI api) : base(api)
        {
            settings = new OpenCriticMetadataSettingsViewModel(this);
            Properties = new MetadataPluginProperties
            {
                HasSettings = false
            };
        }

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            return new OpenCriticMetadataProvider(options, this, openCriticService);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new OpenCriticMetadataSettingsView();
        }
    }
}