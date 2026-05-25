using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.Core.Application
{
    public class SpecialKLaunchSession
    {
        public Guid GameId { get; }

        public bool Started32BitService { get; }

        public bool Started64BitService { get; }

        public SpecialKLaunchSession(
            Guid gameId,
            bool started32BitService,
            bool started64BitService)
        {
            GameId = gameId;
            Started32BitService = started32BitService;
            Started64BitService = started64BitService;
        }
    }
}