using GenericEntityJsonRepository;
using JastUsaLibrary.ProgramsHelper.Models;
using JastUsaLibrary.Services.JastUsaIntegration.Domain.Entities;
using Playnite.SDK.Data;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.JastLibraryCacheService.Entities
{
    public class GameCache : IEntity<int>
    {
        public JastGameDownloads Downloads { get; set; } = null;
        public JastGameData JastGameData { get; set; } = null;
        public int Id { get; set; }

        public GameCache()
        {

        }

        public GameCache(int gameId)
        {
            Id = Guard.Against.Null(gameId);
        }

        internal void UpdateDownloads(JastGameDownloads downloads)
        {
            Downloads = downloads;
        }

        internal void UpdateJastGameData(JastGameData jastGameData)
        {
            JastGameData = jastGameData;
        }

        public GameCache GetClone()
        {
            var clone = new GameCache(Id);
            if (Downloads != null)
            {
                clone.Downloads = Serialization.GetClone(Downloads);
            }

            if (JastGameData != null)
            {
                clone.JastGameData = Serialization.GetClone(JastGameData);
            }

            return clone;
        }
    }
}
