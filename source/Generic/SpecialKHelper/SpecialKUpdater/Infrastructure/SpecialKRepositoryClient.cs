using FlowHttp;
using Newtonsoft.Json;
using SpecialKHelper.SpecialKUpdater.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKUpdater.Infrastructure
{
    public class SpecialKRepositoryClient
    {
        private const string RepositoryUrl = "https://sk-data.special-k.info/repository.json";

        public SpecialKRepositoryClient()
        {

        }

        public async Task<RepositoryRoot> GetRepositoryAsync()
        {
            var url =
                RepositoryUrl +
                "?t=" +
                DateTimeOffset.UtcNow
                .ToUnixTimeSeconds();

            var result = await HttpRequestFactory.GetHttpRequest(url).DownloadStringAsync();

            return JsonConvert.DeserializeObject<RepositoryRoot>(result.Content);
        }
    }
}
