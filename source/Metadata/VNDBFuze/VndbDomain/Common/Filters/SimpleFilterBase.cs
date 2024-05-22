using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;
using VNDBFuze.VndbDomain.Common.Interfaces;
using VNDBFuze.VndbDomain.Common.Utilities;

namespace VNDBFuze.VndbDomain.Common.Filters
{
    public class SimpleFilterBase<T> : IFilter
    {
        public readonly string Name;
        public readonly string Operator;
        public readonly object Value;

        public SimpleFilterBase(string name, string op, string value)
        {
            Name = name;
            Operator = op;
            Value = value;
        }

        public SimpleFilterBase(string name, string op, object value)
        {
            Name = name;
            Operator = op;
            Value = value;
        }

        public SimpleFilterBase(string name, string op, SimpleFilterBase<T> value)
        {
            Name = name;
            Operator = op;
            Value = value;
        }

        public SimpleFilterBase(string name, string op, ComplexFilterBase<T> value)
        {
            Name = name;
            Operator = op;
            Value = value;
        }

        public SimpleFilterBase(string name, string op, params object[] parameters)
        {
            Name = name;
            Operator = op;
            Value = parameters;
        }

        public string ToJsonString()
        {
            var sb = new StringBuilder();
            sb.Append("[ ");
            sb.Append($"\"{Name}\",");
            sb.Append($"\"{Operator}\", ");
            if (Value is object[] objectsArray)
            {
                var arraySb = new StringBuilder();
                arraySb.Append("[ ");
                foreach (object item in objectsArray)
                {
                    AppendValueToBuilder(arraySb, item);
                    arraySb.Append(", ");
                }

                // Remove the trailing comma and space, if present
                if (arraySb.Length >= 2)
                {
                    arraySb.Length -= 2; // Remove the last two characters
                }

                arraySb.Append(" ]");
                sb.Append(arraySb.ToString());
            }
            else
            {
                AppendValueToBuilder(sb, Value);
            }

            sb.Append(" ]");
            return sb.ToString();
        }

        private void AppendValueToBuilder(StringBuilder sb, object item)
        {
            if (item is null)
            {
                sb.Append("null");
            }
            if (item is string str)
            {
                sb.Append($"\"{str}\"");
            }
            else if (item is int intValue)
            {
                sb.Append($"{intValue}");
            }
            else if (item is uint uintValue)
            {
                sb.Append($"{uintValue}");
            }
            else if (item is double doubleValue)
            {
                sb.Append($"{doubleValue}");
            }
            else if (item is IFilter filter)
            {
                var predStr = filter.ToJsonString();
                sb.Append(predStr);
            }
            else if (item is Enum enumValue)
            {
                //var enumValue = (Enum)item;
                //var enumType = enumValue.GetType();
                //var memberInfo = enumType.GetMember(enumValue.ToString());
                //if (memberInfo.Length > 0)

                var memberInfo = EnumUtilities.GetEnumMemberInfo(enumValue);
                if (memberInfo != null)
                {
                    var stringRepresentationAttribute = memberInfo.GetCustomAttribute<StringRepresentationAttribute>();
                    if (stringRepresentationAttribute != null)
                    {
                        sb.Append($"\"{stringRepresentationAttribute.Value}\"");
                    }

                    var intRepresentationAttribute = memberInfo.GetCustomAttribute<IntRepresentationAttribute>();
                    if (intRepresentationAttribute != null)
                    {
                        sb.Append($"{intRepresentationAttribute.Value}");
                    }
                }
            }
        }
    }
}
