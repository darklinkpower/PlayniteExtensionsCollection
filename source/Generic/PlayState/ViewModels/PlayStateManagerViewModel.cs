using Playnite.SDK;
using Playnite.SDK.Models;
using PlayState.Enums;
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
        private readonly MessagesHandler messagesHandler;
        private Guid CurrentDetectionId = Guid.Empty;
        private Game currentGame;
        public Game CurrentGame { get => currentGame; private set => SetValue(ref currentGame, value); }
        private PlayStateData selectedData;
        public PlayStateData SelectedData { get => selectedData; set => SetValue(ref selectedData, value); }
        private ObservableCollection<PlayStateData> playStateDataCollection;
        public ObservableCollection<PlayStateData> PlayStateDataCollection { get => playStateDataCollection; set => SetValue(ref playStateDataCollection, value); }
        private static readonly ILogger logger = LogManager.GetLogger();

        public PlayStateManagerViewModel(IPlayniteAPI playniteApi, MessagesHandler messagesHandler)
        {
            this.playniteApi = playniteApi;
            this.messagesHandler = messagesHandler;
            PlayStateDataCollection = new ObservableCollection<PlayStateData>();
        }

        /// <summary>
        /// Method for obtaining the gameData of the asked game.
        /// </summary>
        internal PlayStateData GetDataOfGame(Game game)
        {
            return playStateDataCollection.FirstOrDefault(x => x.Game.Id == game.Id);
        }

        internal void AddPlayStateData(Game game, SuspendModes suspendMode, List<ProcessItem> gameProcesses, bool setAsCurrentGame = true)
        {
            if (playStateDataCollection.Any(x => x.Game.Id == game.Id))
            {
                logger.Debug($"Data for game {game.Name} with id {game.Id} already exists");
            }
            else
            {
                playStateDataCollection.Add(new PlayStateData(game, gameProcesses, suspendMode));
                var procsExecutablePaths = string.Join(", ", gameProcesses.Select(x => x.ExecutablePath));
                logger.Debug($"Data for game {game.Name} with id {game.Id} was created. Executables: {procsExecutablePaths}");
            }

            if (setAsCurrentGame)
            {
                CurrentGame = game;
                logger.Debug($"Changed current game to {game.Name}");
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
            if (CurrentGame == gameData.Game)
            {
                CurrentGame = playStateDataCollection.Any() ? playStateDataCollection.Last().Game : null;
            }
        }

        public PlayStateData GetCurrentGameData()
        {
            if (CurrentGame == null)
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

        public RelayCommand NavigateBackCommand
        {
            get => new RelayCommand(() =>
            {
                playniteApi.MainView.SwitchToLibraryView();
            });
        }

        public RelayCommand SwitchGameStateCommand
        {
            get => new RelayCommand (() =>
            {
                if (SelectedData != null)
                {
                    SwitchGameState(SelectedData);
                }
            });
        }

        public RelayCommand SetActiveGameCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedData != null)
                {
                    CurrentGame = SelectedData.Game;
                }
            });
        }

        public void SwitchGameState(PlayStateData gameData)
        {
            try
            {
                var processesSuspended = false;
                if (gameData.SuspendMode == SuspendModes.Processes && gameData.GameProcesses.HasItems())
                {
                    foreach (var gameProcess in gameData.GameProcesses)
                    {
                        if (gameProcess == null || gameProcess.Process.Handle == null || gameProcess.Process.Handle == IntPtr.Zero)
                        {
                            return;
                        }
                        if (gameData.IsSuspended)
                        {
                            ProcessesHandler.NtResumeProcess(gameProcess.Process.Handle);
                        }
                        else
                        {
                            ProcessesHandler.NtSuspendProcess(gameProcess.Process.Handle);
                        }
                    }

                    processesSuspended = true;
                }

                if (processesSuspended || gameData.SuspendMode == SuspendModes.Playtime)
                {
                    if (gameData.IsSuspended)
                    {
                        gameData.IsSuspended = false;
                        if (processesSuspended)
                        {
                            messagesHandler.ShowGameStatusNotification(NotificationTypes.Resumed, gameData);
                        }
                        else
                        {
                            messagesHandler.ShowGameStatusNotification(NotificationTypes.PlaytimeResumed, gameData);
                        }

                        gameData.Stopwatch.Stop();
                        logger.Debug($"Game {gameData.Game.Name} resumed in mode {gameData.SuspendMode}");
                    }
                    else
                    {
                        gameData.IsSuspended = true;
                        if (processesSuspended)
                        {
                            messagesHandler.ShowGameStatusNotification(NotificationTypes.Suspended, gameData);
                        }
                        else
                        {
                            messagesHandler.ShowGameStatusNotification(NotificationTypes.PlaytimeSuspended, gameData);
                        }

                        gameData.Stopwatch.Start();
                        logger.Debug($"Game {gameData.Game.Name} suspended in mode {gameData.SuspendMode}");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error while suspending or resuming game {gameData.Game.Name} in mode {gameData.SuspendMode}");
                gameData.GameProcesses = null;
                gameData.Stopwatch.Stop();
            }
        }

        internal void SetDetectionId(Game game)
        {
            CurrentDetectionId = game.Id;
        }

        internal void ResetDetectionId()
        {
            CurrentDetectionId = Guid.Empty;
        }

        internal bool GameMatchedDetectionId(Game game)
        {
            return game.Id == CurrentDetectionId;
        }
    }
}