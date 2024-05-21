using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.ImageAggregate;
using VNDBMetadata.VndbDomain.Aggregates.ReleaseAggregate;
using VNDBMetadata.VndbDomain.Aggregates.TraitAggregate;
using VNDBMetadata.VndbDomain.Aggregates.VnAggregate;
using VNDBMetadata.VndbDomain.Common.Converters;
using VNDBMetadata.VndbDomain.Common.Enums;
using VNDBMetadata.VndbDomain.Common.Utilities;

namespace VNDBMetadata.VndbDomain.Aggregates.CharacterAggregate
{
    public class Character
    {
        [JsonProperty("birthday")]
        [JsonConverter(typeof(CharacterBirthdayConverter))]
        public CharacterBirthday Birthday { get; set; }

        [JsonProperty("sex")]
        [JsonConverter(typeof(CharacterSexConverter))]
        public CharacterSex Sex { get; set; }

        [JsonProperty("cup")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<CharacterCupSizeEnum>))]
        public CharacterCupSizeEnum? Cup { get; set; }

        [JsonProperty("blood_type")]
        public CharacterBloodTypeEnum? BloodType { get; set; }

        [JsonProperty("image")]
        public VndbImage Image { get; set; }

        [JsonProperty("traits")]
        public List<CharacterTrait> Traits { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("vns")]
        public List<CharacterVn> VisualNovelApperances { get; set; }

        [JsonProperty("bust")]
        public int? Bust { get; set; }

        [JsonProperty("waist")]
        public int? Waist { get; set; }

        [JsonProperty("hips")]
        public int? Hips { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("aliases")]
        public List<string> Aliases { get; set; }

        [JsonProperty("height")]
        public int? Height { get; set; }

        [JsonProperty("weight")]
        public int? Weight { get; set; }

        [JsonProperty("age")]
        public int? Age { get; set; }

        [JsonProperty("original")]
        public string Original { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class CharacterBirthdayConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
            }
            else
            {
                CharacterBirthday birthday = (CharacterBirthday)value;
                writer.WriteStartArray();
                writer.WriteValue(birthday.Month);
                writer.WriteValue(birthday.Day);
                writer.WriteEndArray();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray array = JArray.Load(reader);
                if (array.Count == 2 && array[0].Type == JTokenType.Integer && array[1].Type == JTokenType.Integer)
                {
                    int month = array[0].ToObject<int>();
                    int day = array[1].ToObject<int>();
                    return new CharacterBirthday { Month = month, Day = day };
                }
            }

            throw new JsonSerializationException("Unexpected token or value when parsing birthday.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CharacterBirthday);
        }
    }

    public class CharacterSexConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is CharacterSex characterSex)
            {
                writer.WriteStartArray();
                if (characterSex.Apparent.HasValue)
                {
                    EnumUtilities.GetEnumStringRepresentation(characterSex.Apparent.Value);
                }
                else
                {
                    writer.WriteNull();
                }

                if (characterSex.Real.HasValue)
                {
                    EnumUtilities.GetEnumStringRepresentation(characterSex.Real.Value);
                }
                else
                {
                    writer.WriteNull();
                }

                writer.WriteEndArray();
            }

            writer.WriteNull();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray array = JArray.Load(reader);
                if (array.Count != 2)
                {
                    throw new JsonSerializationException("Expected an array with exactly two elements.");
                }

                var firstValue = array[0];
                var secondValue = array[1];

                CharacterSexEnum? apparentSex = null;
                CharacterSexEnum? realSex = null;

                if (firstValue.Type == JTokenType.String)
                {
                    apparentSex = EnumUtilities.GetStringEnumRepresentation<CharacterSexEnum>(firstValue.ToString());
                }
                else if (firstValue.Type != JTokenType.Null)
                {
                    throw new JsonSerializationException("Expected a string or null for apparent sex.");
                }

                if (secondValue.Type == JTokenType.String)
                {
                    realSex = EnumUtilities.GetStringEnumRepresentation<CharacterSexEnum>(secondValue.ToString());
                }
                else if (secondValue.Type != JTokenType.Null)
                {
                    throw new JsonSerializationException("Expected a string or null for real sex.");
                }

                return new CharacterSex { Apparent = apparentSex, Real = realSex };
            }

            throw new JsonSerializationException("Unexpected token or value when parsing character sex.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CharacterSex);
        }
    }


}