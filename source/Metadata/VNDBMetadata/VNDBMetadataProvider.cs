using ComposableAsync;
using Flurl.Http;
using Playnite.SDK.Plugins;
using RateLimiter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata
{
    public class VNDBMetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions options;
        private readonly VNDBMetadata plugin;

        public override List<MetadataField> AvailableFields => new List<MetadataField> { MetadataField.Description };

        public VNDBMetadataProvider(MetadataRequestOptions options, VNDBMetadata plugin)
        {
            this.options = options;
            this.plugin = plugin;
        }

        // Override additional methods based on supported metadata fields.
        public override string GetDescription(GetMetadataFieldArgs args)
        {
            var cts = args.CancelToken;
            DownloadExtensions.DownloadFileAsync(ApiConstants.DatabaseDumps.TagsUrl, "somefolder");
            

            //args.CancelToken
            return base.GetDescription(args);
        }

        private async void test()
        {
            var cts = new System.Threading.CancellationToken();
            var client = new FlurlClient();
            var request = client.Request();
            request.Url = "";
            var ree = request.GetStringAsync(cts);
            await ree;
            if (ree.Status != TaskStatus.RanToCompletion)
            {
                return;
            }

            var constraint1 = new CountByIntervalAwaitableConstraint(360, TimeSpan.FromHours(1));
            var constraint2 = new CountByIntervalAwaitableConstraint(1, TimeSpan.FromSeconds(1));

            var limiter = TimeLimiter.Compose(constraint1, constraint2).AsDelegatingHandler();

            var text = await "http://site.com/readme.txt".GetStringAsync(cts);
            var text2 = await "http://api.com/endpoint".GetJsonAsync<MetadataField>(cts);
            var ss = text.DownloadFileAsync(ApiConstants.DatabaseDumps.TagsUrl, "somefolder");
            var sws = DownloadExtensions.DownloadFileAsync(ApiConstants.DatabaseDumps.TagsUrl, "somefolder");
        }

    }
}