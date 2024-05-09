using ImporterforAnilist.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowHttp;

namespace ImporterforAnilist.Services
{

    public class MalSyncService
    {
        private const string MalSyncAnilistEndpoint = @"https://api.malsync.moe/mal/{0}/anilist:{1}";
        private const string MalSyncMyanimelistEndpoint = @"https://api.malsync.moe/mal/{0}/{1}";
        private ILogger logger = LogManager.GetLogger();
        private MalSyncRateLimiter malSyncRateLimiter = new MalSyncRateLimiter();

        public MalSyncResponse GetMalSyncData(string mediaType, string anilistMediaId, string myAnimelistId)
        {
            if (mediaType.IsNullOrEmpty())
            {
                return null;
            }

            var queryUri = string.Format(MalSyncAnilistEndpoint, mediaType, anilistMediaId);
            if (!myAnimelistId.IsNullOrEmpty())
            {
                queryUri = string.Format(MalSyncMyanimelistEndpoint, mediaType, myAnimelistId);
            }

            malSyncRateLimiter.WaitForSlot();
            var downloadStringResult = HttpRequestFactory.GetFlowHttpRequest().WithUrl(queryUri).DownloadString();
            if (!downloadStringResult.IsSuccess || downloadStringResult.Content.IsNullOrEmpty())
            {
                return null;
            }

            if (Serialization.TryFromJson<MalSyncResponse>(downloadStringResult.Content, out var malSyncResponse))
            {
                return malSyncResponse;
            }
            else
            {
                logger.Info($"MalSync query {queryUri} doesn't have data");
                return null;
            }
        }
    }
}