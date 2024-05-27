using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VndbApiDomain.ImageAggregate;

namespace VNDBNexus.Screenshots
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class ScreenshotsViewModel : INotifyPropertyChanged
    {
        private int _currentIndex;
        private Uri _currentImageUri;

        public ObservableCollection<VndbImage> Images { get; set; }

        public Uri CurrentImageUri
        {
            get => _currentImageUri;
            set
            {
                _currentImageUri = value;
                OnPropertyChanged();
            }
        }

        public bool HasMultipleImages => Images.Count > 1;
        public string ImagePosition => string.Format(ResourceProvider.GetString("LOC_VndbNexus_SelectedScreenshotPositionFormat"), _currentIndex + 1, Images.Count);

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }

        public ScreenshotsViewModel()
        {
            Images = new ObservableCollection<VndbImage>();
            NextCommand = new RelayCommand(NextImage, CanNavigate);
            BackCommand = new RelayCommand(PreviousImage, CanNavigate);
            _currentIndex = -1;
        }

        public void LoadImages(IEnumerable<VndbImage> vndbImages)
        {
            Images = new ObservableCollection<VndbImage>(vndbImages);
            if (Images.Count > 0)
            {
                _currentIndex = 0;
                CurrentImageUri = Images[_currentIndex].Url;
                OnPropertyChanged(nameof(HasMultipleImages));
            }
        }

        public void SelectImage(VndbImage vndbImage)
        {
            var imageIndex = Images.IndexOf(vndbImage);
            if (imageIndex != -1)
            {
                _currentIndex = imageIndex;
                CurrentImageUri = vndbImage.Url;
                OnPropertyChanged(nameof(ImagePosition));
            }
        }

        private void NextImage()
        {
            if (_currentIndex < Images.Count - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0;
            }

            CurrentImageUri = Images[_currentIndex].Url;
            OnPropertyChanged(nameof(ImagePosition));
        }

        private void PreviousImage()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
            else
            {
                _currentIndex = Images.Count - 1;
            }

            CurrentImageUri = Images[_currentIndex].Url;
            OnPropertyChanged(nameof(ImagePosition));
        }

        private bool CanNavigate()
        {
            return Images.Count > 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}