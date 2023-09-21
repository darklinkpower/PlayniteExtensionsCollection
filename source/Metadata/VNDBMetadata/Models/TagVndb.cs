using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Playnite.SDK.Data;
using SqlNado;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.Models
{
    public class Tag
    {
        [SerializationPropertyName("aliases")]
        public string[] Aliases { get; set; }

        [SerializationPropertyName("applicable")]
        public bool Applicable { get; set; }

        [SerializationPropertyName("cat")]
        public TagCategory Cat { get; set; }

        [SerializationPropertyName("description")]
        public string Description { get; set; }

        [SerializationPropertyName("id"), SQLiteColumn(IsPrimaryKey = true)]
        public int Id { get; set; }

        [SerializationPropertyName("meta")]
        public bool Meta { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("parents")]
        public int[] Parents { get; set; }

        [SerializationPropertyName("searchable")]
        public bool Searchable { get; set; }

        [SerializationPropertyName("vns")]
        public int Vns { get; set; }
    }

    public enum TagCategory { Cont, Ero, Tech };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                CatConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class CatConverter : JsonConverter
    {
        public static readonly CatConverter Singleton = new CatConverter();
        public override bool CanConvert(Type t) => t == typeof(TagCategory) || t == typeof(TagCategory?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "cont":
                    return TagCategory.Cont;
                case "ero":
                    return TagCategory.Ero;
                case "tech":
                    return TagCategory.Tech;
            }

            throw new Exception("Cannot unmarshal type Cat");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TagCategory)untypedValue;
            switch (value)
            {
                case TagCategory.Cont:
                    serializer.Serialize(writer, "cont");
                    return;
                case TagCategory.Ero:
                    serializer.Serialize(writer, "ero");
                    return;
                case TagCategory.Tech:
                    serializer.Serialize(writer, "tech");
                    return;
            }

            throw new Exception("Cannot marshal type Cat");
        }
    }
}
