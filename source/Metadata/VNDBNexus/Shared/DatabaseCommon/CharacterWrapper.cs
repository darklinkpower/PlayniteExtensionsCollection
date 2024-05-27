using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.CharacterAggregate;
using VndbApiDomain.SharedKernel;
using VndbApiDomain.TraitAggregate;
using VndbApiDomain.VisualNovelAggregate;

namespace VNDBNexus.Shared.DatabaseCommon
{
    public class CharacterWrapper
    {
        public Character Character { get; }
        public VisualNovelVoiceActor VoiceActor { get; }
        public SpoilerLevelEnum SpoilerLevel { get; }

        public CharacterRoleEnum Role { get; }

        public CharacterWrapper(Character character, VisualNovelVoiceActor matchingVoiceActor, CharacterVisualNovel visualNovelAppearance)
        {
            Character = character;
            VoiceActor = matchingVoiceActor != null ? matchingVoiceActor : null;
            SpoilerLevel = visualNovelAppearance != null ? visualNovelAppearance.Spoiler : SpoilerLevelEnum.None;
            Role = visualNovelAppearance != null ? visualNovelAppearance.Role : CharacterRoleEnum.Main;
        }
    }
}