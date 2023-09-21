using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata
{
    public interface FilterDefinition
    {
        string op { get; }
        string name { get; }
        object value { get; }
    }
    
    public class FilterEquals
    {
        private const string op = "=";
        private readonly string name;
        private readonly object value;

        public FilterEquals(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"\"{name}\",");
            sb.Append($"\"{op}\", ");
            if (value is string str)
            {
                sb.Append($"{str}");
            }
            else if (value is FilterEquals filterEquals)
            {
                var str2 = filterEquals.ToString();
                sb.Append(str2);
            }

            sb.Append(" ]");
            return sb.ToString();
        }
    }


    public class FiltersExpression
    {
        private string Operator { get; set; }
        private List<object> Values { get; set; }

        public FiltersExpression(string op, params object[] values)
        {
            Operator = op;
            Values = new List<object>(values);
        }

        public FiltersExpression And(params FiltersExpression[] expressions)
        {
            Values.Add(new FiltersExpression("and", expressions));
            return this;
        }

        public FiltersExpression Or(params FiltersExpression[] expressions)
        {
            Values.Add(new FiltersExpression("or", expressions));
            return this;
        }

        public FiltersExpression Equals(string field, string value)
        {
            Values.Add(new FiltersExpression(field, "=", value));
            return this;
        }

        public FiltersExpression Equals(string field, FiltersExpression value)
        {
            Values.Add(new FiltersExpression(field, "=", value));
            return this;
        }

        public FiltersExpression NotEquals(string field, string value)
        {
            Values.Add(new FiltersExpression(field, "!=", value));
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[ \"").Append(Operator).Append("\"");
            foreach (var value in Values)
            {
                if (value is FiltersExpression expression)
                {
                    sb.Append(", ").Append(expression.ToString());
                }
                else if (value is string strValue)
                {
                    sb.Append(", [ \"").Append(strValue).Append("\" ]");
                }
            }

            sb.Append(" ]");
            return sb.ToString();
        }


    }
}