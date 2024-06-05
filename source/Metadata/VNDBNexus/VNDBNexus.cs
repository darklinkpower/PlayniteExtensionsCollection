using DatabaseCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using PluginsCommon;
using PluginsCommon.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TemporaryCache;
using VndbApiDomain.CharacterAggregate;
using VndbApiDomain.DatabaseDumpTraitAggregate;
using VndbApiDomain.ProducerAggregate;
using VndbApiDomain.ReleaseAggregate;
using VndbApiDomain.SharedKernel.Entities;
using VndbApiDomain.StaffAggregate;
using VndbApiDomain.TagAggregate;
using VndbApiDomain.TraitAggregate;
using VndbApiDomain.VisualNovelAggregate;
using VndbApiInfrastructure.CharacterAggregate;
using VndbApiInfrastructure.DatabaseDumpTagAggregate;
using VndbApiInfrastructure.ProducerAggregate;
using VndbApiInfrastructure.ReleaseAggregate;
using VndbApiInfrastructure.Services;
using VndbApiInfrastructure.SharedKernel.Responses;
using VndbApiInfrastructure.StaffAggregate;
using VndbApiInfrastructure.TagAggregate;
using VndbApiInfrastructure.TraitAggregate;
using VndbApiInfrastructure.VisualNovelAggregate;
using VNDBNexus.Converters;
using VNDBNexus.Database;
using VNDBNexus.KeyboardSearch;
using EventsCommon;
using VNDBNexus.VndbVisualNovelViewControlAggregate;

namespace VNDBNexus
{
    public class VNDBNexus : MetadataPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly BbCodeProcessor _bbcodeProcessor;
        private readonly VndbDatabase _vndbDatabase;
        private readonly ImageUriToBitmapImageConverter _imageUriToBitmapImageConverter;
        private readonly EventAggregator _eventAggregator;

        public VNDBNexusSettingsViewModel Settings { get; private set; }

        public override Guid Id { get; } = Guid.Parse("39229206-1199-4fee-a014-e8478ea4cd77");
        private const string _pluginExtensionsSource = "VNDBNexus";
        private const string _vndbVisualNovelViewControlName = "VndbVisualNovelViewControl";

        public override List<MetadataField> SupportedFields { get; } = new List<MetadataField>
        {
            MetadataField.Name,
            MetadataField.Platform,
            MetadataField.Developers,
            MetadataField.Description,
            MetadataField.BackgroundImage,
            MetadataField.CoverImage,
            MetadataField.CommunityScore,
            MetadataField.ReleaseDate,
            MetadataField.Tags,
            MetadataField.Links
            //MetadataField.Publishers
        };

        public override string Name => "VNDB Nexus";

        public VNDBNexus(IPlayniteAPI api) : base(api)
        {
            Settings = new VNDBNexusSettingsViewModel(this);
            Properties = new MetadataPluginProperties
            {
                HasSettings = true
            };

            _bbcodeProcessor = new BbCodeProcessor();

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

            _eventAggregator = new EventAggregator();
            Searches = new List<SearchSupport>
            {
                new SearchSupport("vn",
                    ResourceProvider.GetString("LOC_VndbNexus_SearchOnVndbLabel"),
                    new VndbKeyboardSearch(Settings, _eventAggregator))
            };

            var pluginDatabaPath = GetPluginUserDataPath();
            _vndbDatabase = new VndbDatabase(pluginDatabaPath);
            _imageUriToBitmapImageConverter = new ImageUriToBitmapImageConverter(Path.Combine(GetPluginUserDataPath(), "ImagesCache"));
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == _vndbVisualNovelViewControlName)
            {
                return new VndbVisualNovelViewControl(this, Settings, _vndbDatabase, _imageUriToBitmapImageConverter, _eventAggregator);
            }

            return null;
        }

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            return new VNDBNexusMetadataProvider(options, Settings, _bbcodeProcessor);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new VNDBNexusSettingsView();
        }

        private void Tests()
        {
            // Producer
            var producerFilter = ProducerFilterFactory.Search.EqualTo("Hira");
            var producerQuery = new ProducerRequestQuery(producerFilter);
            //var producerQueryResult = vndbService.ExecutePostRequestAsync(producerQuery).GetAwaiter().GetResult();

            //// Staff
            var staffFilter = StaffFilterFactory.Search.EqualTo("Hira");
            var staffQuery = new StaffRequestQuery(staffFilter);
            //var staffQueryResult = vndbService.ExecutePostRequestAsync(staffQuery).GetAwaiter().GetResult();

            ////// Trait
            var traitFilter = TraitFilterFactory.Search.EqualTo("a");
            var traitQuery = new TraitRequestQuery(traitFilter);
            //var traitQueryResult = vndbService.ExecutePostRequestAsync(traitQuery).GetAwaiter().GetResult();

            //// Tag
            var tagFilter = TagFilterFactory.Category.EqualTo(TagCategoryEnum.Technical);
            var tagQuery = new TagRequestQuery(tagFilter);
            //var tagQueryResult = vndbService.ExecutePostRequestAsync(tagQuery).GetAwaiter().GetResult();

            // Character
            var characterFilter = CharacterFilterFactory.Cup.EqualTo(CharacterCupSizeEnum.None);
            var characterQuery = new CharacterRequestQuery(characterFilter);
            //var characterQueryResult = vndbService.ExecutePostRequestAsync(characterQuery).GetAwaiter().GetResult();

            // Release
            var releaseFilter = ReleaseFilterFactory.Voiced.EqualTo(null);
            var releaseQuery = new ReleaseRequestQuery(releaseFilter);
            //var releaseQueryResult = vndbService.ExecutePostRequestAsync(releaseQuery).GetAwaiter().GetResult();

            // Vn
            var vnFilter = VisualNovelFilterFactory.Search.EqualTo("a");
            var vnQuery = new VisualNovelRequestQuery(vnFilter); ;
            //var vnQueryResult = vndbService.ExecutePostRequestAsync(vnQuery).GetAwaiter().GetResult();
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (ShouldUpdateDumps())
            {
                var updateSuccess = Task.Run(() => UpdateTags()).GetAwaiter().GetResult();
                if (updateSuccess)
                {
                    Settings.Settings.LastDataseDumpsUpdate = DateTime.Now;
                    SavePluginSettings(Settings.Settings);
                }
            }
        }

        private async Task<bool> UpdateTags()
        {
            var tags = await VndbService.GetDatabaseDumpsTags();            
            if (tags is null)
            {
                return false;
            }

            var traits = await VndbService.GetDatabaseDumpsTraits();
            if (traits is null)
            {
                return false;
            }

            var tagWrappers = tags.Select(x => new DatabaseDumpTagWrapper(x));
            var traitWrappers = traits.Select(x => new DatabaseDumpTraitWrapper(x));

            _vndbDatabase.DatabaseDumpTags.DeleteAll();
            _vndbDatabase.DatabaseDumpTags.InsertBulk(tagWrappers);

            _vndbDatabase.DatabaseDumpTraits.DeleteAll();
            _vndbDatabase.DatabaseDumpTraits.InsertBulk(traitWrappers);
            return true;
        }

        private bool ShouldUpdateDumps()
        {
            var timeElapsed = DateTime.Now - Settings.Settings.LastDataseDumpsUpdate;
            if (timeElapsed >= TimeSpan.FromDays(7))
            {
                return true;
            }

            if (_vndbDatabase.DatabaseDumpTags.Count == 0)
            {
                return true;
            }

            if (_vndbDatabase.DatabaseDumpTraits.Count == 0)
            {
                return true;
            }

            return false;
        }


    }
}