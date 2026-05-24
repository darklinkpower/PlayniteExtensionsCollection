using Playnite.SDK;
using ReviewViewer.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewViewer.Presentation
{
    internal static class ReviewReactionToMetadataMapper
    {
        private const string BasePath = "pack://application:,,,/ReviewViewer;component/Presentation/Resources/Reactions/";

        private static readonly ConcurrentDictionary<string, bool> ResourceCache = new ConcurrentDictionary<string, bool>();
        private static readonly ConcurrentDictionary<uint, ReactionMetadata> MetadataCache = new ConcurrentDictionary<uint, ReactionMetadata>();

        public static ReactionMetadata Map(Reaction reaction)
        {
            var id = reaction.ReactionType;

            // Cache hit so we don't have to look up resources again
            if (MetadataCache.TryGetValue(id, out var cached))
            {
                return cached;
            }

            var staticIcon = $"{BasePath}{id}.png";
            var animatedIcon = $"{BasePath}{id}-animated.png";

            if (!ResourceExists(staticIcon))
            {
                MetadataCache[id] = null; // cache the miss
                return null;
            }

            var metadata = new ReactionMetadata(
                staticIcon,
                ResourceExists(animatedIcon) ? animatedIcon : null,
                ResourceProvider.GetString($"LOCReview_Viewer_Reaction_{id}_Name"),
                ResourceProvider.GetString($"LOCReview_Viewer_Reaction_{id}_Desc")
            );

            MetadataCache[id] = metadata;
            return metadata;
        }

        private static bool ResourceExists(string uri)
        {
            if (ResourceCache.TryGetValue(uri, out var exists))
            {
                return exists;
            }

            try
            {
                var stream = System.Windows.Application.GetResourceStream(new Uri(uri));
                exists = stream != null;
            }
            catch
            {
                exists = false;
            }

            ResourceCache[uri] = exists;
            return exists;
        }
    }
}
