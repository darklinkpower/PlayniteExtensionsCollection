using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using SqlNado;
using SqlNado.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using VNDBMetadata.Models;
using VNDBMetadata.QueryConstants;
using WebCommon;

namespace VNDBMetadata
{
    public class VNDBMetadata : MetadataPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private VNDBMetadataSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("39229206-1199-4fee-a014-e8478ea4cd77");

        public override List<MetadataField> SupportedFields { get; } = new List<MetadataField>
        {
            //MetadataField.Description
            // Include addition fields if supported by the metadata source
        };

        public override string Name => "VNDB Metadata";

        public VNDBMetadata(IPlayniteAPI api) : base(api)
        {
            settings = new VNDBMetadataSettingsViewModel(this);
            Properties = new MetadataPluginProperties
            {
                HasSettings = true
            };
        }

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            return new VNDBMetadataProvider(options, this);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new VNDBMetadataSettingsView();
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            //var requestSettings = new VisualNovelFieldsSettings();
            //var request = requestSettings.ToString();

            //var predicate1 = new ComplexPredicate(Operators.Predicates.Or,
            //    new SimplePredicate(Release.Filters.Lang, Operators.Matching.IsEqual, QueryEnums.Language.English),
            //    new SimplePredicate(Release.Filters.Lang, Operators.Matching.IsEqual, QueryEnums.Language.German),
            //    new SimplePredicate(Release.Filters.Lang, Operators.Matching.IsEqual, QueryEnums.Language.French));

            //var predicate2 = new SimplePredicate(Vn.Filters.OriginalLanguage, Operators.Inverting.NotEqual, QueryEnums.Language.Japanese);

            //var releaseCondition = new ComplexPredicate(Operators.Predicates.Or,
            //    new SimplePredicate(Release.Filters.Released, Operators.Ordering.GreaterOrEqual, "2020-01-01"),
            //    new SimplePredicate(Release.Filters.Producer, Operators.Matching.IsEqual, new SimplePredicate(Producer.Filters.Id, Operators.Matching.IsEqual, "p30")));
            //var predicate3 = new SimplePredicate(Vn.Filters.Release, Operators.Matching.IsEqual, releaseCondition);

            //var complexFilter = new ComplexPredicate(Operators.Predicates.And,
            //    predicate1,
            //    predicate2,
            //    predicate3);

            //var query = new VndbQuery(complexFilter, requestSettings);
            //var queryJson = JsonConvert.SerializeObject(query, Formatting.None);

            //var steamFilter = new SimplePredicate(Vn.Filters.Release, Operators.Matching.IsEqual, new SimplePredicate(Release.Filters.ExtLink, Operators.Matching.IsEqual, new object[] { ExtLinks.Release.Steam, 888790 }));
            //var steamquery = new VndbQuery(steamFilter, requestSettings);
            //var steamqueryJson = JsonConvert.SerializeObject(steamquery, Formatting.None);
        }
    }
}