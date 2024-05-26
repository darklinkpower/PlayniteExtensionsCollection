using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.VisualNovelAggregate;

namespace VndbApiDomain.SharedKernel
{
    public class VndbReleaseDateJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VisualNovelReleaseDate);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var releaseDate = (VisualNovelReleaseDate)value;
            writer.WriteValue(releaseDate.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dateString = reader.Value?.ToString();
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return null;
            }

            var parts = dateString.Split('-');
            if (parts.Length < 1 || parts.Length > 3)
            {
                throw new JsonSerializationException("Invalid date format.");
            }

            if (!int.TryParse(parts[0], out int year))
            {
                return null;
            }

            int? month = null;
            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
            {
                if (int.TryParse(parts[1], out int monthValue))
                {
                    month = monthValue;
                }
                else
                {
                    throw new JsonSerializationException("Month must be a valid integer.");
                }
            }

            int? day = null;
            if (month.HasValue && parts.Length > 2 && !string.IsNullOrEmpty(parts[2]))
            {
                if (int.TryParse(parts[2], out int dayValue))
                {
                    day = dayValue;
                }
                else
                {
                    throw new JsonSerializationException("Day must be a valid integer.");
                }
            }

            return new VisualNovelReleaseDate
            {
                Year = year,
                Month = month,
                Day = day
            };
        }
    }
}
