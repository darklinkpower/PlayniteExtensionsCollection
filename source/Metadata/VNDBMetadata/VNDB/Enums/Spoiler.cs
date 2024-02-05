using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDB.ApiConstants;

namespace VNDBMetadata.VNDB.Enums
{
    public enum SpoilerLevelEnum : int
    {
        None = 0,
        Minimum = 1,
        Major = 2
    }

    public enum TagLevelEnum : int
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3
    }

    public enum ExtLink
    {
        [StringRepresentation(ExtLinks.Release.Steam)]
        Steam,
        [StringRepresentation(ExtLinks.Release.JASTUSA)]
        JastUsa
    }

    public class StringRepresentationAttribute : Attribute
    {
        public string Value { get; }

        public StringRepresentationAttribute(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}