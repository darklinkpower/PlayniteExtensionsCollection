using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.Interfaces;
using VNDBMetadata.VNDB.Enums;

namespace VNDBMetadata.Filters
{
    public abstract class SimpleFilterBase : IFilter
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

        public SimpleFilterBase(string name, string op, SimpleFilterBase value)
        {
            Name = name;
            Operator = op;
            Value = value;
        }

        public SimpleFilterBase(string name, string op, ComplexFilterBase value)
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
            else if (item is SimpleFilterBase simplePredicate)
            {
                var predStr = simplePredicate.ToJsonString();
                sb.Append(predStr);
            }
            else if (item is ComplexFilterBase complexPredicate)
            {
                var predStr = complexPredicate.ToJsonString();
                sb.Append(predStr);
            }
            else if (item.GetType().IsEnum)
            {
                var enumValue = (Enum)item;
                var enumType = enumValue.GetType();
                var memberInfo = enumType.GetMember(enumValue.ToString());
                if (memberInfo.Length > 0)
                {
                    var stringRepresentationAttribute = memberInfo[0].GetCustomAttribute<StringRepresentationAttribute>();
                    if (stringRepresentationAttribute != null)
                    {
                        sb.Append($"\"{stringRepresentationAttribute.Value}\"");
                    }
                }
            }
        }
    }
}
