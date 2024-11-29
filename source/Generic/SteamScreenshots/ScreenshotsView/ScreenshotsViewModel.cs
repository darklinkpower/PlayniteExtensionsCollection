using Playnite.SDK;
using PluginsCommon.Converters;
using SteamScreenshots.Application.Services;
using SteamScreenshots.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

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
        private BitmapImage _currentBitmapImage;
        private readonly Window _window;
        private ImageIdentifier _lastImageSet = ImageIdentifier.ImageB;
        private DoubleAnimation _fadeOutAnimation;
        private DoubleAnimation _fadeInAnimation;
        private BitmapImage _bitmapImageA;

        public BitmapImage BitmapImageA
        {
            get => _bitmapImageA;
            set
            {
                _bitmapImageA = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage BitmapImageB
        {
            get => _currentBitmapImage;
            set
            {
                _currentBitmapImage = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Screenshot> Screenshots { get; } = new ObservableCollection<Screenshot>();
        public bool HasMultipleImages => Screenshots.Count > 1;

        public string ImagePositionLabel =>
            string.Format(ResourceProvider.GetString("LOC_SteamScreenshots_SelectedScreenshotPositionFormat"),
                _currentIndex + 1,
                Screenshots.Count);

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand CloseWindowCommand { get; }


        public Screenshot LastDisplayedScreenshot = null;

        public ScreenshotsViewModel(Window window, List<Screenshot> screenshots)
        {
            _window = window;
            Screenshots = screenshots.ToObservable();
            NextCommand = new RelayCommand(NextImage, CanNavigate);
            BackCommand = new RelayCommand(PreviousImage, CanNavigate);
            CloseWindowCommand = new RelayCommand(CloseWindow);
            _currentIndex = -1;

            AddKeyBindings();
            InitializeAnimations();
        }

        public void LoadScreenshots(IEnumerable<Screenshot> screenshots)
        {
            Screenshots.Clear();
            screenshots.ForEach(screenshot => Screenshots.Add(screenshot));
            if (Screenshots.Count > 0)
            {
                _currentIndex = 0;
                SetNewScreenshot(Screenshots[_currentIndex]);
                OnPropertyChanged(nameof(HasMultipleImages));
            }
        }

        public void SelectScreenshot(Screenshot screenshot)
        {
            var imageIndex = Screenshots.IndexOf(screenshot);
            if (imageIndex != -1)
            {
                _currentIndex = imageIndex;
                SetNewScreenshot(Screenshots[_currentIndex]);
                OnPropertyChanged(nameof(ImagePositionLabel));
            }
        }

        private void NextImage()
        {
            if (_currentIndex < Screenshots.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0;
            }

            SetNewScreenshot(Screenshots[_currentIndex]);
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
                _currentIndex = Screenshots.Count - 1;
            }

            SetNewScreenshot(Screenshots[_currentIndex]);
            OnPropertyChanged(nameof(ImagePositionLabel));
        }

        private void SetNewScreenshot(Screenshot screenshot)
        {
            if (_lastImageSet == ImageIdentifier.ImageA)
            {
                BitmapImageB = screenshot.FullImage;
                _lastImageSet = ImageIdentifier.ImageB;
            }
            else
            {
                BitmapImageA = screenshot.FullImage;
                _lastImageSet = ImageIdentifier.ImageA;
            }

            LastDisplayedScreenshot = screenshot;
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
            return Screenshots.Count > 1;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}