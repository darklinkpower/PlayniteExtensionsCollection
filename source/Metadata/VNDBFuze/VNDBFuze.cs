using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using VNDBFuze.VndbDomain.Aggregates.CharacterAggregate;
using VNDBFuze.VndbDomain.Aggregates.ProducerAggregate;
using VNDBFuze.VndbDomain.Aggregates.ReleaseAggregate;
using VNDBFuze.VndbDomain.Aggregates.StaffAggregate;
using VNDBFuze.VndbDomain.Aggregates.TagAggregate;
using VNDBFuze.VndbDomain.Aggregates.TraitAggregate;
using VNDBFuze.VndbDomain.Aggregates.VnAggregate;
using VNDBFuze.VndbDomain.Common.Enums;
using VNDBFuze.VndbDomain.Services;

namespace VNDBFuze
{
    public class VNDBFuze : MetadataPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly VndbService _vndbService;
        private readonly BbCodeProcessor _bbcodeProcessor;
        private readonly VNDBFuzeSettingsViewModel _settings;

        public override Guid Id { get; } = Guid.Parse("39229206-1199-4fee-a014-e8478ea4cd77");

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
            //MetadataField.Publishers,
            //MetadataField.Links
        };

        public override string Name => "VNDB Fuze";

        public VNDBFuze(IPlayniteAPI api) : base(api)
        {
            _settings = new VNDBFuzeSettingsViewModel(this);
            Properties = new MetadataPluginProperties
            {
                HasSettings = true
            };

            _vndbService = new VndbService();
            _bbcodeProcessor = new BbCodeProcessor();
        }

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            return new VNDBFuzeMetadataProvider(options, _vndbService, _settings, _bbcodeProcessor);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return _settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new VNDBFuzeSettingsView();
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
            var vnFilter = VnFilterFactory.Search.EqualTo("a");
            var vnQuery = new VnRequestQuery(vnFilter); ;
            //var vnQueryResult = vndbService.ExecutePostRequestAsync(vnQuery).GetAwaiter().GetResult();

            var ss = "";
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            Tests();

            //var filterTwo = ProducerFilterFactory.Language.EqualTo(LanguageEnum.English);
            //var complexFilter = ProducerFilterFactory.And(filterOne, filterTwo);

            //var filterOneJson = filterOne.ToJsonString();
            //var filterTwoJson = filterTwo.ToJsonString();
            //var complexFilterJson = complexFilter.ToJsonString();
            //var complexQuery = new ProducerRequestQuery(complexFilter);

            //var simpleQuerySerialized = Serialization.ToJson(producerQuery);
            //var complexQuerySerialized = Serialization.ToJson(complexQuery);

            //var complexQueryResult = vndbService.ExecutePostRequestAsync(complexQuery).GetAwaiter().GetResult();

            //var requestSettings = new VisualNovelFieldsSettings();
            //var request = requestSettings.ToString();

            //var predicateOne = new SimplePredicate("Name 1", Operators.OrderingOperator.GreaterOrEqual, "string");
            //var predicateTwo = new SimplePredicate("Name 1", Operators.OrderingOperator.GreaterOrEqual, predicateOne);

            //var sss = Operators.OrderingOperator.GreaterOrEqual;
            //var predicate1 = new ComplexPredicate(Operators.Predicates.Or,
            //    new SimplePredicate(ReleaseConstants.Filters.Lang, Operators.OrderingOperator.GreaterOrEqual, QueryEnums.Language.English),
            //    new SimplePredicate(ReleaseConstants.Filters.Lang, Operators.Matching.IsEqual, QueryEnums.Language.German),
            //    new SimplePredicate(ReleaseConstants.Filters.Lang, Operators.Matching.IsEqual, QueryEnums.Language.French));

            //var predicate2 = new SimplePredicate(VnConstants.Filters.OriginalLanguage, Operators.Inverting.NotEqual, QueryEnums.Language.Japanese);

            //var releaseCondition = new ComplexPredicate(Operators.Predicates.Or,
            //    new SimplePredicate(ReleaseConstants.Filters.Released, Operators.Ordering.GreaterThanOrEqual, "2020-01-01"),
            //    new SimplePredicate(ReleaseConstants.Filters.Producer, Operators.Matching.IsEqual, new SimplePredicate(ProducerConstants.Filters.Id, Operators.Matching.IsEqual, "p30")));
            //var predicate3 = new SimplePredicate(VnConstants.Filters.Release, Operators.Matching.IsEqual, releaseCondition);

            //var complexFilter = new ComplexPredicate(Operators.Predicates.And,
            //    predicate1,
            //    predicate2,
            //    predicate3);

            //var query = new VndbQuery(complexFilter, requestSettings);
            //var queryJson = JsonConvert.SerializeObject(query, Formatting.None);

            //var steamFilter = new SimplePredicate(VnConstants.Filters.Release, Operators.Matching.IsEqual, new SimplePredicate(ReleaseConstants.Filters.ExtLink, Operators.Matching.IsEqual, new object[] { ExtLinks.Release.Steam, 888790 }));
            //var steamquery = new VndbQuery(steamFilter, requestSettings);
            //var steamqueryJson = JsonConvert.SerializeObject(steamquery, Formatting.None);

            //var attribute = new ProducerA.FilterStringAttribute("someValue");
            //string value = attribute.GetValue();
            //var ssdsd = FilterFlags.Invertible;
            //var builder = new VNDB.Models.StandardPredicateBuilder<VNDB.Enums.SpoilerLevelEnum, int, int>("images");
            //var predicate = builder.EqualTo(VNDB.Enums.SpoilerLevelEnum.Major, 2, 3);
            ////var ssss = new SimplePredicate(ProducerFilters.Lang, );

            //var vnSearch = new StandardPredicateBuilder<string>("search");

            //var vnLangSearch = VnRequests.Lang;
            //var vnLangSearchP = VnRequests.Lang.EqualTo("english");

            //vnSearch.--------------------
            //var vnSearchPredicate = vnSearch.EqualTo("My vn name");

            //var vnSearchPredicate = vnSearch("My vn name").AreEqual();
            //var vnSearchPredicate = vnSearch("My vn name").EqualTo();

            //var vnIdPredicate = vnIdSearch(23).IsGreater();
        }

    }
}