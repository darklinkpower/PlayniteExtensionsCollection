using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.VNDB.Models
{
    public interface IPredicate
    {
        string ToJsonString();
    }

    public class SimplePredicate : IPredicate
    {
        public readonly string Name;
        public readonly string Operator;
        public readonly object Value;

        public SimplePredicate(string name, string op, string value)
        {
            Name = name;
            Operator = op;
            Value = value;
        }

        public SimplePredicate(string name, string op, SimplePredicate value)
        {
            Name = name;
            Operator = op;
            Value = value;
        }

        public SimplePredicate(string name, string op, ComplexPredicate value)
        {
            Name = name;
            Operator = op;
            Value = value;
        }

        public SimplePredicate(string name, string op, params object[] parameters)
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
            if (item is string str)
            {
                sb.Append($"\"{str}\"");
            }
            else if (item is int numValue)
            {
                sb.Append($"{numValue}");
            }
            else if (item is SimplePredicate simplePredicate)
            {
                var predStr = simplePredicate.ToString();
                sb.Append(predStr);
            }
            else if (item is ComplexPredicate complexPredicate)
            {
                var predStr = complexPredicate.ToString();
                sb.Append(predStr);
            }
        }
    }

    public class ComplexPredicate : IPredicate
    {
        public readonly string Operator;
        public IPredicate[] Predicates;

        public ComplexPredicate(string op, params IPredicate[] predicates)
        {
            Operator = op;
            Predicates = predicates;
        }

        public string ToJsonString()
        {
            var sb = new StringBuilder();
            sb.Append("[ ");
            sb.Append($"\"{Operator}\", ");

            var predicatesStrings = Predicates.Select(p => p.ToJsonString()).ToList();
            sb.Append(string.Join(", ", predicatesStrings));
            sb.Append(" ]");
            return sb.ToString();
        }
    }
}