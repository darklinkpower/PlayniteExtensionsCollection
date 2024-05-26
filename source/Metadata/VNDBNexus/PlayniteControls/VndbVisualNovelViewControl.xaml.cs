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
using VndbApiDomain.CharacterAggregate;
using VndbApiDomain.ImageAggregate;
using VndbApiDomain.ReleaseAggregate;
using VndbApiDomain.SharedKernel;
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
        private readonly BbCodeProcessor _bbcodeProcessor;
        private readonly IPlayniteAPI _playniteApi;
        private readonly string _pluginStoragePath;
        private readonly VNDBNexusSettingsViewModel _settingsViewModel;
        private readonly DesktopView _activeViewAtCreation;
        private readonly DispatcherTimer _updateControlDataDelayTimer;
        private Game _currentGame;
        private Guid _currentGameId = Guid.Empty;

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

        public VndbVisualNovelViewControl(VNDBNexus plugin, VNDBNexusSettingsViewModel settingsViewModel, BbCodeProcessor bbcodeProcessor, VndbDatabase vndbDatabase)
        {
            var imageUriToBitmapImageConverter = new ImageUriToBitmapImageConverter(Path.Combine(plugin.GetPluginUserDataPath(), "ImagesCache"));
            Resources.Add("ImageUriToBitmapImageConverter", imageUriToBitmapImageConverter);
            InitializeComponent();
            _vndbDatabase = vndbDatabase;
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

            var vndbId = VndbUtilities.GetVndbIdFromLinks(_currentGame);
            if (!vndbId.IsNullOrEmpty())
            {
                await LoadVisualNovelById(vndbId).ConfigureAwait(false);
            }
        }

        private async Task LoadVisualNovelById(string vndbId)
        {
            var contextGame = _currentGame;
            var contextGameId = contextGame.Id;
            _currentGameId = contextGameId;

            var visualNovel = _vndbDatabase.VisualNovels.GetById(vndbId);
            if (visualNovel is null)
            {
                var updateSuccess = await UpdateVisualNovel(vndbId);
                if (!updateSuccess || _currentGameId == null || _currentGameId != contextGameId)
                {
                    return;
                }

                visualNovel = _vndbDatabase.VisualNovels.GetById(vndbId);
            }

            var vnMatchingTags = _vndbDatabase.Tags.GetByIds(visualNovel.Tags.Select(x => x.Id)).ToDictionary(x => x.Id);
            foreach (var tag in visualNovel.Tags)
            {
                if (vnMatchingTags.TryGetValue(tag.Id, out var matchingTag))
                {
                    tag.Name = matchingTag.Name;
                }
            }

            // Releases
            var vnRelations = _vndbDatabase.VisualNovelRelations.GetOrCreateById(visualNovel.Id);
            if (vnRelations.ReleaseIds is null)
            {
                var updateSuccess = await UpdateVisualNovelReleases(vndbId, vnRelations);
                if (!updateSuccess || _currentGameId == null || _currentGameId != contextGameId)
                {
                    return;
                }
            }

            var matchingReleases = _vndbDatabase.Releases.GetByIds(vnRelations.ReleaseIds);
            var developers = matchingReleases.SelectMany(x => x.Producers.Where(p => p.IsDeveloper));
            var publishers = matchingReleases.SelectMany(x => x.Producers.Where(p => p.IsPublisher));

            // Characters
            if (vnRelations.CharacterIds is null)
            {
                var updateSuccess = await UpdateVisualNovelCharacters(vndbId, vnRelations);
                if (!updateSuccess || _currentGameId == null || _currentGameId != contextGameId)
                {
                    return;
                }
            }

            var matchingCharacters = _vndbDatabase.Characters.GetByIds(vnRelations.CharacterIds);
            var uniqueTraitIds = matchingCharacters
                .SelectMany(character => character.Traits)
                .Select(trait => trait.Id)
                .Distinct();

            var matchingTraits = _vndbDatabase.Traits.GetByIds(uniqueTraitIds).ToDictionary(x => x.Id);
            var voiceActors = visualNovel.VoiceActors.ToDictionary(x => x.Character.Id);

            var characterWrappers = matchingCharacters.Select(c => GetCharacterWrapper(c, matchingTraits, voiceActors));

            ActiveVisualNovel = visualNovel;
            Releases = matchingReleases;
            CharacterWrappers = characterWrappers;
            Developers = developers;
            Publishers = publishers;
            SetVisibleVisibility();
        }

        private async Task<bool> UpdateVisualNovel(string vndbId)
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

            var reponse = await VndbService.ExecutePostRequestAsync(query);
            if (reponse is null || !reponse.Results.HasItems())
            {
                return false;
            }

            var visualNovel = reponse.Results.First();
            _vndbDatabase.VisualNovels.Insert(visualNovel);
            return true;
        }

        private async Task<bool> UpdateVisualNovelReleases(string vndbId, VisualNovelRelations vnRelations)
        {
            var vndbRequestFilter = VisualNovelFilterFactory.Id.EqualTo(vndbId);
            var releasesRequestFilter = ReleaseFilterFactory.VisualNovel.EqualTo(vndbRequestFilter);
            var query = new ReleaseRequestQuery(releasesRequestFilter);
            query.Fields.DisableAllFlags(true);
            query.Fields.EnableAllFlags(false);
            query.Fields.Subfields.ExternalLinks.EnableAllFlags();
            query.Fields.Subfields.Producer.Flags = StaffRequestFieldsFlags.Id | StaffRequestFieldsFlags.Name | StaffRequestFieldsFlags.Original;
            query.Page = 1;
            query.Results = 40;

            var releaseIds = new List<string>();
            while (true)
            {
                var response = await VndbService.ExecutePostRequestAsync(query);
                if (response is null)
                {
                    return false;
                }

                if (response.Results.HasItems())
                {
                    foreach (var release in response.Results)
                    {
                        _vndbDatabase.Releases.InsertOrReplace(release);
                    }

                    releaseIds.AddRange(response.Results.Select(c => c.Id));
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

            vnRelations.ReleaseIds = releaseIds;
            _vndbDatabase.VisualNovelRelations.Update(vnRelations);
            return true;
        }

        private async Task<bool> UpdateVisualNovelCharacters(string vndbId, VisualNovelRelations vnRelations)
        {
            var vndbRequestFilter = VisualNovelFilterFactory.Id.EqualTo(vndbId);
            var characterRequestFilter = CharacterFilterFactory.VisualNovel.EqualTo(vndbRequestFilter);
            var query = new CharacterRequestQuery(characterRequestFilter);
            query.Fields.DisableAllFlags(true);
            query.Fields.Subfields.Image.EnableAllFlags();
            query.Fields.Subfields.Traits.Flags = TraitRequestFieldsFlags.Id | TraitRequestFieldsFlags.GroupName;
            query.Fields.EnableAllFlags(false);
            query.Page = 1;
            query.Results = 40;

            var charaterIds = new List<string>();
            while (true)
            {
                var response = await VndbService.ExecutePostRequestAsync(query);
                if (response is null)
                {
                    return false;
                }

                if (response.Results.HasItems())
                {
                    foreach (var character in response.Results)
                    {
                        _vndbDatabase.Characters.InsertOrReplace(character);
                    }

                    charaterIds.AddRange(response.Results.Select(c => c.Id).ToList());
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

            vnRelations.CharacterIds = charaterIds;
            _vndbDatabase.VisualNovelRelations.Update(vnRelations);
            return true;
        }

        private CharacterWrapper GetCharacterWrapper(
            Character character,
            Dictionary<string, Trait> matchingTraits,
            Dictionary<string, VisualNovelVoiceActor> voiceActors)
        {
            foreach (var trait in character.Traits)
            {
                if (matchingTraits.TryGetValue(trait.Id, out var matchingTrait))
                {
                    trait.Name = matchingTrait.Name;
                    trait.Description = matchingTrait.Description;
                }
            }

            if (voiceActors.TryGetValue(character.Id, out var voiceActor))
            {
                return new CharacterWrapper(character, voiceActor);
            }
            else
            {
                return new CharacterWrapper(character, null);
            }
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

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

        public RelayCommand<object> OpenUriInWebViewCommand
        {
            get => new RelayCommand<object>((object parameter) =>
            {
                if (parameter is Uri uri)
                {
                    using (var webView = _playniteApi.WebViews.CreateView(1280, 720))
                    {
                        webView.Navigate(uri.ToString());
                        webView.OpenDialog();
                    }
                }
            });
        }
    }
}