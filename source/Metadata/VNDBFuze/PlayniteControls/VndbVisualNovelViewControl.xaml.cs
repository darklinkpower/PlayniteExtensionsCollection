using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VNDBFuze.Enums;
using VNDBFuze.VndbDomain.Aggregates.CharacterAggregate;
using VNDBFuze.VndbDomain.Aggregates.TagAggregate;
using VNDBFuze.VndbDomain.Aggregates.VnAggregate;
using VNDBFuze.VndbDomain.Common.Enums;
using VNDBFuze.VndbDomain.Common.Models;
using VNDBFuze.VndbDomain.Services;

namespace VNDBFuze.PlayniteControls
{
    /// <summary>
    /// Interaction logic for VndbVisualNovelViewControl.xaml
    /// </summary>
    public partial class VndbVisualNovelViewControl : PluginUserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly VndbService _vndbService;
        private readonly BbCodeProcessor _bbcodeProcessor;
        private readonly IPlayniteAPI _playniteApi;
        private readonly string _pluginStoragePath;
        private readonly VNDBFuzeSettingsViewModel _settingsViewModel;
        private readonly DesktopView _activeViewAtCreation;
        private readonly DispatcherTimer _updateControlDataDelayTimer;
        private Game _currentGame;
        private Guid _currentGameId = Guid.Empty;

        private List<Character> _activeVisualNovelCharacters;
        public List<Character> ActiveVisualNovelCharacters
        {
            get => _activeVisualNovelCharacters;
            set
            {
                _activeVisualNovelCharacters = value;
                OnPropertyChanged();
            }
        }

        private SpoilerLevelEnum _charactersMaxSpoilerLevel = SpoilerLevelEnum.None;
        public SpoilerLevelEnum CharactersMaxSpoilerLevel
        {
            get => _charactersMaxSpoilerLevel;
            set
            {
                _charactersMaxSpoilerLevel = value;
                OnPropertyChanged();
            }
        }

        private bool _displayCharacterSexualTraits = false;
        public bool DisplayCharacterSexualTraits
        {
            get => _displayCharacterSexualTraits;
            set
            {
                _displayCharacterSexualTraits = value;
                OnPropertyChanged();
            }
        }

        private Vn _activeVisualNovel;
        public Vn ActiveVisualNovel
        {
            get => _activeVisualNovel;
            set
            {
                _activeVisualNovel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TagsToDisplay));

                OnPropertyChanged(nameof(TagsContentCategoryButtonVisibility));
                OnPropertyChanged(nameof(TagsTechnicalCategoryButtonVisibility));
                OnPropertyChanged(nameof(TagsSexualCategoryButtonVisibility));

                OnPropertyChanged(nameof(TagsNoneSpoilerButtonVisibility));
                OnPropertyChanged(nameof(TagsMinimumSpoilerButtonVisibility));
                OnPropertyChanged(nameof(TagsMajorSpoilerButtonVisibility));
            }
        }

        public IEnumerable<VnVndbTag> TagsToDisplay => GetTagsToDisplay();

        public Visibility TagsContentCategoryButtonVisibility =>
            _activeVisualNovel?.Tags.Any(x => x.Category == TagCategoryEnum.Content) == true
            ? Visibility.Visible : Visibility.Collapsed;

        public Visibility TagsTechnicalCategoryButtonVisibility =>
            _activeVisualNovel?.Tags.Any(x => x.Category == TagCategoryEnum.Technical) == true
            ? Visibility.Visible : Visibility.Collapsed;

        public Visibility TagsSexualCategoryButtonVisibility =>
            _activeVisualNovel?.Tags.Any(x => x.Category == TagCategoryEnum.SexualContent) == true
            ? Visibility.Visible : Visibility.Collapsed;

        public Visibility TagsNoneSpoilerButtonVisibility =>
            _activeVisualNovel?.Tags.Any(x => x.Spoiler == SpoilerLevelEnum.None) == true
            ? Visibility.Visible : Visibility.Collapsed;

        public Visibility TagsMinimumSpoilerButtonVisibility =>
            _activeVisualNovel?.Tags.Any(x => x.Spoiler == SpoilerLevelEnum.Minimum) == true
            ? Visibility.Visible : Visibility.Collapsed;

        public Visibility TagsMajorSpoilerButtonVisibility =>
            _activeVisualNovel?.Tags.Any(x => x.Spoiler == SpoilerLevelEnum.Major) == true
            ? Visibility.Visible : Visibility.Collapsed;

        private double _tagsMinimumScore = 2.0;
        public double TagsMinimumScore
        {
            get { return _tagsMinimumScore; }
            set
            {
                var currentRating = _tagsMinimumScore;
                _tagsMinimumScore = Math.Round(value, 1);
                OnPropertyChanged();
                if (currentRating != _tagsMinimumScore)
                {
                    OnPropertyChanged(nameof(TagsToDisplay));
                }
                
            }
        }

        private TagsDisplayOptionEnum _tagsDisplayOption = TagsDisplayOptionEnum.Summary;
        public TagsDisplayOptionEnum TagsDisplayOption
        {
            get { return _tagsDisplayOption; }
            set
            {
                _tagsDisplayOption = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TagsToDisplay));
            }
        }

        private bool _tagsDisplayNoneSpoilers = true;
        public bool TagsDisplayNoneSpoilers
        {
            get => _tagsDisplayNoneSpoilers;
            set
            {
                _tagsDisplayNoneSpoilers = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TagsToDisplay));
            }
        }

        private bool _tagsDisplayMinimumSpoilers = false;
        public bool TagsDisplayMinimumSpoilers
        {
            get => _tagsDisplayMinimumSpoilers;
            set
            {
                _tagsDisplayMinimumSpoilers = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TagsToDisplay));
            }
        }

        private bool _tagsDisplayMajorSpoilers = false;
        public bool TagsDisplayMajorSpoilers
        {
            get => _tagsDisplayMajorSpoilers;
            set
            {
                _tagsDisplayMajorSpoilers = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TagsToDisplay));
            }
        }

        private bool _tagsDisplayContentCategory = true;
        public bool TagsDisplayContentCategory
        {
            get => _tagsDisplayContentCategory;
            set
            {
                _tagsDisplayContentCategory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TagsToDisplay));
            }
        }

        private bool _tagsDisplayTechnicalCategory = false;
        public bool TagsDisplayTechnicalCategory
        {
            get => _tagsDisplayTechnicalCategory;
            set
            {
                _tagsDisplayTechnicalCategory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TagsToDisplay));
            }
        }

        private bool _tagsDisplaySexualCategory = false;
        public bool TagsDisplaySexualCategory
        {
            get => _tagsDisplaySexualCategory;
            set
            {
                _tagsDisplaySexualCategory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TagsToDisplay));
            }
        }

        private bool _displayUnnoficialRelations = false;
        public bool DisplayUnnoficialRelations
        {
            get => _displayUnnoficialRelations;
            set
            {
                _displayUnnoficialRelations = value;
                OnPropertyChanged();
            }
        }

        public VndbVisualNovelViewControl()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                LoadDesignData();
                DataContext = this;
            }
        }

        private void LoadDesignData()
        {
            var filePath = @"C:\VndbApiTests\VndbVnResponse.json";
            if (FileSystem.FileExists(filePath))
            {
                var text = FileSystem.ReadStringFromFile(filePath);
                var queryResponse = JsonConvert.DeserializeObject<VndbDatabaseQueryReponse<Vn>>(text);
                if (queryResponse.Results.Count > 0)
                {
                    ActiveVisualNovel = queryResponse.Results.FirstOrDefault();
                }
            }

            var characterFilePath = @"C:\VndbApiTests\VndbCharacterResponse.json";
            if (FileSystem.FileExists(characterFilePath))
            {
                var characterText = FileSystem.ReadStringFromFile(characterFilePath);
                var queryResponse = JsonConvert.DeserializeObject<VndbDatabaseQueryReponse<Character>>(characterText);
                if (queryResponse.Results.Count > 0)
                {
                    ActiveVisualNovelCharacters = queryResponse.Results;
                }
            }
        }

        public VndbVisualNovelViewControl(VndbService vndbService, VNDBFuze plugin, VNDBFuzeSettingsViewModel settingsViewModel, BbCodeProcessor bbcodeProcessor)
        {
            InitializeComponent();
            _vndbService = vndbService;
            _bbcodeProcessor = bbcodeProcessor;
            _playniteApi = plugin.PlayniteApi;
            _pluginStoragePath = plugin.GetPluginUserDataPath();
            _settingsViewModel = settingsViewModel;
            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                _activeViewAtCreation = _playniteApi.MainView.ActiveDesktopView;
            }

            _updateControlDataDelayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(700)
            };

            _updateControlDataDelayTimer.Tick += new EventHandler(UpdateControlData);

            LoadDesignData();
            SetVisibleVisibility();
            DataContext = this;
        }

        private async void UpdateControlData(object sender, EventArgs e)
        {
            _updateControlDataDelayTimer.Stop();
            await UpdateControlAsync();
        }

        private void SetVisibleVisibility()
        {
            Visibility = Visibility.Visible;
            _settingsViewModel.Settings.IsControlVisible = true;
        }

        private void SetCollapsedVisibility()
        {
            Visibility = Visibility.Collapsed;
            _settingsViewModel.Settings.IsControlVisible = false;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            return;
            //The GameContextChanged method is rised even when the control
            //is not in the active view. To prevent unecessary processing we
            //can stop processing if the active view is not the same one was
            //the one during creation
            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop && _activeViewAtCreation != _playniteApi.MainView.ActiveDesktopView)
            {
                _updateControlDataDelayTimer.Stop();
                return;
            }

            _currentGame = newContext;
            SetCollapsedVisibility();
            _updateControlDataDelayTimer.Stop();

            if (ActiveVisualNovel != null)
            {
                ActiveVisualNovel = null;
            }

            if (_currentGame is null || !_settingsViewModel.Settings.EnableVnViewControl)
            {
                _currentGameId = Guid.Empty;
            }
            else
            {
                _currentGameId = _currentGame.Id;
                _updateControlDataDelayTimer.Start();
            }
        }

        private async Task UpdateControlAsync()
        {
            if (_currentGame is null)
            {
                return;
            }

            var contextGame = _currentGame;
            var contextGameId = contextGame.Id;
            _currentGameId = contextGameId;
            var vndbId = VndbUtilities.GetVndbIdFromLinks(contextGame);
            if (vndbId.IsNullOrEmpty())
            {
                return;
            }

            var searchStoragePath = Path.Combine(_pluginStoragePath, "SearchVnId", $"{vndbId}.json");
            if (!FileSystem.FileExists(searchStoragePath))
            {
                var vndbRequestFilter = VnFilterFactory.Id.EqualTo(vndbId);
                var query = new VnRequestQuery(vndbRequestFilter);
                query.Fields.EnableAllFlags(true);
                var searchResult = await _vndbService.GetResponseFromPostRequest(query);
                if (searchResult.IsNullOrEmpty())
                {
                    return;
                }

                FileSystem.WriteStringToFile(searchStoragePath, searchResult, true);
                if (_currentGameId == null || _currentGameId != contextGameId)
                {
                    return;
                }
            }

            if (Serialization.TryFromJsonFile<VndbDatabaseQueryReponse<Vn>>(searchStoragePath, out var queryResponse))
            {
                SetVisibleVisibility();
                if (queryResponse.Results.Count == 0)
                {
                    return;
                }

                ActiveVisualNovel = queryResponse.Results.FirstOrDefault();
            }
        }

        private IEnumerable<VnVndbTag> GetTagsToDisplay()
        {
            var counters = new Dictionary<TagCategoryEnum, int>
            {
                [TagCategoryEnum.Content] = 0,
                [TagCategoryEnum.Technical] = 0,
                [TagCategoryEnum.SexualContent] = 0
            };

            foreach (var tag in ActiveVisualNovel?.Tags?.OrderByDescending(x => x.Rating))
            {
                if (tag.Category == TagCategoryEnum.Content && !_tagsDisplayContentCategory)
                {
                    continue;
                }

                if (tag.Category == TagCategoryEnum.Technical && !_tagsDisplayTechnicalCategory)
                {
                    continue;
                }

                if (tag.Category == TagCategoryEnum.SexualContent && !_tagsDisplaySexualCategory)
                {
                    continue;
                }

                if (tag.Spoiler == SpoilerLevelEnum.None && !_tagsDisplayNoneSpoilers)
                {
                    continue;
                }

                if (tag.Spoiler == SpoilerLevelEnum.Minimum && !_tagsDisplayMinimumSpoilers)
                {
                    continue;
                }

                if (tag.Spoiler == SpoilerLevelEnum.Major && !_tagsDisplayMajorSpoilers)
                {
                    continue;
                }

                if (_tagsMinimumScore > tag.Rating)
                {
                    continue;
                }

                if (TagsDisplayOption == TagsDisplayOptionEnum.All || counters[tag.Category] < 15)
                {
                    counters[tag.Category]++;
                    yield return tag;
                }

            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public RelayCommand<Vn> OpenVnVndbPageCommand
        {
            get => new RelayCommand<Vn>((Vn visualNovel) =>
            {
                if (visualNovel != null && !visualNovel.Id.IsNullOrEmpty())
                {
                    ProcessStarter.StartUrl($"https://vndb.org/{visualNovel.Id}");
                }
            });
        }

        public RelayCommand<CharacterTrait> OpenTraitVndbPageCommand
        {
            get => new RelayCommand<CharacterTrait>((CharacterTrait characterTrait) =>
            {
                if (characterTrait != null && !characterTrait.Id.IsNullOrEmpty())
                {
                    ProcessStarter.StartUrl($"https://vndb.org/{characterTrait.Id}");
                }
            });
        }
    }
}