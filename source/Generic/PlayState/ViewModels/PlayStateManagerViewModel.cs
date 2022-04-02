using Playnite.SDK;
using Playnite.SDK.Models;
using PlayState.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayState.ViewModels
{
    public class PlayStateManagerViewModel : ObservableObject
    {
        private readonly IPlayniteAPI playniteApi;
        private Game currentGame;
        public Game CurrentGame { get => currentGame; set => SetValue(ref currentGame, value); }
        private ObservableCollection<PlayStateData> playStateDataCollection;
        public ObservableCollection<PlayStateData> PlayStateDataCollection { get => playStateDataCollection; set => SetValue(ref playStateDataCollection, value); }
        private static readonly ILogger logger = LogManager.GetLogger();

        public PlayStateManagerViewModel(IPlayniteAPI playniteApi)
        {
            this.playniteApi = playniteApi;
            PlayStateDataCollection = new ObservableCollection<PlayStateData>();
        }

        /// <summary>
        /// Method for obtaining the gameData of the asked game.
        /// </summary>
        internal PlayStateData GetDataOfGame(Game game)
        {
            return playStateDataCollection.FirstOrDefault(x => x.Game.Id == game.Id);
        }

        internal void AddPlayStateData(Game game, List<ProcessItem> gameProcesses, bool suspendPlaytimeOnly = false)
        {
            if (playStateDataCollection.Any(x => x.Game.Id == game.Id))
            {
                logger.Debug($"Data for game {game.Name} with id {game.Id} already exists");
            }
            else
            {
                playStateDataCollection.Add(new PlayStateData(game, gameProcesses, suspendPlaytimeOnly));
                var procsExecutablePaths = string.Join(", ", gameProcesses.Select(x => x.ExecutablePath));
                logger.Debug($"Data for game {game.Name} with id {game.Id} was created. Executables: {procsExecutablePaths}");
            }
        }

        public void RemovePlayStateData(Game game)
        {
            var data = GetDataOfGame(game);
            if (data != null)
            {
                RemovePlayStateData(data);
            }
        }

        public void RemovePlayStateData(PlayStateData gameData)
        {
            playStateDataCollection.Remove(gameData);
            logger.Debug($"Data for game {gameData.Game.Name} with id {gameData.Game.Id} was removed");
            if (currentGame == gameData.Game)
            {
                currentGame = playStateDataCollection.Any() ? playStateDataCollection.Last().Game : null;
            }
        }

        public PlayStateData GetCurrentGameData()
        {
            if (currentGame == null)
            {
                return null;
            }

            return GetDataOfGame(CurrentGame);
        }

        public bool GetIsCurrentGameDifferent(Game game)
        {
            if (CurrentGame == null || CurrentGame.Id != game.Id)
            {
                return true;
            }

            return false;
        }
    }
}