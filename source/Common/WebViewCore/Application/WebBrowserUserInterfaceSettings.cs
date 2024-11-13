using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WebViewCore.Application
{
    public class WebBrowserUserInterfaceSettings : INotifyPropertyChanged
    {
        private bool _isNavigationVisible = true;
        private bool _isBookmarksVisible = true;

        public bool IsNavigationVisible
        {
            get => _isNavigationVisible;
            set
            {
                if (_isNavigationVisible != value)
                {
                    _isNavigationVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsBookmarksVisible
        {
            get => _isBookmarksVisible;
            set
            {
                if (_isBookmarksVisible != value)
                {
                    _isBookmarksVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public void ShowAll()
        {
            IsNavigationVisible = true;
            IsBookmarksVisible = true;
        }

        public void HideAll()
        {
            IsNavigationVisible = false;
            IsBookmarksVisible = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}