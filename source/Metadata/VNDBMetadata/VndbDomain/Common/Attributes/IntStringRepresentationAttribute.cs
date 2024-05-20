using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.VndbDomain.Common.Attributes
{
    public class IntRepresentationAttribute : Attribute
    {
        public int Value { get; }

        public IntRepresentationAttribute(int value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

}