using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Common.Utilities
{
    internal static class EnumUtilities
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Enum, MemberInfo>> _enumMemberInfoCache =
            new ConcurrentDictionary<Type, ConcurrentDictionary<Enum, MemberInfo>>();
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Enum>> _stringEnumRepresentationCache =
            new ConcurrentDictionary<Type, ConcurrentDictionary<string, Enum>>();

        private static readonly ConcurrentDictionary<Type, Enum> _cachedAllEnumFlagsResults = new ConcurrentDictionary<Type, Enum>();

        internal static MemberInfo GetEnumMemberInfo<TEnum>(TEnum enumValue) where TEnum : Enum
        {
            var enumType = enumValue.GetType();
            if (!_enumMemberInfoCache.TryGetValue(enumType, out var valueDict))
            {
                valueDict = new ConcurrentDictionary<Enum, MemberInfo>();
                _enumMemberInfoCache[enumType] = valueDict;
            }

            if (!valueDict.TryGetValue(enumValue, out var memberInfo))
            {
                memberInfo = enumType.GetMember(enumValue.ToString()).FirstOrDefault();
                valueDict[enumValue] = memberInfo;
            }

            return memberInfo;
        }

        public static TEnum GetStringEnumRepresentation<TEnum>(string value) where TEnum : Enum
        {
            var enumType = typeof(TEnum);
            if (!_stringEnumRepresentationCache.TryGetValue(enumType, out var stringDict))
            {
                stringDict = new ConcurrentDictionary<string, Enum>();
                foreach (TEnum enumValue in Enum.GetValues(enumType))
                {
                    var fieldInfo = enumType.GetField(enumValue.ToString());
                    var attributes = (StringRepresentationAttribute[])fieldInfo.GetCustomAttributes(typeof(StringRepresentationAttribute), false);
                    if (attributes.Length > 0)
                    {
                        stringDict[attributes[0].Value] = enumValue;
                    }
                }

                _stringEnumRepresentationCache[enumType] = stringDict;
            }

            if (stringDict.TryGetValue(value, out var result))
            {
                return (TEnum)result;
            }

            throw new ArgumentException($"Unknown string representation: {value}");
        }

        public static string GetEnumStringRepresentation<TEnum>(TEnum enumValue, string prefixString = "") where TEnum : Enum
        {
            var memberInfo = GetEnumMemberInfo(enumValue);
            if (memberInfo != null)
            {
                var attributes = memberInfo.GetCustomAttributes(typeof(StringRepresentationAttribute), false);
                if (attributes.Length > 0)
                {
                    return $"{prefixString}{((StringRepresentationAttribute)attributes[0]).Value}";
                }
            }

            return $"{prefixString}{enumValue}";
        }

        public static int GetIntRepresentation<TEnum>(TEnum enumValue) where TEnum : Enum
        {
            var memberInfo = GetEnumMemberInfo(enumValue);
            if (memberInfo != null)
            {
                var attributes = memberInfo.GetCustomAttributes(typeof(IntRepresentationAttribute), false);
                if (attributes.Length > 0)
                {
                    return ((IntRepresentationAttribute)attributes[0]).Value;
                }
            }

            throw new ArgumentException($"Unknown int representation: {enumValue}");
        }

        public static List<string> GetStringRepresentations<TEnum>(TEnum enumValue, string prefixString = "") where TEnum : Enum
        {
            var type = typeof(TEnum);
            if (!type.GetCustomAttributes(typeof(FlagsAttribute), false).Any())
            {
                throw new InvalidOperationException($"The enum type {type.Name} must have the Flags attribute.");
            }

            var enumValueAsULong = Convert.ToUInt64(enumValue);
            var result = new List<string>();
            foreach (TEnum field in Enum.GetValues(type))
            {
                ulong fieldValue = Convert.ToUInt64(field);
                if (fieldValue != 0 && (enumValueAsULong & fieldValue) == fieldValue)
                {
                    result.Add(GetEnumStringRepresentation(field, prefixString));
                }
            }

            return result;
        }

        public static string GetCommaSeparatedStringRepresentations<TEnum>(TEnum enumValue, string prefixString = "") where TEnum : Enum
        {
            var stringRepresentations = GetStringRepresentations(enumValue, prefixString);
            return string.Join(",", $"{stringRepresentations}");
        }

        public static void SetAllEnumFlags<TEnum>(ref TEnum targetField) where TEnum : Enum
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum must be an enum type.");
            }

            if (!Attribute.IsDefined(typeof(TEnum), typeof(FlagsAttribute)))
            {
                throw new ArgumentException("TEnum must have the FlagsAttribute.");
            }

            var enumType = typeof(TEnum);
            if (!_cachedAllEnumFlagsResults.ContainsKey(enumType))
            {
                TEnum result = default;
                foreach (TEnum value in Enum.GetValues(enumType))
                {
                    var numValue = Convert.ToInt64(value);
                    var targetNumValue = Convert.ToInt64(result);
                    result = (TEnum)Enum.ToObject(enumType, numValue | targetNumValue);
                }

                _cachedAllEnumFlagsResults[enumType] = result;
            }

            targetField = (TEnum)_cachedAllEnumFlagsResults[enumType];
        }


    }

}