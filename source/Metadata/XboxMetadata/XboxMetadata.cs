using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using XboxMetadata.Services;

namespace XboxMetadata
{
    public class XboxMetadata : MetadataPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly XboxMetadataSettingsViewModel _settings;

        private readonly XboxWebService _xboxWebService;

        public override Guid Id { get; } = Guid.Parse("7a663fbb-99b9-4291-a5e1-3ce7f2442b59");

        public override List<MetadataField> SupportedFields { get; } = new List<MetadataField>
        {
            MetadataField.Name,
            MetadataField.Developers,
            MetadataField.Publishers,
            MetadataField.AgeRating,
            MetadataField.Platform,
            MetadataField.Description,
            MetadataField.Icon,
            MetadataField.BackgroundImage,
            MetadataField.CoverImage,
            MetadataField.CommunityScore,
            MetadataField.ReleaseDate
        };

        public override string Name => "Xbox";

        public XboxMetadata(IPlayniteAPI api) : base(api)
        {
            _settings = new XboxMetadataSettingsViewModel(this);
            _xboxWebService = new XboxWebService(_settings);
            Properties = new MetadataPluginProperties
            {
                HasSettings = true
            };
        }

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            return new XboxMetadataProvider(options, this, _settings, _xboxWebService);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return _settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new XboxMetadataSettingsView();
        }
    }
}