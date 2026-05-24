using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Domain.Displays.Entities
{
    public sealed class DisplayConfiguration
    {
        public string DisplayId { get; }

        public DisplayState State { get; }

        public bool SetAsPrimary { get; }

        public DisplayConfiguration(
            string displayId,
            DisplayState state,
            bool setAsPrimary)
        {
            DisplayId = displayId;
            State = state;
            SetAsPrimary = setAsPrimary;
        }
    }
}
