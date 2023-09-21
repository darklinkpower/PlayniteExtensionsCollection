using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.Interfaces;

namespace VNDBMetadata.Models
{
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

            var predicatesStrings = new List<string>();
            foreach (var predicate in Predicates)
            {
                predicatesStrings.Add(predicate.ToJsonString());
            }

            sb.Append(string.Join(", ", predicatesStrings));
            sb.Append(" ]");
            return sb.ToString();
        }
    }
}
