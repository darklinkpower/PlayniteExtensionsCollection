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
        private string _tooltip = null;
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

        public string Tooltip
        {
            get => _tooltip;
            set
            {
                _tooltip = value;
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
            ResetToDefaultValues();
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
                Tooltip = GetTooltip(_secondClassData);
                _isAtDefaultValues = false;
            }
        }

        private void ResetToDefaultValues()
        {
            _secondClassData = null;
            NumberOfIssues = string.Empty;
            Tooltip = null;
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

        private static string GetTooltip(GogSecondClassGame gogSecondClassGame)
        {
            if (gogSecondClassGame is null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            // General Issues
            if (gogSecondClassGame.GeneralIssues.GetIssuesCount() > 0)
            {
                sb.Append(ResourceProvider.GetString("LOC_GogSecondClass_GeneralIssuesLabel"));
                sb.Append($"\n{new string('-', 40)}\n");
                var lines = new List<string>();
                AddLinesToTooltip(lines, gogSecondClassGame.GeneralIssues.MissingUpdates, "LOC_GogSecondClass_MissingUpdatesLabel");
                AddLinesToTooltip(lines, gogSecondClassGame.GeneralIssues.MissingLanguages, "LOC_GogSecondClass_MissingLanguagesLabel");
                AddLinesToTooltip(lines, gogSecondClassGame.GeneralIssues.MissingFreeDlc, "LOC_GogSecondClass_MissingFreeDlcLabel");
                AddLinesToTooltip(lines, gogSecondClassGame.GeneralIssues.MissingPaidDlc, "LOC_GogSecondClass_MissingPaidDlcLabel");
                AddLinesToTooltip(lines, gogSecondClassGame.GeneralIssues.MissingFeatures, "LOC_GogSecondClass_MissingFeaturesLabel");
                AddLinesToTooltip(lines, gogSecondClassGame.GeneralIssues.MissingSoundtrack, "LOC_GogSecondClass_MissingSoundtrackLabel");
                AddLinesToTooltip(lines, gogSecondClassGame.GeneralIssues.OtherIssues, "LOC_GogSecondClass_OtherIssuesLabel");
                AddLinesToTooltip(lines, gogSecondClassGame.GeneralIssues.MissingBuilds, "LOC_GogSecondClass_MissingBuildsLabel");
                AddLinesToTooltip(lines, gogSecondClassGame.GeneralIssues.RegionLocking, "LOC_GogSecondClass_RegionLockingLabel");
                sb.Append(string.Join("\n", lines));
            }

            // Achievements Issues
            if (gogSecondClassGame.AchievementsIssues.GetIssuesCount() > 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append($"\n\n");
                }

                sb.Append(ResourceProvider.GetString("LOC_GogSecondClass_AchievementsIssuesLabel"));
                sb.Append($"\n{new string('-', 40)}\n");
                var lines = new List<string>();
                AddLinesToTooltip(lines, gogSecondClassGame.AchievementsIssues.MissingAllAchievements, "LOC_GogSecondClass_MissingAllAchievementsLabel");
                AddLinesToTooltip(lines, gogSecondClassGame.AchievementsIssues.MissingSomeAchievements, "LOC_GogSecondClass_MissingSomeAchievementsLabel");
                AddLinesToTooltip(lines, gogSecondClassGame.AchievementsIssues.BrokenAchievements, "LOC_GogSecondClass_BrokenAchievementsLabel");
                AddLinesToTooltip(lines, gogSecondClassGame.AchievementsIssues.AchievementsAskedResponse, "LOC_GogSecondClass_AchievementsAskedResponseLabel");
                sb.Append(string.Join("\n", lines));
            }

            return sb.ToString();
        }

        private static void AddLinesToTooltip(List<string> lines, IReadOnlyList<string> issuesStrings, string locStringKey)
        {
            if (!issuesStrings?.Any() == true)
            {
                return;
            }

            lines.Add($"{ResourceProvider.GetString(locStringKey)}:");
            var lineIndentString = new string(' ', 8);
            foreach (var issueString in issuesStrings)
            {
                var splittedIssueString = issueString.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var str in splittedIssueString)
                {
                    lines.Add($"{lineIndentString}{str}");
                }
            }
        }

        private static void AddLinesToTooltip(List<string> lines, string issuesString, string locStringKey)
        {
            if (issuesString.IsNullOrEmpty())
            {
                return;
            }

            lines.Add($"{ResourceProvider.GetString(locStringKey)}:");
            var lineIndentString = new string(' ', 8);
            var splittedIssueString = issuesString.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var str in splittedIssueString)
            {
                lines.Add($"{lineIndentString}{str}");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
