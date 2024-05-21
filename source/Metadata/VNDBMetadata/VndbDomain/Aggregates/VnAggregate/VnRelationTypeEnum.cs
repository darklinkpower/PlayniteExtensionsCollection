using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.VndbDomain.Common.Attributes;

namespace VNDBMetadata.VndbDomain.Aggregates.VnAggregate
{
    /// <summary>
    /// Enums representing different types of relationships of visual novels.
    /// </summary>
    public enum VnRelationTypeEnum
    {
        /// <summary>
        /// Alternative version of the same visual novel.
        /// </summary>
        [StringRepresentation(VnConstants.VnRelationType.AlternativeVersion)]
        AlternativeVersion,

        /// <summary>
        /// Visual novel that shares characters with another visual novel.
        /// </summary>
        [StringRepresentation(VnConstants.VnRelationType.SharesCharacters)]
        SharesCharacters,

        /// <summary>
        /// Fandisc, usually additional content or a side story for an existing visual novel.
        /// </summary>
        [StringRepresentation(VnConstants.VnRelationType.Fandisc)]
        Fandisc,

        /// <summary>
        /// The original game that a visual novel is based on.
        /// </summary>
        [StringRepresentation(VnConstants.VnRelationType.OriginalGame)]
        OriginalGame,

        /// <summary>
        /// Parent story from which the current visual novel is derived.
        /// </summary>
        [StringRepresentation(VnConstants.VnRelationType.ParentStory)]
        ParentStory,

        /// <summary>
        /// Prequel, a story that precedes the current visual novel.
        /// </summary>
        [StringRepresentation(VnConstants.VnRelationType.Prequel)]
        Prequel,

        /// <summary>
        /// Sequel, a story that follows the current visual novel.
        /// </summary>
        [StringRepresentation(VnConstants.VnRelationType.Sequel)]
        Sequel,

        /// <summary>
        /// Visual novel that is part of the same series.
        /// </summary>
        [StringRepresentation(VnConstants.VnRelationType.SameSeries)]
        SameSeries,

        /// <summary>
        /// Visual novel that is set in the same universe or setting.
        /// </summary>
        [StringRepresentation(VnConstants.VnRelationType.SameSetting)]
        SameSetting,

        /// <summary>
        /// Side story, a narrative that runs parallel or provides additional context to the main story.
        /// </summary>
        [StringRepresentation(VnConstants.VnRelationType.SideStory)]
        SideStory
    }
}
