using GenericEntityJsonRepository;
using JastUsaLibrary.ProgramsHelper.Models;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.GameInstallationManager.Entities
{
    public class GameInstallCache : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Program Program { get; set; }

        public GameInstallCache()
        {

        }

        public GameInstallCache(Guid id)
        {
            Id = id;
        }
    }
}