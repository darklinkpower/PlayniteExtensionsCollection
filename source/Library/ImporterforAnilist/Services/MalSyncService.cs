﻿using ImporterforAnilist.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCommon;

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
            var downloadStringResult = HttpDownloader.DownloadString(queryUri);
            if (!downloadStringResult.Success || downloadStringResult.Result.IsNullOrEmpty())
            {
                return null;
            }

            if (Serialization.TryFromJson<MalSyncResponse>(downloadStringResult.Result, out var malSyncResponse))
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