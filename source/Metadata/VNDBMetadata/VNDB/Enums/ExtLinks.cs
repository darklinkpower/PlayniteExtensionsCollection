using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDB.ApiConstants;

namespace VNDBMetadata.VNDB.Enums
{
    public enum Release
    {
        /// <summary>
        /// Steam
        /// Url format: https://store.steampowered.com/app/%d/
        /// </summary>
        [StringRepresentation(ExtLinks.Release.Steam)]
        Steam,

        /// <summary>
        /// Gyutto
        /// Url format: https://gyutto.com/i/item%d
        /// </summary>
        [StringRepresentation(ExtLinks.Release.Gyutto)]
        Gyutto,

        /// <summary>
        /// Digiket
        /// Url format: https://www.digiket.com/work/show/_data/ID=ITM%07d/
        /// </summary>
        [StringRepresentation(ExtLinks.Release.Digiket)]
        Digiket,
    }

}
