using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using PluginsCommon.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VndbApiDomain.CharacterAggregate;
using VndbApiDomain.ImageAggregate;
using VndbApiDomain.ReleaseAggregate;
using VndbApiDomain.SharedKernel;
using VndbApiDomain.StaffAggregate;
using VndbApiDomain.TagAggregate;
using VndbApiDomain.TraitAggregate;
using VndbApiDomain.VisualNovelAggregate;
using VndbApiInfrastructure.CharacterAggregate;
using VndbApiInfrastructure.ProducerAggregate;
using VndbApiInfrastructure.ReleaseAggregate;
using VndbApiInfrastructure.Services;
using VndbApiInfrastructure.SharedKernel.Responses;
using VndbApiInfrastructure.StaffAggregate;
using VndbApiInfrastructure.TagAggregate;
using VndbApiInfrastructure.TraitAggregate;
using VndbApiInfrastructure.VisualNovelAggregate;
using VNDBNexus.Converters;
using VNDBNexus.Database;
using VNDBNexus.Enums;
using VNDBNexus.Screenshots;
using VNDBNexus.Shared.DatabaseCommon;

namespace VNDBNexus.PlayniteControls
{
    /// <summary>
    /// Interaction logic for VndbVisualNovelViewControl.xaml
    /// </summary>
    public partial class VndbVisualNovelViewControl : PluginUserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly VndbDatabase _vndbDatabase;
        private readonly IPlayniteAPI _playniteApi;
        private readonly string _pluginStoragePath;
        private readonly VNDBNexusSettingsViewModel _settingsViewModel;
        private readonly DesktopView _activeViewAtCreation;
        private readonly DispatcherTimer _updateControlDataDelayTimer;
        private readonly ImageUriToBitmapImageConverter _imageUriToBitmapImageConverter;
        private bool _isValuesDefaultState = true;
        private Game _currentGame;
        private Guid _activeContext = default;

        private IEnumerable<ReleaseProducer> _developers;
        public IEnumerable<ReleaseProducer> Developers
        {
            get => _developers;
            set
            {
                _developers = value;
                OnPropertyChanged();
            }
        }

        private IEnumerable<ReleaseProducer> _publishers;
        public IEnumerable<ReleaseProducer> Publishers
        {
            get => _publishers;
            set
            {
                _publishers = value;
                OnPropertyChanged();
            }
        }

        private IEnumerable<Release> _releases;
        public IEnumerable<Release> Releases
        {
            get => _releases;
            set
            {
                _releases = value;
                OnPropertyChanged();
            }
        }

        private IEnumerable<CharacterWrapper> _characterWrappers;
        public IEnumerable<CharacterWrapper> CharacterWrappers
        {
            get => _characterWrappers;
            set
            {
                _characterWrappers = value;
                OnPropertyChanged();
                SetCharacterWrappers();
            }
        }

        private Visibility _mainCharacterWrappersVisibility = Visibility.Collapsed;
        public Visibility MainCharacterWrappersVisibility
        {
            get => _mainCharacterWrappersVisibility;
            set
            {
                _mainCharacterWrappersVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _primaryCharacterWrappersVisibility = Visibility.Collapsed;
        public Visibility PrimaryCharacterWrappersVisibility
        {
            get => _primaryCharacterWrappersVisibility;
            set
            {
                _primaryCharacterWrappersVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _sideCharacterWrappersVisibility = Visibility.Collapsed;
        public Visibility SideCharacterWrappersVisibility
        {
            get => _sideCharacterWrappersVisibility;
            set
            {
                _sideCharacterWrappersVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _appearsCharacterWrappersVisibility = Visibility.Collapsed;
        public Visibility AppearsCharacterWrappersVisibility
        {
            get => _appearsCharacterWrappersVisibility;
            set
            {
                _appearsCharacterWrappersVisibility = value;
                OnPropertyChanged();
            }
        }

        private void SetCharacterWrappers()
        {
            if (_characterWrappers?.Any() == true)
            {
                MainCharacterWrappers = GetFilteredCharacters(CharacterRoleEnum.Main);
                PrimaryCharacterWrappers = GetFilteredCharacters(CharacterRoleEnum.Primary);
                SideCharacterWrappers = GetFilteredCharacters(CharacterRoleEnum.Side);
                AppearsCharacterWrappers = GetFilteredCharacters(CharacterRoleEnum.Appears);
            }
            else
            {
                MainCharacterWrappers = Enumerable.Empty<CharacterWrapper>();
                PrimaryCharacterWrappers = Enumerable.Empty<CharacterWrapper>();
                SideCharacterWrappers = Enumerable.Empty<CharacterWrapper>();
                AppearsCharacterWrappers = Enumerable.Empty<CharacterWrapper>();
            }
        }

        private IEnumerable<CharacterWrapper> _mainCharacterWrappers;
        public IEnumerable<CharacterWrapper> MainCharacterWrappers
        {
            get => _mainCharacterWrappers;
            set
            {
                _mainCharacterWrappers = value;
                OnPropertyChanged();
                MainCharacterWrappersVisibility = _mainCharacterWrappers?.Any() == true
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private IEnumerable<CharacterWrapper> _primaryCharacterWrappers;
        public IEnumerable<CharacterWrapper> PrimaryCharacterWrappers
        {
            get => _primaryCharacterWrappers;
            set
            {
                _primaryCharacterWrappers = value;
                OnPropertyChanged();
                PrimaryCharacterWrappersVisibility = _primaryCharacterWrappers?.Any() == true
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private IEnumerable<CharacterWrapper> _sideCharacterWrappers;
        public IEnumerable<CharacterWrapper> SideCharacterWrappers
        {
            get => _sideCharacterWrappers;
            set
            {
                _sideCharacterWrappers = value;
                OnPropertyChanged();
                SideCharacterWrappersVisibility = _sideCharacterWrappers?.Any() == true
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private IEnumerable<CharacterWrapper> _appearsCharacterWrappers;
        public IEnumerable<CharacterWrapper> AppearsCharacterWrappers
        {
            get => _appearsCharacterWrappers;
            set
            {
                _appearsCharacterWrappers = value;
                OnPropertyChanged();
                AppearsCharacterWrappersVisibility = _appearsCharacterWrappers?.Any() == true
                    ? Visibility.Visible
                    : Visibility.Collapsed;
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
                SetCharacterWrappers();
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

        private VisualNovel _activeVisualNovel;
        public VisualNovel ActiveVisualNovel
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

        public IEnumerable<VisualNovelTag> TagsToDisplay =>
            ActiveVisualNovel?.Tags?.HasItems() == true
            ? GetTagsToDisplay()
            : Enumerable.Empty<VisualNovelTag>();

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

        private GroupedDictionary<LanguageEnum, Release> _groupedReleasesByLanguage;
        public GroupedDictionary<LanguageEnum, Release> GroupedReleasesByLanguage
        {
            get { return _groupedReleasesByLanguage; }
            set
            {
                _groupedReleasesByLanguage = value;
                OnPropertyChanged();
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

        private bool _displayUnnoficialRelations = true;
        public bool DisplayUnnoficialRelations
        {
            get => _displayUnnoficialRelations;
            set
            {
                _displayUnnoficialRelations = value;
                OnPropertyChanged();
            }
        }

        private ImageSexualityLevelEnum _screenshotsMaxSexualityLevel = ImageSexualityLevelEnum.Safe;
        public ImageSexualityLevelEnum ScreenshotsMaxSexualityLevel
        {
            get => _screenshotsMaxSexualityLevel;
            set
            {
                _screenshotsMaxSexualityLevel = value;
                OnPropertyChanged();
            }
        }

        private ImageViolenceLevelEnum _screenshotsMaxViolenceLevel = ImageViolenceLevelEnum.Tame;
        public ImageViolenceLevelEnum ScreenshotsMaxViolenceLevel
        {
            get => _screenshotsMaxViolenceLevel;
            set
            {
                _screenshotsMaxViolenceLevel = value;
                OnPropertyChanged();
            }
        }

        public VndbVisualNovelViewControl(VNDBNexus plugin, VNDBNexusSettingsViewModel settingsViewModel, VndbDatabase vndbDatabase, ImageUriToBitmapImageConverter imageUriToBitmapImageConverter)
        {
            _imageUriToBitmapImageConverter = imageUriToBitmapImageConverter;
            Resources.Add("ImageUriToBitmapImageConverter", imageUriToBitmapImageConverter);
            _playniteApi = plugin.PlayniteApi;
            SetControlTextBlockStyle();
            
            _vndbDatabase = vndbDatabase;
            
            _pluginStoragePath = plugin.GetPluginUserDataPath();
            _settingsViewModel = settingsViewModel;
            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                _activeViewAtCreation = _playniteApi.MainView.ActiveDesktopView;
            }

            _updateControlDataDelayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };

            _updateControlDataDelayTimer.Tick += new EventHandler(UpdateControlData);

            InitializeComponent();
            DataContext = this;
        }

        private void SetControlTextBlockStyle()
        {
            // Desktop mode uses BaseTextBlockStyle and Fullscreen Mode uses TextBlockBaseStyle
            var baseStyleName = _playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop ? "BaseTextBlockStyle" : "TextBlockBaseStyle";
            if (ResourceProvider.GetResource(baseStyleName) is Style baseStyle && baseStyle.TargetType == typeof(TextBlock))
            {
                var implicitStyle = new Style(typeof(TextBlock), baseStyle);
                Resources.Add(typeof(TextBlock), implicitStyle);
            }
        }

        private async void UpdateControlData(object sender, EventArgs e)
        {
            _updateControlDataDelayTimer.Stop();
            await UpdateControlAsync();
        }

        private void SetVisibleVisibility()
        {
            CharactersTabVisibility = CharacterWrappers?.Any() == true ? Visibility.Visible : Visibility.Collapsed;
            ReleasesTabVisibility = GroupedReleasesByLanguage?.GroupedResults?.Any() == true ? Visibility.Visible : Visibility.Collapsed;
            ScreenshotsTabVisibility = ActiveVisualNovel?.Screenshots?.Any() == true ? Visibility.Visible : Visibility.Collapsed;
            RelationsSectionVisibility = ActiveVisualNovel?.Relations?.Any() == true ? Visibility.Visible : Visibility.Collapsed;
            RelationsUnofficialButonVisibility = ActiveVisualNovel?.Relations?.Any(vn => !vn.RelationOfficial) == true ? Visibility.Visible : Visibility.Collapsed;
            SelectedControlTabIndex = 0;
            SelectedCharactersControlTabIndex = CharacterWrappers?.Any(x => x.Role == CharacterRoleEnum.Main && x.SpoilerLevel == SpoilerLevelEnum.None) == true ? 0 : 1;

            Visibility = Visibility.Visible;
            _settingsViewModel.Settings.IsControlVisible = true;
        }

        private Visibility _relationsSectionVisibility = Visibility.Collapsed;
        public Visibility RelationsSectionVisibility
        {
            get => _relationsSectionVisibility;
            set
            {
                _relationsSectionVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _CharactersTabVisibility = Visibility.Collapsed;
        public Visibility CharactersTabVisibility
        {
            get => _CharactersTabVisibility;
            set
            {
                _CharactersTabVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _releasesTabVisibility = Visibility.Collapsed;
        public Visibility ReleasesTabVisibility
        {
            get => _releasesTabVisibility;
            set
            {
                _releasesTabVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _screenshotsTabVisibility = Visibility.Collapsed;
        public Visibility ScreenshotsTabVisibility
        {
            get => _screenshotsTabVisibility;
            set
            {
                _screenshotsTabVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _relationsUnofficialButonVisibility = Visibility.Collapsed;
        public Visibility RelationsUnofficialButonVisibility
        {
            get => _relationsUnofficialButonVisibility;
            set
            {
                _relationsUnofficialButonVisibility = value;
                OnPropertyChanged();
            }
        }

        private int _selectedControlTabIndex = 0;
        public int SelectedControlTabIndex
        {
            get => _selectedControlTabIndex;
            set
            {
                _selectedControlTabIndex = value;
                OnPropertyChanged();
            }
        }

        private int _selectedCharactersControlTabIndex = 0;
        public int SelectedCharactersControlTabIndex
        {
            get => _selectedCharactersControlTabIndex;
            set
            {
                _selectedCharactersControlTabIndex = value;
                OnPropertyChanged();
            }
        }


        private void SetCollapsedVisibility()
        {
            Visibility = Visibility.Collapsed;
            _settingsViewModel.Settings.IsControlVisible = false;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            //The GameContextChanged method is rised even when the control
            //is not in the active view. To prevent unecessary processing we
            //can stop processing if the active view is not the same one was
            //the one during creation
            _updateControlDataDelayTimer.Stop();
            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop && _activeViewAtCreation != _playniteApi.MainView.ActiveDesktopView)
            {
                return;
            }

            if (!_isValuesDefaultState)
            {
                ResetToDefaultValues();
            }

            if (newContext != null &&_settingsViewModel.Settings.EnableVnViewControl)
            {
                _currentGame = newContext;
                _updateControlDataDelayTimer.Start();
            }
        }

        private void ResetToDefaultValues()
        {
            SetCollapsedVisibility();
            _activeContext = default;
            ActiveVisualNovel = null;
            Developers = Enumerable.Empty<ReleaseProducer>();
            Publishers = Enumerable.Empty<ReleaseProducer>();
            Releases = Enumerable.Empty<Release>();
            CharacterWrappers = Enumerable.Empty<CharacterWrapper>();
            CharactersMaxSpoilerLevel = SpoilerLevelEnum.None;
            DisplayCharacterSexualTraits = false;
            TagsMinimumScore = 2.0;
            GroupedReleasesByLanguage = new GroupedDictionary<LanguageEnum, Release>();
            TagsDisplayOption = TagsDisplayOptionEnum.Summary;
            TagsDisplayNoneSpoilers = true;
            TagsDisplayMinimumSpoilers = false;
            TagsDisplayMajorSpoilers = false;
            TagsDisplayContentCategory = true;
            TagsDisplayTechnicalCategory = false;
            TagsDisplaySexualCategory = false;
            DisplayUnnoficialRelations = true;
            ScreenshotsMaxSexualityLevel = ImageSexualityLevelEnum.Safe;
            ScreenshotsMaxViolenceLevel = ImageViolenceLevelEnum.Tame;

            _isValuesDefaultState = true;
        }

        private async Task UpdateControlAsync()
        {
            if (_currentGame is null)
            {
                return;
            }

            await LoadGameVisualNovelAsync(_currentGame, false);
        }

        private async Task LoadGameVisualNovelAsync(Game game, bool forceUpdate, CancellationToken cancellationToken = default)
        {
            var vndbId = VndbUtilities.GetVndbIdFromLinks(game);
            if (!vndbId.IsNullOrEmpty())
            {
                await LoadVisualNovelByIdAsync(vndbId, forceUpdate).ConfigureAwait(false);
            }
        }

        private async Task LoadVisualNovelByIdAsync(string vndbId, bool forceUpdate, CancellationToken cancellationToken = default)
        {
            var contextId = Guid.NewGuid();
            _activeContext = contextId;
            _isValuesDefaultState = false;
            var visualNovel = _vndbDatabase.VisualNovels.GetById(vndbId);
            if (visualNovel is null || forceUpdate)
            {
                var updateSuccess = await UpdateVisualNovel(vndbId, cancellationToken);
                if (!updateSuccess || _activeContext != contextId || cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                visualNovel = _vndbDatabase.VisualNovels.GetById(vndbId);
                await DownloadVisualNovelImages(visualNovel);
            }

            var vnMatchingTags = _vndbDatabase.DatabaseDumpTags.GetByIds(visualNovel.Tags.Select(x => x.Id)).ToDictionary(x => x.Id);
            foreach (var tag in visualNovel.Tags)
            {
                if (vnMatchingTags.TryGetValue(tag.Id, out var matchingTag))
                {
                    tag.Name = matchingTag.Tag.Name;
                }
            }

            // Releases
            var releasesGroup = _vndbDatabase.Releases.GetById(visualNovel.Id);
            if (releasesGroup is null || forceUpdate)
            {
                var updateSuccess = await UpdateVisualNovelReleases(visualNovel, cancellationToken);
                if (!updateSuccess || _activeContext != contextId || cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                releasesGroup = _vndbDatabase.Releases.GetById(visualNovel.Id);
            }

            var developers = releasesGroup.Members.SelectMany(x => x.Producers.Where(p => p.IsDeveloper));
            var publishers = releasesGroup.Members.SelectMany(x => x.Producers.Where(p => p.IsPublisher));

            // Characters
            var charactersGroup = _vndbDatabase.Characters.GetById(visualNovel.Id);
            if (charactersGroup is null || forceUpdate)
            {
                var updateSuccess = await UpdateVisualNovelCharacters(visualNovel, cancellationToken);
                if (!updateSuccess || _activeContext != contextId || cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                charactersGroup = _vndbDatabase.Characters.GetById(visualNovel.Id);
                if (charactersGroup.Members != null)
                {
                    await DownloadCharactersImages(charactersGroup.Members);
                }
            }

            var matchingCharacters = charactersGroup.Members;
            var uniqueTraitIds = matchingCharacters
                .SelectMany(character => character.Traits)
                .Select(trait => trait.Id)
                .Distinct();

            var matchingTraits = _vndbDatabase.DatabaseDumpTraits.GetByIds(uniqueTraitIds).ToDictionary(x => x.Id);

            var groupedVoiceActors = new GroupedDictionary<string, VisualNovelVoiceActor>(visualNovel.VoiceActors, va => va.Character.Id);
            var characterWrappers = matchingCharacters.Select(c => GetCharacterWrapper(c, matchingTraits, groupedVoiceActors, visualNovel));
            
            ActiveVisualNovel = visualNovel;
            CharacterWrappers = characterWrappers;
            Developers = developers;
            Publishers = publishers;

            GroupedReleasesByLanguage = new GroupedDictionary<LanguageEnum, Release>(
                releasesGroup.Members.Distinct(),
                release => release.LanguagesAvailability.Select(langInfo => langInfo.Language),
                releases => releases
                    .OrderBy(r => r.ReleaseDate?.Year ?? 2222)
                    .ThenBy(r => r.ReleaseDate?.Month ?? 13)
                    .ThenBy(r => r.ReleaseDate?.Day ?? 32));
            CharacterWrappers = characterWrappers.OrderBy(c => c.Character.Name);

            var groupedDevelopers = new GroupedDictionary<LanguageEnum, ReleaseProducer>(developers.Distinct(), dev => dev.Language);
            var groupedPublishers = new GroupedDictionary<LanguageEnum, ReleaseProducer>(publishers.Distinct(), dev => dev.Language);
            _playniteApi.MainView.UIDispatcher.Invoke(() => SetVisibleVisibility());
        }

        private async Task DownloadCharactersImages(List<Character> characters)
        {
            if (!characters.HasItems())
            {
                return;
            }

            var tasks = new List<Func<Task>>();
            foreach (var character in characters)
            {
                if (character.Image is null)
                {
                    continue;
                }

                tasks.Add(async () =>
                {
                    await _imageUriToBitmapImageConverter.DownloadUriToStorageAsync(character.Image.Url);
                });
            }

            using (var taskExecutor = new TaskExecutor(6))
            {
                await taskExecutor.ExecuteAsync(tasks);
            }
        }

        private async Task DownloadVisualNovelImages(VisualNovel visualNovel)
        {
            if (visualNovel is null)
            {
                return;
            }

            var tasks = new List<Func<Task>>();
            if (visualNovel.Image != null)
            {
                tasks.Add(async () =>
                {
                    await _imageUriToBitmapImageConverter.DownloadUriToStorageAsync(visualNovel.Image.Url);
                });
            }

            if (visualNovel.Screenshots.HasItems())
            {
                foreach (var vndbImage in visualNovel.Screenshots)
                {
                    tasks.Add(async () =>
                    {
                        await _imageUriToBitmapImageConverter.DownloadUriToStorageAsync(vndbImage.Url);
                    });

                    tasks.Add(async () =>
                    {
                        await _imageUriToBitmapImageConverter.DownloadUriToStorageAsync(vndbImage.ThumbnailUrl);
                    });
                }
            }

            using (var taskExecutor = new TaskExecutor(5))
            {
                await taskExecutor.ExecuteAsync(tasks);
            }
        }

        private async Task<bool> UpdateVisualNovel(string vndbId, CancellationToken cancellationToken)
        {
            var vndbRequestFilter = VisualNovelFilterFactory.Id.EqualTo(vndbId);
            var query = new VisualNovelRequestQuery(vndbRequestFilter);
            query.Fields.DisableAllFlags(true);
            query.Fields.EnableAllFlags(false);

            query.Fields.Subfields.ExternalLinks.EnableAllFlags();
            query.Fields.Subfields.Image.EnableAllFlags();
            query.Fields.Subfields.Screenshots.EnableAllFlags();
            query.Fields.Subfields.ScreenshotsRelease.Flags = ReleaseRequestFieldsFlags.Title | ReleaseRequestFieldsFlags.LanguagesMain | ReleaseRequestFieldsFlags.LanguagesTitle;
            query.Fields.Subfields.VisualNovelRelationsFlags = VnRequestFieldsFlags.Id | VnRequestFieldsFlags.Title | VnRequestFieldsFlags.ReleaseDate;
            query.Fields.Subfields.VoiceActorCharacter.Flags = CharacterRequestFieldsFlags.Id;
            query.Fields.Subfields.VoiceActor.Flags = StaffRequestFieldsFlags.Id | StaffRequestFieldsFlags.Name | StaffRequestFieldsFlags.Original;
            query.Fields.Subfields.Tags.Flags = TagRequestFieldsFlags.Id | TagRequestFieldsFlags.Category;
            query.Results = 1;

            var reponse = await VndbService.ExecutePostRequestAsync(query, cancellationToken);
            if (reponse is null || !reponse.Results.HasItems())
            {
                return false;
            }

            var visualNovel = reponse.Results.First();
            _vndbDatabase.VisualNovels.InsertOrReplace(visualNovel);
            return true;
        }

        private async Task<bool> UpdateVisualNovelReleases(VisualNovel visualNovel, CancellationToken cancellationToken)
        {
            var vndbRequestFilter = VisualNovelFilterFactory.Id.EqualTo(visualNovel.Id);
            var releasesRequestFilter = ReleaseFilterFactory.VisualNovel.EqualTo(vndbRequestFilter);
            var query = new ReleaseRequestQuery(releasesRequestFilter);
            query.Fields.DisableAllFlags(true);
            query.Fields.EnableAllFlags(false);
            query.Fields.Subfields.ExternalLinks.EnableAllFlags();
            query.Fields.Subfields.Producer.Flags = ProducerRequestFieldsFlags.Id | ProducerRequestFieldsFlags.Name | ProducerRequestFieldsFlags.Original | ProducerRequestFieldsFlags.Language;
            query.Page = 1;
            query.Results = 40;

            var releases = new List<Release>();
            while (true)
            {
                var response = await VndbService.ExecutePostRequestAsync(query, cancellationToken);
                if (response is null)
                {
                    return false;
                }

                if (response.Results.HasItems())
                {
                    releases.AddRange(response.Results);
                }

                if (response.More)
                {
                    query.Page++;
                }
                else
                {
                    break;
                }
            }

            var group = new VisualNovelReleasesSearchResults(visualNovel, releases);
            _vndbDatabase.Releases.InsertOrReplace(group);
            return true;
        }

        private async Task<bool> UpdateVisualNovelCharacters(VisualNovel visualNovel, CancellationToken cancellationToken)
        {
            var vndbRequestFilter = VisualNovelFilterFactory.Id.EqualTo(visualNovel.Id);
            var characterRequestFilter = CharacterFilterFactory.VisualNovel.EqualTo(vndbRequestFilter);
            var query = new CharacterRequestQuery(characterRequestFilter);
            query.Fields.DisableAllFlags(true);
            query.Fields.Subfields.Image.EnableAllFlags();
            query.Fields.Subfields.VisualNovelFlags = VnRequestFieldsFlags.Id;
            query.Fields.Subfields.Traits.Flags = TraitRequestFieldsFlags.Id | TraitRequestFieldsFlags.GroupName;
            query.Fields.EnableAllFlags(false);
            query.Page = 1;
            query.Results = 40;

            var characters = new List<Character>();
            while (true)
            {
                var response = await VndbService.ExecutePostRequestAsync(query, cancellationToken);
                if (response is null)
                {
                    return false;
                }

                if (response.Results.HasItems())
                {
                    characters.AddRange(response.Results);
                }

                if (response.More)
                {
                    query.Page++;
                }
                else
                {
                    break;
                }
            }

            var group = new VisualNovelCharactersSearchResults(visualNovel, characters);
            _vndbDatabase.Characters.InsertOrReplace(group);
            return true;
        }

        private CharacterWrapper GetCharacterWrapper(
            Character character,
            Dictionary<string, DatabaseDumpTraitWrapper> matchingTraits,
            GroupedDictionary<string, VisualNovelVoiceActor> voiceActorsGroups,
            VisualNovel visualNovel)
        {
            foreach (var trait in character.Traits)
            {
                if (matchingTraits.TryGetValue(trait.Id, out var matchingTrait))
                {
                    trait.Name = matchingTrait.Trait.Name;
                    trait.Description = matchingTrait.Trait.Description;
                }
            }

            var visualNovelAppearance = character.VisualNovelAppearances
                .FirstOrDefault(vnAppearance => vnAppearance.Id == visualNovel.Id);
            var voiceActorGroup = voiceActorsGroups[character.Id];
            var voiceActor = voiceActorGroup.FirstOrDefault();
            return new CharacterWrapper(character, voiceActor, visualNovelAppearance);
        }

        private IEnumerable<VisualNovelTag> GetTagsToDisplay()
        {
            var counters = new Dictionary<TagCategoryEnum, int>
            {
                [TagCategoryEnum.Content] = 0,
                [TagCategoryEnum.Technical] = 0,
                [TagCategoryEnum.SexualContent] = 0
            };

            foreach (var tag in ActiveVisualNovel.Tags.OrderByDescending(x => x.Rating))
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

        private IEnumerable<CharacterWrapper> GetFilteredCharacters(CharacterRoleEnum targetRole)
        {
            foreach (var characterWrapper in _characterWrappers)
            {
                if (characterWrapper.Role != targetRole)
                {
                    continue;
                }

                if (characterWrapper.SpoilerLevel == SpoilerLevelEnum.None)
                {
                    yield return characterWrapper;
                }
                else if (_charactersMaxSpoilerLevel == SpoilerLevelEnum.Major)
                {
                    yield return characterWrapper;
                }
                else if (characterWrapper.SpoilerLevel == _charactersMaxSpoilerLevel)
                {
                    yield return characterWrapper;
                }
            }
        }

        public void OpenVisualNovelScreenshots(VndbImage selectedImage = null)
        {
            if (_activeVisualNovel is null || !_activeVisualNovel.Screenshots.HasItems())
            {
                return;
            }

            var window = _playniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Width = 1330;
            window.Height = 845;
            window.Title = string.Format(
                ResourceProvider.GetString("LOC_VndbNexus_VisualNovelScreenshotsFormat"),
                _activeVisualNovel.Title);

            var screenshotsViewModel = new ScreenshotsViewModel();
            screenshotsViewModel.LoadImages(_activeVisualNovel.Screenshots);
            if (selectedImage != null)
            {
                screenshotsViewModel.SelectImage(selectedImage);
            }

            window.Owner = API.Instance.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.DataContext = screenshotsViewModel;
            window.Content = new ScreenshotsView(_imageUriToBitmapImageConverter);
            window.ShowDialog();
        }

        public RelayCommand<object> OpenScreenshotCommand
        {
            get => new RelayCommand<object>((object parameter) =>
            {
                if (parameter is VndbImage vndbImage)
                {
                    OpenVisualNovelScreenshots(vndbImage);
                }
            });
        }

        public RelayCommand<VisualNovel> OpenVnVndbPageCommand
        {
            get => new RelayCommand<VisualNovel>((VisualNovel visualNovel) =>
            {
                if (visualNovel != null && !visualNovel.Id.IsNullOrEmpty())
                {
                    ProcessStarter.StartUrl($"https://vndb.org/{visualNovel.Id}");
                }
            });
        }

        public RelayCommand<object> LoadVnRelationDataCommand
        {
            get => new RelayCommand<object>((object parameter) =>
            {
                if (parameter is VisualNovelRelation visualNovel)
                {
                    var dialogText = string.Format(ResourceProvider.GetString("LOC_VndbNexus_LoadingVndbDataProgressFormat"), visualNovel.Title);
                    var progressOptions = new GlobalProgressOptions(dialogText, true)
                    {
                        IsIndeterminate = true
                    };

                    ResetToDefaultValues();
                    _playniteApi.Dialogs.ActivateGlobalProgress(async (a) =>
                    {
                        await LoadVisualNovelByIdAsync(visualNovel.Id, false, a.CancelToken);
                    }, progressOptions);
                }
            });
        }

        public RelayCommand<object> RefreshVisualNovelDataCommand
        {
            get => new RelayCommand<object>((object parameter) =>
            {
                if (parameter is VisualNovel visualNovel)
                {
                    var dialogText = string.Format(ResourceProvider.GetString("LOC_VndbNexus_LoadingVndbDataProgressFormat"), visualNovel.Title);
                    var progressOptions = new GlobalProgressOptions(dialogText, true)
                    {
                        IsIndeterminate = true
                    };

                    _playniteApi.Dialogs.ActivateGlobalProgress(async (a) =>
                    {
                        await LoadVisualNovelByIdAsync(visualNovel.Id, true, a.CancelToken);
                    }, progressOptions);
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

        public RelayCommand<object> OpenCharacterVndbPageCommand
        {
            get => new RelayCommand<object>((object parameter) =>
            {
                if (parameter is Character character)
                {
                    ProcessStarter.StartUrl($"https://vndb.org/{character.Id}");
                }
            });
        }

        public RelayCommand<object> OpenStaffVndbPageCommand
        {
            get => new RelayCommand<object>((object parameter) =>
            {
                if (parameter is Staff staff)
                {
                    ProcessStarter.StartUrl($"https://vndb.org/{staff.Id}");
                }
            });
        }

        public RelayCommand<object> OpenReleaseVndbPageCommand
        {
            get => new RelayCommand<object>((object parameter) =>
            {
                if (parameter is Release release)
                {
                    ProcessStarter.StartUrl($"https://vndb.org/{release.Id}");
                }
            });
        }

        public RelayCommand<object> OpenTagVndbPageCommand
        {
            get => new RelayCommand<object>((object parameter) =>
            {
                if (parameter is VndbApiDomain.TagAggregate.Tag tag)
                {
                    ProcessStarter.StartUrl($"https://vndb.org/{tag.Id}");
                }
            });
        }

        public RelayCommand<object> OpenUriCommand
        {
            get => new RelayCommand<object>((object parameter) =>
            {
                if (parameter is Uri uri)
                {
                    ProcessStarter.StartUrl(uri.ToString());
                }
            });
        }

        public RelayCommand<object> OpenUriInWebViewCommand
        {
            get => new RelayCommand<object>((object parameter) =>
            {
                if (parameter is Uri uri)
                {
                    using (var webView = _playniteApi.WebViews.CreateView(1200, 720))
                    {
                        webView.Navigate(uri.ToString());
                        webView.OpenDialog();
                    }
                }
            });
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}