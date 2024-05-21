using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.VndbDomain.Aggregates.CharacterAggregate
{
    public class CharacterSex
    {
        public CharacterSexEnum? Apparent { get; set; }
        public CharacterSexEnum? Real { get; set; }
    }
}