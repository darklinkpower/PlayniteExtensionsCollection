using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.MetadataProviders
{
    public class LogoDownloadOptions
    {
        public bool IsBackgroundDownload { get; private set; }

        public LogoDownloadOptions(bool isBackgroundDownload)
        {
            IsBackgroundDownload = isBackgroundDownload;
        }
    }

    interface ILogoProvider
    {
        string Id { get; }
        string GetLogoUrl(Game game, LogoDownloadOptions downloadOptions, CancellationToken cancelToken);
    }
}
