using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using PlayState.Enums;
using PlayState.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PlayState.Controls
{
    /// <summary>
    /// Interaction logic for GameStateSwitchControl.xaml
    /// </summary>
    public partial class GameStateSwitchControl : PluginUserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private Guid currentGameId = Guid.Empty;
        private Visibility controlVisibility = Visibility.Collapsed;
        private Visibility pauseIconVisibility = Visibility.Collapsed;
        private Visibility resumeIconlVisibility = Visibility.Collapsed;
        private PlayStateManagerViewModel playStateManagerViewModel;
        private PlayStateDataStatus gameStatus = PlayStateDataStatus.NotFound;
        private IPlayniteAPI playniteApi;
        private PlayStateSettingsViewModel settings;
        private readonly DispatcherTimer updateDataTimer;

        public PlayStateDataStatus GameStatus
        {
            get => gameStatus;
            set
            {
                gameStatus = value;
                OnPropertyChanged();
            }
        }

        public Visibility ControlVisibility
        {
            get => controlVisibility;
            set
            {
                controlVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility PauseIconVisibility
        {
            get => pauseIconVisibility;
            set
            {
                pauseIconVisibility = value;
                OnPropertyChanged();
            }
        }
        public Visibility ResumeIconVisibility
        {
            get => resumeIconlVisibility;
            set
            {
                resumeIconlVisibility = value;
                OnPropertyChanged();
            }
        }
        public DesktopView ActiveViewAtCreation { get; }

        public GameStateSwitchControl(PlayStateManagerViewModel playStateManagerViewModel, IPlayniteAPI playniteApi, PlayStateSettingsViewModel settings)
        {
            InitializeComponent();
            DataContext = this;
            this.playStateManagerViewModel = playStateManagerViewModel;
            this.playniteApi = playniteApi;
            this.settings = settings;
            updateDataTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(150)
            };
            updateDataTimer.Tick += new EventHandler(UpdateDataEvent);

            if (playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                ActiveViewAtCreation = playniteApi.MainView.ActiveDesktopView;
            }

            playStateManagerViewModel.PlayStateDataCollection.CollectionChanged += PlayStateDataCollection_CollectionChanged;
            playStateManagerViewModel.GameStatusSwitched += PlayStateManagerViewModel_GameStatusSwitched;
        }

        private void PlayStateManagerViewModel_GameStatusSwitched(object sender, EventArgs e)
        {
            UpdateData();
        }

        private void PlayStateDataCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateData();
        }

        private void UpdateDataEvent(object sender, EventArgs e)
        {
            UpdateData();
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            updateDataTimer.Stop();
            if (!GetShouldContinueExecution())
            {
                return;
            }

            ControlVisibility = Visibility.Collapsed;
            settings.Settings.IsControlVisible = false;
            if (newContext == null || !settings.Settings.EnableGameStateSwitchControl)
            {
                currentGameId = Guid.Empty;
                return;
            }

            currentGameId = newContext.Id;
            RestartTimer();
        }

        private bool GetShouldContinueExecution()
        {
            //The GameContextChanged method is rised even when the control
            //is not in the active view. To prevent unecessary processing we
            //can stop processing if the active view is not the same one was
            //the one during creation
            if (playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop &&
                ActiveViewAtCreation != playniteApi.MainView.ActiveDesktopView)
            {
                return false;
            }

            return true;
        }

        public void RestartTimer()
        {
            updateDataTimer.Stop();
            updateDataTimer.Start();
        }

        public void UpdateData()
        {
            updateDataTimer.Stop();
            ControlVisibility = Visibility.Collapsed;
            settings.Settings.IsControlVisible = false;
            if (!settings.Settings.EnableGameStateSwitchControl ||
                !GetShouldContinueExecution() ||
                currentGameId == Guid.Empty)
            {
                return;
            }

            GameStatus = playStateManagerViewModel.GetStatusOfGameFromId(currentGameId);
            switch (GameStatus)
            {
                case PlayStateDataStatus.Running:
                    PauseIconVisibility = Visibility.Visible;
                    ResumeIconVisibility = Visibility.Collapsed;
                    ControlVisibility = Visibility.Visible;
                    settings.Settings.IsControlVisible = true;
                    break;
                case PlayStateDataStatus.Paused:
                    PauseIconVisibility = Visibility.Collapsed;
                    ResumeIconVisibility = Visibility.Visible;
                    ControlVisibility = Visibility.Visible;
                    settings.Settings.IsControlVisible = true;
                    break;
                default:
                    break;
            }
        }

        public RelayCommand SwitchCurrentGameStatusCommand
        {
            get => new RelayCommand(() =>
            {
                if (currentGameId != Guid.Empty)
                {
                    playStateManagerViewModel.SwitchGameStateFromId(currentGameId);
                }
            });
        }

    }
}