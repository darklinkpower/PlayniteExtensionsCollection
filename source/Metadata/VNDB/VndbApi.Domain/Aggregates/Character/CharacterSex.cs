using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApi.Domain.CharacterAggregate
{
    public class CharacterSex
    {
        public CharacterSexEnum? Apparent { get; set; }
        public CharacterSexEnum? Real { get; set; }

        public override string ToString()
        {
            var apparentString = Apparent.HasValue ? Apparent.Value.ToString() : "Not available";
            var realString = Real.HasValue ? Real.Value.ToString() : "Not available";
            return $"Apparent: {apparentString}, Real: {realString}";
        }


    }
}