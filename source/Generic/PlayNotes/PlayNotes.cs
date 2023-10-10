using PlayNotes.Models;
using PlayNotes.PlayniteControls;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DatabaseCommon;
using System.Windows;

namespace PlayNotes
{
    public class PlayNotes : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private LiteDbRepository<MarkdownDatabaseItem> _notesDatabase;
        private const string pluginElementsSourceName = "PlayNotes";
        private const string notesControlName = "NotesViewerControl";
        public PlayNotesSettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("4208657d-4f78-42d2-968f-39f24de275e1");

        public PlayNotes(IPlayniteAPI api) : base(api)
        {
            Settings = new PlayNotesSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                SourceName = pluginElementsSourceName,
                ElementList = new List<string> { notesControlName }
            });

            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = pluginElementsSourceName,
                SettingsRoot = $"{nameof(Settings)}.{nameof(Settings.Settings)}"
            });

            var databasePath = Path.Combine(GetPluginUserDataPath(), "database.db");
            _notesDatabase = new LiteDbRepository<MarkdownDatabaseItem>(databasePath);

            PlayniteApi.Database.Games.ItemCollectionChanged += (sender, ItemCollectionChangedArgs) =>
            {
                foreach (var removedItem in ItemCollectionChangedArgs.RemovedItems)
                {
                    _notesDatabase.AddToDeleteBuffer(removedItem.Id);
                }
            };
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == notesControlName)
            {
                return new NotesViewerControl(PlayniteApi, Settings.Settings, _notesDatabase);
            }

            return null;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new PlayNotesSettingsView();
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            _notesDatabase.Dispose();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("PlayNotes_MainMenuItemImportPlayniteNotesLabel"),
                    MenuSection = "@Play Notes",
                    Action = a =>
                    {
                        ImportPlayniteNotes();
                    }
                }
            };
        }

        private void ImportPlayniteNotes()
        {
            var options = new List<MessageBoxOption>
            {
                new MessageBoxOption(ResourceProvider.GetString("LOCMetaGameSourceSelected")),
                new MessageBoxOption(ResourceProvider.GetString("LOCMetaGameSourceAll")),
                new MessageBoxOption(ResourceProvider.GetString("LOCCancelLabel"), true, true)
            };

            var selected = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCSelect"), "Play Notes", MessageBoxImage.None, options);
            if (selected.IsCancel)
            {
                return;
            }

            var titleSelection = PlayniteApi.Dialogs.SelectString(
                ResourceProvider.GetString("PlayNotes_ImportNotesEnterNotesTitlesMessage"),
                "Play Notes",
                ResourceProvider.GetString("LOCNotesLabel"));
            if (!titleSelection.Result)
            {
                return;
            }

            var notesTitle = titleSelection.SelectedString;
            var progressOptions = new GlobalProgressOptions(ResourceProvider.GetString("PlayNotes_ImportNotesImportingMessage"), true)
            {
                IsIndeterminate = false
            };

            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                var games = selected == options[0] ? PlayniteApi.MainView.SelectedGames : PlayniteApi.Database.Games;
                a.ProgressMaxValue = games.Count();
                foreach (var game in games)
                {
                    a.CurrentProgressValue++;
                    if (a.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (game.Notes.IsNullOrEmpty())
                    {
                        continue;
                    }

                    var databaseItem = _notesDatabase.GetOrCreateById(game.Id);
                    if (databaseItem.Notes.Any(x => x.Text == game.Notes))
                    {
                        continue;
                    }

                    var newNote = new PlayNote(notesTitle, game.Notes);
                    databaseItem.Notes.Add(newNote);
                    _notesDatabase.Update(databaseItem);
                }

            }, progressOptions);

            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("PlayNotes_ImportNotesImportFinishedMessage"), "Play Notes");
        }
    }
}