using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBFuze.VndbDomain.Common.Attributes;

namespace VNDBFuze.VndbDomain.Aggregates.CharacterAggregate
{
    public enum CharacterBloodTypeEnum
    {
        /// <summary>
        /// String, possibly null, "a".
        /// </summary>
        [StringRepresentation(CharacterConstants.BloodType.A)]
        A,

        /// <summary>
        /// String, possibly null, "b".
        /// </summary>
        [StringRepresentation(CharacterConstants.BloodType.B)]
        B,

        /// <summary>
        /// String, possibly null, "ab".
        /// </summary>
        [StringRepresentation(CharacterConstants.BloodType.AB)]
        AB,

        /// <summary>
        /// String, possibly null, "o".
        /// </summary>
        [StringRepresentation(CharacterConstants.BloodType.O)]
        O
    }

}