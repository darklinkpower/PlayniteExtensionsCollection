using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.Interfaces
{
    interface ILogoProvider
    {
        string Id { get; }
        string GetLogoUrl(Game game, bool isBackgroundDownload, CancellationToken cancelToken);
    }
}
