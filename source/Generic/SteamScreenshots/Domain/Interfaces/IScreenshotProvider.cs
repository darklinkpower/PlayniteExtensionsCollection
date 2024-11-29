using SteamScreenshots.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamScreenshots.Domain.Interfaces
{
    public interface IScreenshotProvider
    {
        List<ScreenshotData> GetScreenshots(string id, CancellationToken cancellationToken = default);
    }
}