using DisplayHelper.Application.Displays.DTOs;
using DisplayHelper.Domain.Displays.Entities;
using System.Collections.Generic;

namespace DisplayHelper.Application.Displays.Services
{
    public interface IDisplayTransactionService
    {
        DisplayTransactionResult Apply(
            IReadOnlyList<ApplyDisplayConfigurationRequest> configurations);
    }
}