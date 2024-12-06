using GOGSecondClassGameWatcher.Application;
using GOGSecondClassGameWatcher.Domain.ValueObjects;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
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

namespace GOGSecondClassGameWatcher.Presentation
{
    /// <summary>
    /// Interaction logic for PlayniteThemesUserControl.xaml
    /// </summary>
    public partial class PlayniteThemesUserControl : PluginUserControl, INotifyPropertyChanged
    {
        private readonly IPlayniteAPI _playniteApi;
        private GogSecondClassService _gogSecondClassService;
        private readonly GOGSecondClassGameWatcherSettingsViewModel _gOGSecondClassWatcherSettingsViewModel;
        private readonly GogSecondClassGameWindowCreator _gogSecondClassGameWindowCreator;
        private readonly Guid _gogPluginId = Guid.Parse("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E");
        private readonly DesktopView _activeViewAtCreation;

        public event PropertyChangedEventHandler PropertyChanged;
        private bool _isAtDefaultValues = true;
        private string _numberOfIssues = string.Empty;
        private GogSecondClassGame _secondClassData;

        public string NumberOfIssues
        {
            get => _numberOfIssues;
            set
            {
                _numberOfIssues = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand OpenWindowCommand { get; }

        public PlayniteThemesUserControl(IPlayniteAPI playniteApi, GogSecondClassService gogSecondClassService, GOGSecondClassGameWatcherSettingsViewModel gOGSecondClassWatcherSettingsViewModel, GogSecondClassGameWindowCreator gogSecondClassGameWindowCreator)
        {
            InitializeComponent();
            _playniteApi = playniteApi;
            _gogSecondClassService = gogSecondClassService;
            _gOGSecondClassWatcherSettingsViewModel = gOGSecondClassWatcherSettingsViewModel;
            _gogSecondClassGameWindowCreator = gogSecondClassGameWindowCreator;
            SetControlTextBlockStyle(playniteApi, Resources);
            OpenWindowCommand = new RelayCommand(OpenWindow, () => _secondClassData != null);
            if (playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                _activeViewAtCreation = playniteApi.MainView.ActiveDesktopView;
            }

            DataContext = this;
        }

        private static void SetControlTextBlockStyle(IPlayniteAPI playniteApi, ResourceDictionary resources)
        {
            // Desktop mode uses BaseTextBlockStyle and Fullscreen Mode uses TextBlockBaseStyle
            var baseStyleName = playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop ? "BaseTextBlockStyle" : "TextBlockBaseStyle";
            if (ResourceProvider.GetResource(baseStyleName) is Style baseStyle &&
                baseStyle.TargetType == typeof(TextBlock))
            {
                var implicitStyle = new Style(typeof(TextBlock), baseStyle);
                resources.Add(typeof(TextBlock), implicitStyle);
            }
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            //The GameContextChanged method is rised even when the control
            //is not in the active view. To prevent unecessary processing we
            //can stop processing if the active view is not the same one was
            //the one during creation
            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop &&
                _activeViewAtCreation != _playniteApi.MainView.ActiveDesktopView)
            {
                return;
            }

            if (!_isAtDefaultValues)
            {
                ResetToDefaultValues();
            }

            if (newContext == null)
            {
                return;
            }

            var game = newContext;
            var data = _gogSecondClassService.GetDataForGame(game);
            if (data != null && GogLibraryUtilities.ShouldWatcherNotify(_gOGSecondClassWatcherSettingsViewModel.Settings, data))
            {
                _secondClassData = data;
                NumberOfIssues = _secondClassData.TotalIssues.ToString();
                Visibility = Visibility.Visible;
                _gOGSecondClassWatcherSettingsViewModel.Settings.IsControlVisible = true;
                _isAtDefaultValues = false;
            }
        }

        private void ResetToDefaultValues()
        {
            _secondClassData = null;
            NumberOfIssues = string.Empty;
            Visibility = Visibility.Collapsed;
            _gOGSecondClassWatcherSettingsViewModel.Settings.IsControlVisible = false;
            _isAtDefaultValues = true;
        }

        private void OpenWindow()
        {
            if (_secondClassData is null)
            {
                return;
            }

            _gogSecondClassGameWindowCreator.OpenWindow(_secondClassData, this.GameContext);
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
