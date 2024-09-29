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
using System.Windows.Media.Animation;

namespace SteamScreenshots.Screenshots
{
    public class ScreenshotsViewModel : INotifyPropertyChanged
    {
        private enum ImageIdentifier
        {
            ImageA,
            ImageB
        }
        
        private int _currentIndex;
        private Uri _currentImageUri;
        private readonly Window _window;
        private ImageIdentifier _lastImageSet = ImageIdentifier.ImageB;
        private DoubleAnimation _fadeOutAnimation;
        private DoubleAnimation _fadeInAnimation;

        public ObservableCollection<Uri> ImageUris { get; set; }

        private Uri _oldImageUri;
        public Uri ImageUriA
        {
            get => _oldImageUri;
            set
            {
                _oldImageUri = value;
                OnPropertyChanged();
            }
        }

        public Uri ImageUriB
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
            InitializeAnimations();
        }

        public Uri GetLastDisplayedImageUri()
        {
            return _lastImageSet == ImageIdentifier.ImageA
                ? ImageUriA
                : ImageUriB;
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

        private void InitializeAnimations()
        {
            _fadeOutAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.25),
                FillBehavior = FillBehavior.HoldEnd
            };

            _fadeInAnimation = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.25),
                FillBehavior = FillBehavior.HoldEnd
            };
        }

        public void LoadUris(IEnumerable<Uri> uris)
        {
            ImageUris.Clear();
            uris.ForEach(uri => ImageUris.Add(uri));
            if (ImageUris.Count > 0)
            {
                _currentIndex = 0;
                SetNewImageUri(ImageUris[_currentIndex]);
                OnPropertyChanged(nameof(HasMultipleImages));
            }
        }

        public void SelectImage(Uri uri)
        {
            var imageIndex = ImageUris.IndexOf(uri);
            if (imageIndex != -1)
            {
                _currentIndex = imageIndex;
                SetNewImageUri(ImageUris[_currentIndex]);
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

            SetNewImageUri(ImageUris[_currentIndex]);
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

            SetNewImageUri(ImageUris[_currentIndex]);
            OnPropertyChanged(nameof(ImagePositionLabel));
        }

        private void SetNewImageUri(Uri uri)
        {
            if (_lastImageSet == ImageIdentifier.ImageA)
            {
                ImageUriB = uri;
                _lastImageSet = ImageIdentifier.ImageB;
            }
            else
            {
                ImageUriA = uri;
                _lastImageSet = ImageIdentifier.ImageA;
            }

            FadeImages();
        }

        private void FadeImages()
        {
            if (_window.Content is ScreenshotsView content)
            {
                if (_lastImageSet == ImageIdentifier.ImageA)
                {
                    Storyboard.SetTarget(_fadeInAnimation, content.ImageA);
                    Storyboard.SetTarget(_fadeOutAnimation, content.ImageB);
                }
                else
                {
                    Storyboard.SetTarget(_fadeOutAnimation, content.ImageA);
                    Storyboard.SetTarget(_fadeInAnimation, content.ImageB);
                }
                
                Storyboard.SetTargetProperty(_fadeOutAnimation, new PropertyPath("Opacity"));                
                Storyboard.SetTargetProperty(_fadeInAnimation, new PropertyPath("Opacity"));

                var storyboard = new Storyboard();
                storyboard.Children.Add(_fadeOutAnimation);
                storyboard.Children.Add(_fadeInAnimation);
                storyboard.Begin();
            }
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