using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SteamScreenshots.Screenshots
{
    public class ScreenshotsViewModel : INotifyPropertyChanged
    {
        private int _currentIndex;
        private Uri _currentImageUri;
        private readonly Window _window;

        public ObservableCollection<Uri> ImageUris { get; set; }

        public Uri CurrentImageUri
        {
            get => _currentImageUri;
            set
            {
                _currentImageUri = value;
                OnPropertyChanged();
            }
        }

        public bool HasMultipleImages => ImageUris.Count > 1;
        public string ImagePositionLabel =>
            string.Format(ResourceProvider.GetString("LOC_SteamScreenshots_SelectedScreenshotPositionFormat"),
                _currentIndex + 1,
                ImageUris.Count);

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand CloseWindowCommand { get; }

        public ScreenshotsViewModel(Window window)
        {
            _window = window;
            ImageUris = new ObservableCollection<Uri>();
            NextCommand = new RelayCommand(NextImage, CanNavigate);
            BackCommand = new RelayCommand(PreviousImage, CanNavigate);
            CloseWindowCommand = new RelayCommand(CloseWindow);
            _currentIndex = -1;
            AddKeyBindings();
        }

        private void AddKeyBindings()
        {
            var leftKeyBinding = new KeyBinding
            {
                Key = Key.Left,
                Command = BackCommand
            };
            _window.InputBindings.Add(leftKeyBinding);

            var rightKeyBinding = new KeyBinding
            {
                Key = Key.Right,
                Command = NextCommand
            };
            _window.InputBindings.Add(rightKeyBinding);

            var escapeKeyBinding = new KeyBinding
            {
                Key = Key.Escape,
                Command = CloseWindowCommand
            };
            _window.InputBindings.Add(escapeKeyBinding);
        }

        public void LoadUris(IEnumerable<Uri> uris)
        {
            ImageUris.Clear();
            uris.ForEach(uri => ImageUris.Add(uri));
            if (ImageUris.Count > 0)
            {
                _currentIndex = 0;
                CurrentImageUri = ImageUris[_currentIndex];
                OnPropertyChanged(nameof(HasMultipleImages));
            }
        }

        public void SelectImage(Uri uri)
        {
            var imageIndex = ImageUris.IndexOf(uri);
            if (imageIndex != -1)
            {
                _currentIndex = imageIndex;
                CurrentImageUri = ImageUris[_currentIndex];
                OnPropertyChanged(nameof(ImagePositionLabel));
            }
        }

        private void NextImage()
        {
            if (_currentIndex < ImageUris.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0;
            }

            CurrentImageUri = ImageUris[_currentIndex];
            OnPropertyChanged(nameof(ImagePositionLabel));
        }

        private void PreviousImage()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
            else
            {
                _currentIndex = ImageUris.Count - 1;
            }

            CurrentImageUri = ImageUris[_currentIndex];
            OnPropertyChanged(nameof(ImagePositionLabel));
        }

        private void CloseWindow()
        {
            _window.Close();
        }

        private bool CanNavigate()
        {
            return ImageUris.Count > 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}