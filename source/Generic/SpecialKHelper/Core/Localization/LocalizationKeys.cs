using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper
{
    public static class LOC
    {
        /// <summary>
        /// Global
        /// </summary>
        public const string SpecialKExecutionModeGlobal = "LOCSpecial_K_Helper_EnumDescriptionSpecialKExecutionModeGlobal";
        
        /// <summary>
        /// Selective
        /// </summary>
        public const string SpecialKExecutionModeSelective = "LOCSpecial_K_Helper_EnumDescriptionSpecialKExecutionModeSelective";
        
        /// <summary>
        /// Desktop
        /// </summary>
        public const string SteamOverlayDesktop = "LOCSpecial_K_Helper_EnumDescriptionSteamOverlayDesktop";
        /// <summary>
        /// Big Picture Mode
        /// </summary>
        public const string SteamOverlayBpm = "LOCSpecial_K_Helper_EnumDescriptionSteamOverlayBpm";

        /// <summary>
        /// After successful game injection
        /// </summary>
        public const string SpecialKServiceStopModeOnInjection =
            "LOCSpecial_K_Helper_SpecialKServiceStopModeOnInjection";

        /// <summary>
        /// When the game closes
        /// </summary>
        public const string SpecialKServiceStopModeOnGameStop =
            "LOCSpecial_K_Helper_SpecialKServiceStopModeOnGameStop";

        /// <summary>
        /// Never automatically stop
        /// </summary>
        public const string SpecialKServiceStopModeNever =
            "LOCSpecial_K_Helper_SpecialKServiceStopModeNever";
    }
}
