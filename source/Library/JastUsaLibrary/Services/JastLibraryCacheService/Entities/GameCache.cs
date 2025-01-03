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
    public class GameCache
    {
        public JastGameDownloads Downloads { get; set; } = null;
        public JastGameData JastGameData { get; set; } = null;
        public Program Program { get; set; } = null;
        public string GameId { get; set; }

        public GameCache()
        {

        }

        public GameCache(string gameId)
        {
            GameId = Guard.Against.NullOrEmpty(gameId);
        }

        internal void UpdateDownloads(JastGameDownloads downloads)
        {
            Downloads = downloads;
        }

        internal void UpdateJastGameData(JastGameData jastGameData)
        {
            JastGameData = jastGameData;
        }

        internal void UpdateProgram(Program program)
        {
            Program = program;
        }

        public GameCache GetClone()
        {
            var clone = new GameCache(GameId);
            if (Downloads != null)
            {
                clone.Downloads = Serialization.GetClone(Downloads);
            }

            if (JastGameData != null)
            {
                clone.JastGameData = Serialization.GetClone(JastGameData);
            }

            if (Program != null)
            {
                clone.Program = Serialization.GetClone(Program);
            }

            return clone;
        }
    }
}
