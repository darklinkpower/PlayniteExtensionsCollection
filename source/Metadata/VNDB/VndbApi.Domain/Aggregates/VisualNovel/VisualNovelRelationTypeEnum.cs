using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApi.Domain.SharedKernel;

namespace VndbApi.Domain.VisualNovelAggregate
{
    /// <summary>
    /// Enums representing different types of relationships of visual novels.
    /// </summary>
    public enum VnRelationTypeEnum
    {
        /// <summary>
        /// Alternative version of the same visual novel.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RelationType.AlternativeVersion)]
        AlternativeVersion,

        /// <summary>
        /// Visual novel that shares characters with another visual novel.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RelationType.SharesCharacters)]
        SharesCharacters,

        /// <summary>
        /// Fandisc, usually additional content or a side story for an existing visual novel.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RelationType.Fandisc)]
        Fandisc,

        /// <summary>
        /// The original game that a visual novel is based on.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RelationType.OriginalGame)]
        OriginalGame,

        /// <summary>
        /// Parent story from which the current visual novel is derived.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RelationType.ParentStory)]
        ParentStory,

        /// <summary>
        /// Prequel, a story that precedes the current visual novel.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RelationType.Prequel)]
        Prequel,

        /// <summary>
        /// Sequel, a story that follows the current visual novel.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RelationType.Sequel)]
        Sequel,

        /// <summary>
        /// Visual novel that is part of the same series.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RelationType.SameSeries)]
        SameSeries,

        /// <summary>
        /// Visual novel that is set in the same universe or setting.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RelationType.SameSetting)]
        SameSetting,

        /// <summary>
        /// Side story, a narrative that runs parallel or provides additional context to the main story.
        /// </summary>
        [StringRepresentation(VisualNovelConstants.RelationType.SideStory)]
        SideStory
    }
}
