using Playnite.SDK.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImporterforAnilist.Models;
using System.Net.Http;
using System.Threading;
using Playnite.SDK.Data;
using WebCommon;
using ImporterforAnilist.Services;

namespace ImporterforAnilist
{
    public class AnilistMetadataProvider : LibraryMetadataProvider
    {
        private ILogger logger = LogManager.GetLogger();
        private ImporterForAnilistSettings settings;
        private AnilistService anilistService;
        private MalSyncService malSyncService;

        public AnilistMetadataProvider(ImporterForAnilistSettings settings, AnilistService anilistService, MalSyncService malSyncService)
        {
            this.settings = settings;
            this.anilistService = anilistService;
            this.malSyncService = malSyncService;
        }

        public override GameMetadata GetMetadata(Game game)
        {
            var metadata = new GameMetadata();
            var mediaData = anilistService.GetMediaDataById(game.GameId);
            if (mediaData is null)
            {
                return metadata;
            }

            metadata = AnilistResponseHelper.MediaToGameMetadata(mediaData.Data.Media, true, settings.PropertiesPrefix);
            var type = mediaData.Data.Media.Type.ToString().ToLower() ?? string.Empty;
            var idMal = mediaData.Data.Media.IdMal.ToString().ToLower() ?? string.Empty;

            var malSyncData = malSyncService.GetMalSyncData(type, game.GameId, idMal);
            if (malSyncData != null)
            {
                AddMalSyncLinksToMetadata(metadata, malSyncData);
            }

            return metadata;
        }

        private void AddMalSyncLinksToMetadata(GameMetadata metadata, MalSyncResponse malSyncResponse)
        {
            if (malSyncResponse.Sites is null)
            {
                return;
            }

            foreach (var site in malSyncResponse.Sites)
            {
                var siteName = site.Key;
                foreach (var siteItem in site.Value)
                {
                    var malSyncItem = siteItem.Value;
                    metadata.Links.Add(new Link(string.Format("{0} - {1}", siteName, malSyncItem.Title), malSyncItem.Url));
                }
            }
        }
    }
}