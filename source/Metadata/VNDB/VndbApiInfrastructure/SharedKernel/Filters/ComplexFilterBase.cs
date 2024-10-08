﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApiInfrastructure.SharedKernel.Filters
{
    public class ComplexFilterBase<T> : IFilter
    {
        public readonly string Predicate;
        public IFilter[] Filters;

        public ComplexFilterBase(string predicate, params IFilter[] filters)
        {
            Predicate = predicate;
            Filters = filters;
        }

        public string ToJsonString()
        {
            var sb = new StringBuilder();
            sb.Append("[ ");
            sb.Append($"\"{Predicate}\", ");

            var predicatesStrings = new List<string>();
            foreach (var predicate in Filters)
            {
                predicatesStrings.Add(predicate.ToJsonString());
            }

            sb.Append(string.Join(", ", predicatesStrings));
            sb.Append(" ]");
            return sb.ToString();
        }
    }
}
