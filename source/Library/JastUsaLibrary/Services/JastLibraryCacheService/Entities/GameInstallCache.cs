using JastUsaLibrary.ProgramsHelper.Models;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastLibraryCacheService.Entities
{
    public class GameInstallCache
    {
        public Guid Id;
        public string GameId;
        public Program Program;
    }
}