using DisplayHelper.Domain.Displays.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHelper.Application.Displays.DTOs
{
    public sealed class ApplyDisplayConfigurationRequest
    {
        public string DisplayId { get; }

        public Resolution Resolution { get; }

        public RefreshRate RefreshRate { get; }

        public DisplayPosition DisplayPosition { get; }

        public bool SetAsPrimary { get; }

        public ApplyDisplayConfigurationRequest(
            string displayId,
            bool setAsPrimary,
            Resolution resolution = null,
            RefreshRate refreshRate = null,
            DisplayPosition displayPosition = null)
        {
            DisplayId = displayId;
            Resolution = resolution;
            RefreshRate = refreshRate;
            SetAsPrimary = setAsPrimary;
            DisplayPosition = displayPosition;
        }
    }
}
