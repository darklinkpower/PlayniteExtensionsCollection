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
using VNDB.ApiConstants;
using WebCommon;
using VNDBMetadata.VNDB.Requests.Post;

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
            var requestSettings = new VisualNovelFieldsSettings();
            var request = requestSettings.ToString();

            var predicateOne = new SimplePredicate("Name 1", Operators.OrderingOperator.GreaterOrEqual, "string");
            var predicateTwo = new SimplePredicate("Name 1", Operators.OrderingOperator.GreaterOrEqual, predicateOne);

            var sss = Operators.OrderingOperator.GreaterOrEqual;
            var predicate1 = new ComplexPredicate(Operators.Predicates.Or,
                new SimplePredicate(Release.Filters.Lang, Operators.OrderingOperator.GreaterOrEqual, QueryEnums.Language.English),
                new SimplePredicate(Release.Filters.Lang, Operators.Matching.IsEqual, QueryEnums.Language.German),
                new SimplePredicate(Release.Filters.Lang, Operators.Matching.IsEqual, QueryEnums.Language.French));

            var predicate2 = new SimplePredicate(Vn.Filters.OriginalLanguage, Operators.Inverting.NotEqual, QueryEnums.Language.Japanese);

            var releaseCondition = new ComplexPredicate(Operators.Predicates.Or,
                new SimplePredicate(Release.Filters.Released, Operators.Ordering.GreaterOrEqual, "2020-01-01"),
                new SimplePredicate(Release.Filters.Producer, Operators.Matching.IsEqual, new SimplePredicate(Producer.Filters.Id, Operators.Matching.IsEqual, "p30")));
            var predicate3 = new SimplePredicate(Vn.Filters.Release, Operators.Matching.IsEqual, releaseCondition);

            var complexFilter = new ComplexPredicate(Operators.Predicates.And,
                predicate1,
                predicate2,
                predicate3);

            var query = new VndbQuery(complexFilter, requestSettings);
            var queryJson = JsonConvert.SerializeObject(query, Formatting.None);

            var steamFilter = new SimplePredicate(Vn.Filters.Release, Operators.Matching.IsEqual, new SimplePredicate(Release.Filters.ExtLink, Operators.Matching.IsEqual, new object[] { ExtLinks.Release.Steam, 888790 }));
            var steamquery = new VndbQuery(steamFilter, requestSettings);
            var steamqueryJson = JsonConvert.SerializeObject(steamquery, Formatting.None);

            var attribute = new ProducerA.FilterStringAttribute("someValue");
            string value = attribute.GetValue();
            var ssdsd = FilterFlags.Invertible;
            var builder = new VNDB.Models.StandardPredicateBuilder<VNDB.Enums.SpoilerLevelEnum, int, int>("images");
            var predicate = builder.EqualTo(VNDB.Enums.SpoilerLevelEnum.Major, 2, 3);
            //var ssss = new SimplePredicate(ProducerFilters.Lang, );

            var vnSearch = new StandardPredicateBuilder<string>("search");

            var vnLangSearch = VnRequests.Lang;
            var vnLangSearchP = VnRequests.Lang.EqualTo("english");
            //vnSearch.
            //var vnSearchPredicate = vnSearch.EqualTo("My vn name");

            //var vnSearchPredicate = vnSearch("My vn name").AreEqual();
            //var vnSearchPredicate = vnSearch("My vn name").EqualTo();

            //var vnIdPredicate = vnIdSearch(23).IsGreater();
        }

        public class StandardPredicateBuilder<T>
        {
            protected readonly string _name;

            public StandardPredicateBuilder(string name)
            {
                _name = name;
            }

            public SimplePredicate EqualTo(T value)
            {
                var predicateOperator = Operators.Matching.IsEqual;
                return new SimplePredicate(_name, predicateOperator, value);
            }

            public SimplePredicate NotEqualTo(T value)
            {
                var predicateOperator = Operators.Inverting.NotEqual;
                return new SimplePredicate(_name, predicateOperator, value);
            }
        }

        public class OrderingPredicateBuilder<T> : StandardPredicateBuilder<T>
        {

            public OrderingPredicateBuilder(string name) : base(name)
            {

            }

            public SimplePredicate GreaterThan(T value)
            {
                var predicateOperator = Operators.Ordering.GreaterThan;
                return new SimplePredicate(_name, predicateOperator, value);
            }

            public SimplePredicate GreaterOrEqual(T value)
            {
                var predicateOperator = Operators.Ordering.GreaterOrEqual;
                return new SimplePredicate(_name, predicateOperator, value);
            }

            public SimplePredicate LessThan(T value)
            {
                var predicateOperator = Operators.Ordering.LessThan;
                return new SimplePredicate(_name, predicateOperator, value);
            }

            public SimplePredicate LessThanOrEqual(T value)
            {
                var predicateOperator = Operators.Inverting.LessThanOrEqual;
                return new SimplePredicate(_name, predicateOperator, value);
            }
        }

        public class SimplePredicateA<TFilters, TFields> where TFilters : Enum
        {
            private TFilters _filter;
            private TFields[] _fields;
            private OperatorFlags _operatorFlags;

            public SimplePredicateA(TFilters filter, OperatorFlags operatorFlags, params TFields[] fields)
            {
                _filter = filter;
                _fields = fields;
                _operatorFlags = operatorFlags;
            }

            public void GreatherThan()
            {

            }
        }

        public static class ProducerFilters
        {
            public static Filter Id = new Filter(Producer.Filters.Id, FilterFlags.Ordering | FilterFlags.NullAccepting);
            public static Filter Search = new Filter(Producer.Filters.Id, FilterFlags.MultiEntryMatch | FilterFlags.Invertible);
            public static Filter Lang = new Filter(Producer.Filters.Id, FilterFlags.NullAccepting);
            public static Filter Type = new Filter(Producer.Filters.Id, FilterFlags.Invertible);
        }

        public class Filter
        {
            public string Name { get; }
            public FilterFlags FilterFlags { get; }

            public Filter(string name, FilterFlags filterFlags)
            {
                Name = name;
                FilterFlags = filterFlags;
            }
        }

        public static class Language
        {
            public const string Arabic = "ar";
            public const string Basque = "eu";
            public const string Bulgarian = "bg";
        }

        public class ProducerA
        {
            public class SimplePredicateB<TFilters, TFields> where TFilters : Enum
            {
                private TFilters _filter;
                private TFields[] _fields;
                private OperatorFlags _operatorFlags;

                public SimplePredicateB(TFilters filter, OperatorFlags operatorFlags, params TFields[] fields)
                {
                    _filter = filter;
                    _fields = fields;
                    _operatorFlags = operatorFlags;
                }
            }

            public enum ProducerFilters
            {
                [FilterString(Producer.Fields.Id)]
                Id,
                [FilterString(Producer.Fields.Lang)]
                Lang,
                [FilterString(Producer.Fields.Type)]
                Type
            }

            public class FilterStringAttribute : Attribute
            {
                public string Value { get; }

                public FilterStringAttribute(string value)
                {
                    Value = value;
                }

                public string GetValue()
                {
                    return Value;
                }
            }

            public class FieldStringAttribute : Attribute
            {
                public string Value { get; }

                public FieldStringAttribute(string value)
                {
                    Value = value;
                }
            }

            public enum ProducerFields
            {
                Id,
                Name,
                Original,
                Aliases,
                Lang,
                Type,
                Description
            }
        }

        public class Filter<TFilters, TFields> where TFilters : Enum
        {
            private TFilters _filter;
            private TFields[] _fields;
            private OperatorFlags _operatorFlags;

            public Filter(TFilters filter, OperatorFlags operatorFlags, params TFields[] fields)
            {
                _filter = filter;
                _fields = fields;
                _operatorFlags = operatorFlags;
            }
        }

        [Flags]
        public enum OperatorFlags : uint
        {
            None = 0x0000,
            [FilterFlag("o")]
            Ordering = 0x0001,
            [FilterFlag("n")]
            NullAccepting = 0x0002,
            [FilterFlag("m")]
            MultiEntryMatch = 0x0004,
            [FilterFlag("i")]
            Invertible = 0x0008
        }

        [Flags]
        public enum FilterFlags
        {
            None = 0x0000,
            Ordering = 0x0001,
            NullAccepting = 0x0002,
            MultiEntryMatch = 0x0004,
            Invertible = 0x0008
        }

        public enum CharacterFilters
        {
            Id = FilterFlags.Ordering | FilterFlags.NullAccepting,
            Search = FilterFlags.MultiEntryMatch | FilterFlags.Invertible,
            Lang = FilterFlags.NullAccepting,
            Type = FilterFlags.Invertible
        }



        public class FilterFlagAttribute : Attribute
        {
            public string Value { get; }

            public FilterFlagAttribute(string value)
            {
                Value = value;
            }
        }
    }
}