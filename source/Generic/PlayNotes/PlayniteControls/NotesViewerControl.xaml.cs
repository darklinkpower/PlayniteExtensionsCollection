using PlayNotes.Models;
using MdXaml;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DatabaseCommon;

namespace PlayNotes.PlayniteControls
{
    /// <summary>
    /// Interaction logic for NotesViewerControl.xaml
    /// </summary>
    public partial class NotesViewerControl : PluginUserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            var caller = name;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private readonly IPlayniteAPI _playniteApi;
        public PlayNotesSettings Settings { get; private set; }
        private readonly LiteDbRepository<MarkdownDatabaseItem> _notesDatabase;
        private readonly Dispatcher _dispatcher;
        private readonly DesktopView ActiveViewAtCreation;
        private readonly DispatcherTimer _updateControl;
        
        private bool _multipleNotesAvailable = false;
        private bool _isSelectedItemFirst = false;
        private bool _isSelectedItemLast = false;
        private Game currentGame = null;

        public SteamGuideImporter SteamGuideImporter { get; }

        private PlayNote _selectedNotes;
        public PlayNote SelectedNotes
        {
            get => _selectedNotes;
            set
            {
                _selectedNotes = value;
                OnPropertyChanged();
            }
        }

        private MarkdownDatabaseItem _currentGameNotes = null;
        public MarkdownDatabaseItem CurrentGameNotes
        {
            get => _currentGameNotes;
            set
            {
                _currentGameNotes = value;
                OnPropertyChanged();
            }
        }

        private string _title = string.Empty; 
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private bool _expandNotes = false;
        public bool ExpandNotes
        {
            get => _expandNotes;
            set
            {
                _expandNotes = value;
                OnPropertyChanged();
            }
        }

        public Visibility _editorVisibility = Visibility.Collapsed;
        public Visibility EditorVisibility
        {
            get { return _editorVisibility; }
            set
            {
                if (_editorVisibility == value)
                {
                    return;
                }

                _editorVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility _toolsVisibility = Visibility.Collapsed;
        public Visibility ToolsVisibility
        {
            get { return _toolsVisibility; }
            set
            {
                if (_toolsVisibility == value)
                {
                    return;
                }

                _toolsVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility _notesSectionVisibility = Visibility.Collapsed;
        public Visibility NotesSectionVisibility
        {
            get { return _notesSectionVisibility; }
            set
            {
                if (_notesSectionVisibility == value)
                {
                    return;
                }

                _notesSectionVisibility = value;
                OnPropertyChanged();
            }
        }

        public NotesViewerControl(IPlayniteAPI playniteApi, PlayNotesSettings settings, LiteDbRepository<MarkdownDatabaseItem> notesDatabase)
        {
            var engine = new Markdown(); // For some reason there's a crash during initialization and this fixes it
            InitializeComponent();
            _playniteApi = playniteApi;
            Settings = settings;
            _notesDatabase = notesDatabase;
            _dispatcher = Application.Current.Dispatcher;
            DataContext = this;
            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                ActiveViewAtCreation = _playniteApi.MainView.ActiveDesktopView;
            }

            _updateControl = new DispatcherTimer();
            _updateControl.Interval = TimeSpan.FromMilliseconds(300);
            _updateControl.Tick += UpdateControl_Tick;
            SetControlTextBlockStyle();
            SteamGuideImporter = new SteamGuideImporter(playniteApi);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExpandNotes = false;
            EditorVisibility = Visibility.Collapsed;
            UpdateSectionsVisibilityAndCanExecute();
        }

        private void SetControlTextBlockStyle()
        {
            // Desktop mode uses BaseTextBlockStyle and Fullscreen Mode uses TextBlockBaseStyle
            var baseStyleName = _playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop ? "BaseTextBlockStyle" : "TextBlockBaseStyle";
            if (ResourceProvider.GetResource(baseStyleName) is Style baseStyle &&
                baseStyle.TargetType == typeof(TextBlock))
            {
                var implicitStyle = new Style(typeof(TextBlock), baseStyle);
                Resources.Add(typeof(TextBlock), implicitStyle);
            }
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            _updateControl.Stop();
            //The GameContextChanged method is rised even when the control
            //is not in the active view. To prevent unecessary processing we
            //can stop processing if the active view is not the same one was
            //the one during creation
            if (_playniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop &&
                ActiveViewAtCreation != _playniteApi.MainView.ActiveDesktopView)
            {
                return;
            }

            Visibility = Visibility.Collapsed;
            NotesSectionVisibility = Visibility.Collapsed;
            Settings.IsControlVisible = false;
            if (newContext is null)
            {
                CurrentGameNotes = null;
                SelectedNotes = null;
                return;
            }

            currentGame = newContext;
            _updateControl.Start();
        }

        private async void UpdateControl_Tick(object sender, EventArgs e)
        {
            _updateControl.Stop();
            await _dispatcher.Invoke(async () =>
            {
                await UpdateControlAsync();
            });
        }

        private async Task UpdateControlAsync()
        {
            var gameNotesItem = _notesDatabase.GetOrCreateById(currentGame.Id);
            var clonedObject = gameNotesItem.GetClone();
            CurrentGameNotes = clonedObject;
            SelectedNotes = CurrentGameNotes.Notes.HasItems() ? CurrentGameNotes.Notes.First() : null;
            ExpandNotes = false;
            EditorVisibility = Visibility.Collapsed;
            ToolsVisibility = Visibility.Collapsed;
            Visibility = Visibility.Visible;
            Settings.IsControlVisible = true;
            SteamGuideImporter.ResetValues();
            await Task.Run(UpdateSectionsVisibilityAndCanExecute);
        }

        private void UpdateSectionsVisibilityAndCanExecute()
        {
            NotesSectionVisibility = _currentGameNotes?.Notes.HasItems() == true ? Visibility.Visible : Visibility.Collapsed;
            _multipleNotesAvailable = _currentGameNotes?.Notes?.Count > 1;

            _isSelectedItemFirst = false;
            _isSelectedItemLast = false;
            if (!(_selectedNotes is null || _currentGameNotes is null))
            {
                var currendIndex = _currentGameNotes.Notes.IndexOf(_selectedNotes);
                if (currendIndex != -1)
                {
                    if (currendIndex == 0)
                    {
                        _isSelectedItemFirst = true;
                    }
                    else if (currendIndex == _currentGameNotes.Notes.Count - 1)
                    {
                        _isSelectedItemLast = true;
                    }
                }
            }

            OnPropertyChanged(nameof(MoveNoteNextCommand));
            OnPropertyChanged(nameof(MoveNotePreviousCommand));
            OnPropertyChanged(nameof(RemoveCurrentItemCommand));
            OnPropertyChanged(nameof(ImportSteamGuideCommand));
        }

        private void AddItem()
        {
            var newItem = new PlayNote(ResourceProvider.GetString("PlayNotes_NewNoteLabel"), string.Empty);
            _currentGameNotes.Notes.Add(newItem);
            SelectedNotes = newItem;
            EditorVisibility = Visibility.Visible;
            UpdateSectionsVisibilityAndCanExecute();
        }

        private void RemoveCurrentItem()
        {
            if (_selectedNotes is null)
            {
                return;
            }

            if (_currentGameNotes.Notes.Remove(_selectedNotes))
            {
                UpdateSectionsVisibilityAndCanExecute();
            }
        }

        private void MoveNotePrevious()
        {
            if (_selectedNotes is null)
            {
                return;
            }

            var currentIndex = _currentGameNotes.Notes.IndexOf(_selectedNotes);
            if (currentIndex != -1 && currentIndex != 0)
            {
                _currentGameNotes.Notes.Move(currentIndex, currentIndex - 1);
                UpdateSectionsVisibilityAndCanExecute();
            }
        }

        private void MoveNoteNext()
        {
            if (_selectedNotes is null)
            {
                return;
            }

            var currentIndex = _currentGameNotes.Notes.IndexOf(_selectedNotes);
            if (currentIndex != -1 && currentIndex != _currentGameNotes.Notes.Count - 1)
            {
                _currentGameNotes.Notes.Move(currentIndex, currentIndex + 1);
                UpdateSectionsVisibilityAndCanExecute();
            }
        }

        private void SaveItem()
        {
            if (_currentGameNotes is null)
            {
                return;
            }

            if (_notesDatabase.Update(_currentGameNotes))
            {
                _playniteApi.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("PlayNotes_SavedNotesMessage"), currentGame.Name));
            }
        }

        private void ImportSteamGuide()
        {
            var progressOptions = new GlobalProgressOptions(ResourceProvider.GetString("PlayNotes_SteamGuideImporterImportingMessage"), true)
            {
                IsIndeterminate = true
            };

            var importSuccessful = false;
            _playniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    importSuccessful = SteamGuideImporter.ImportSteamGuide(CurrentGameNotes, a.CancelToken);
                });
            }, progressOptions);

            if (importSuccessful)
            {
                SelectedNotes = CurrentGameNotes.Notes.FirstOrDefault();
                UpdateSectionsVisibilityAndCanExecute();
            }
        }

        public RelayCommand SwitchMarkdownHeightCommand
        {
            get => new RelayCommand(() =>
            {
                ExpandNotes = !ExpandNotes;
            });
        }

        public RelayCommand SwitchEditorVisibilityCommand
        {
            get => new RelayCommand(() =>
            {
                EditorVisibility = EditorVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }, () => !(SelectedNotes is null));
        }

        public RelayCommand SwitchToolsVisibilityCommand
        {
            get => new RelayCommand(() =>
            {
                ToolsVisibility = ToolsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            });
        }

        public RelayCommand RemoveCurrentItemCommand
        {
            get => new RelayCommand(() =>
            {
                RemoveCurrentItem();
            }, () => !(SelectedNotes is null));
        }

        public RelayCommand AddItemCommand
        {
            get => new RelayCommand(() =>
            {
                AddItem();
            });
        }

        public RelayCommand MoveNotePreviousCommand
        {
            get => new RelayCommand(() =>
            {
                MoveNotePrevious();
            }, () => !_isSelectedItemFirst && !(SelectedNotes is null) && _multipleNotesAvailable);
        }

        public RelayCommand MoveNoteNextCommand
        {
            get => new RelayCommand(() =>
            {
                MoveNoteNext();
            }, () => !_isSelectedItemLast && !(SelectedNotes is null) && _multipleNotesAvailable);
        }

        public RelayCommand SaveItemCommand
        {
            get => new RelayCommand(() =>
            {
                SaveItem();
            });
        }

        public RelayCommand ImportSteamGuideCommand
        {
            get => new RelayCommand(() =>
            {
                ImportSteamGuide();
            }, () => !(CurrentGameNotes is null));
        }
    }
}