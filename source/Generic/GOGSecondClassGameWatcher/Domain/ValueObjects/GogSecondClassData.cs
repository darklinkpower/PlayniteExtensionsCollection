using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOGSecondClassGameWatcher.Domain.ValueObjects
{
    [ProtoContract]
    public class GogSecondClassData
    {
        [ProtoMember(1)]
        public DateTime? LastCheckTime { get; set; }

        [ProtoMember(2)]
        public List<GogSecondClassGame> Items { get; set; } = new List<GogSecondClassGame>();
    }
}
