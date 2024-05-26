using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbApiDomain.CharacterAggregate;
using VndbApiDomain.TraitAggregate;
using VndbApiDomain.VisualNovelAggregate;

namespace VNDBNexus.Shared.DatabaseCommon
{
    public class CharacterWrapper
    {
        public Character Character { get; }
        public VisualNovelVoiceActor VoiceActor { get; }

        public CharacterWrapper(Character character, VisualNovelVoiceActor matchingVoiceActor)
        {
            Character = character;
            VoiceActor = matchingVoiceActor;
        }
    }
}