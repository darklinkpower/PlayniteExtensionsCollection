﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Enums
{
    /// <summary>
    /// Specifies the angle of the screen.
    /// </summary>   
    public enum ScreenOrientation
    {
        /// <summary>
        /// The screen is oriented at 0 degrees.
        /// </summary>
        Angle0 = 0,
        /// <summary>
        ///     The screen is oriented at 90 degrees
        /// </summary>
        Angle90 = 1,
        /// <summary>
        /// The screen is oriented at 180 degrees.
        /// </summary>
        Angle180 = 2,
        /// <summary>
        /// The screen is oriented at 270 degrees.
        /// </summary>
        Angle270 = 3
    }
}
