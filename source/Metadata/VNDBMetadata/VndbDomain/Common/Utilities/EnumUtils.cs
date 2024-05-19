using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Attributes;

namespace VNDBMetadata.VndbDomain.Common.Utilities
{
    internal static class EnumUtils
    {
        public static TEnum GetEnumRepresentation<TEnum>(string value) where TEnum : Enum
        {
            foreach (TEnum enumValue in Enum.GetValues(typeof(TEnum)))
            {
                var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
                var attributes = (StringRepresentationAttribute[])fieldInfo.GetCustomAttributes(typeof(StringRepresentationAttribute), false);
                if (attributes.Length > 0 && attributes[0].Value == value)
                {
                    return enumValue;
                }
            }

            throw new ArgumentException($"Unknown string representation: {value}");
        }

        public static string GetStringRepresentation<TEnum>(TEnum enumValue, string prefixString = "") where TEnum : Enum
        {
            var type = typeof(TEnum);
            var memberInfo = type.GetMember(enumValue.ToString());
            if (memberInfo.Length > 0)
            {
                var attributes = memberInfo[0].GetCustomAttributes(typeof(StringRepresentationAttribute), false);
                if (attributes.Length > 0)
                {
                    return $"{prefixString}{((StringRepresentationAttribute)attributes[0]).Value}";
                }
            }

            return $"{prefixString}{enumValue}";
        }

        public static List<string> GetStringRepresentations<TEnum>(TEnum enumValue, string prefixString = "") where TEnum : Enum
        {
            var type = typeof(TEnum);
            if (!type.GetCustomAttributes(typeof(FlagsAttribute), false).Any())
            {
                throw new InvalidOperationException($"The enum type {type.Name} must have the Flags attribute.");
            }

            ulong enumValueAsULong = Convert.ToUInt64(enumValue);
            var result = new List<string>();
            foreach (TEnum field in Enum.GetValues(type))
            {
                ulong fieldValue = Convert.ToUInt64(field);
                if (fieldValue != 0 && (enumValueAsULong & fieldValue) == fieldValue)
                {
                    result.Add(GetStringRepresentation(field, prefixString));
                }
            }

            return result;
        }

        public static string GetCommaSeparatedStringRepresentations<TEnum>(TEnum enumValue, string prefixString = "") where TEnum : Enum
        {
            var stringRepresentations = GetStringRepresentations(enumValue, prefixString);
            return string.Join(",", $"{stringRepresentations}");
        }
    }

}