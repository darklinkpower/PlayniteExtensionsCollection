using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Domain.CharacterAggregate
{
    public enum CharacterCupSizeEnum
    {
        /// <summary>
        /// None
        /// </summary>
        [StringRepresentation(null)]
        None,

        /// <summary>
        /// String, possibly null, "AAA".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.AAA)]
        AAA,

        /// <summary>
        /// String, possibly null, "AA".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.AA)]
        AA,

        /// <summary>
        /// String, possibly null, "A".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.A)]
        A,

        /// <summary>
        /// String, possibly null, "B".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.B)]
        B,

        /// <summary>
        /// String, possibly null, "C".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.C)]
        C,

        /// <summary>
        /// String, possibly null, "D".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.D)]
        D,

        /// <summary>
        /// String, possibly null, "E".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.E)]
        E,

        /// <summary>
        /// String, possibly null, "F".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.F)]
        F,

        /// <summary>
        /// String, possibly null, "G".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.G)]
        G,

        /// <summary>
        /// String, possibly null, "H".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.H)]
        H,

        /// <summary>
        /// String, possibly null, "I".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.I)]
        I,

        /// <summary>
        /// String, possibly null, "J".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.J)]
        J,

        /// <summary>
        /// String, possibly null, "K".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.K)]
        K,

        /// <summary>
        /// String, possibly null, "L".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.L)]
        L,

        /// <summary>
        /// String, possibly null, "M".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.M)]
        M,

        /// <summary>
        /// String, possibly null, "N".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.N)]
        N,

        /// <summary>
        /// String, possibly null, "O".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.O)]
        O,

        /// <summary>
        /// String, possibly null, "P".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.P)]
        P,

        /// <summary>
        /// String, possibly null, "Q".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.Q)]
        Q,

        /// <summary>
        /// String, possibly null, "R".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.R)]
        R,

        /// <summary>
        /// String, possibly null, "S".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.S)]
        S,

        /// <summary>
        /// String, possibly null, "T".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.T)]
        T,

        /// <summary>
        /// String, possibly null, "U".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.U)]
        U,

        /// <summary>
        /// String, possibly null, "V".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.V)]
        V,

        /// <summary>
        /// String, possibly null, "W".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.W)]
        W,

        /// <summary>
        /// String, possibly null, "X".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.X)]
        X,

        /// <summary>
        /// String, possibly null, "Y".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.Y)]
        Y,

        /// <summary>
        /// String, possibly null, "Z".
        /// </summary>
        [StringRepresentation(CharacterConstants.CupSize.Z)]
        Z
    }

}
