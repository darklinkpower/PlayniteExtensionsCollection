using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Aggregates.TraitAggregate;
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

        [JsonProperty("traits")]
        public List<Trait> Traits { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("vns")]
        public List<Vn> VisualNovelApperances { get; set; }

        [JsonProperty("bust")]
        public long? Bust { get; set; }

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
    }

    public class Vn
    {
        [JsonProperty("role")]
        [JsonConverter(typeof(StringRepresentationEnumConverter<CharacterRoleEnum>))]
        public CharacterRoleEnum Role { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("spoiler")]
        public SpoilerLevel Spoiler { get; set; }
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
                writer.WriteValue(EnumUtils.GetStringRepresentation(characterSex.Apparent));
                writer.WriteValue(EnumUtils.GetStringRepresentation(characterSex.Real));
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
                if (array.Count == 2 && array[0].Type == JTokenType.String && array[1].Type == JTokenType.String)
                {
                    var apparent = EnumUtils.GetEnumRepresentation<CharacterSexEnum>(array[0].ToString());
                    var real = EnumUtils.GetEnumRepresentation<CharacterSexEnum>(array[1].ToString());
                    return new CharacterSex { Apparent = apparent, Real = real };
                }
            }

            throw new JsonSerializationException("Unexpected token or value when parsing sex.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CharacterSex);
        }
    }


}