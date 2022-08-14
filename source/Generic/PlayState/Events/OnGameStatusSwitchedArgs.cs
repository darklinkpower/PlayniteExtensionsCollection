using PlayState.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayState.Events
{
    public class OnGameStatusSwitchedArgs
    {
        /// <summary>
        /// Gets PlayState Data initiating the event.
        /// </summary>
        public PlayStateData PlayStateData { get; internal set; }
    }
}