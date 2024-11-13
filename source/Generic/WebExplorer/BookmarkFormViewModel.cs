using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebViewCore.Application;

namespace WebExplorer
{
    public class BookmarkFormViewModel : ObservableObject
    {
        private string _newBookmarkName = string.Empty;
        private string _newBookmarkAddress = string.Empty;
        private string _newBookmarkIconPath = string.Empty;
        private readonly BookmarksManager _manager;

        public string NewBookmarkName
        {
            get => _newBookmarkName;
            set
            {
                _newBookmarkName = value;
                OnPropertyChanged(nameof(NewBookmarkName));
            }
        }

        public string NewBookmarkAddress
        {
            get => _newBookmarkAddress;
            set
            {
                _newBookmarkAddress = value;
                OnPropertyChanged(nameof(NewBookmarkAddress));
            }
        }

        public string NewBookmarkIconPath
        {
            get => _newBookmarkIconPath;
            set
            {
                _newBookmarkIconPath = value;
                OnPropertyChanged(nameof(NewBookmarkIconPath));
            }
        }

        public ICommand AddBookmarkCommand { get; set; }
        public ICommand SelectIconCommand { get; set; }

        public BookmarkFormViewModel(BookmarksManager manager)
        {
            _manager = manager;
            AddBookmarkCommand = new RelayCommand(AddBookmark);
            SelectIconCommand = new RelayCommand(SelectIcon);
        }

        private void SelectIcon()
        {
            var selectedIcon = API.Instance.Dialogs.SelectIconFile();
            if (!string.IsNullOrEmpty(selectedIcon))
            {
                _newBookmarkIconPath = selectedIcon;
            }
        }

        private void AddBookmark()
        {
            if (string.IsNullOrWhiteSpace(_newBookmarkName) || string.IsNullOrWhiteSpace(_newBookmarkAddress))
            {
                return;
            }

            var addedBookmark = _manager.AddBookmark(_newBookmarkName, _newBookmarkAddress, _newBookmarkIconPath);
            if (addedBookmark != null)
            {
                ResetFields();
            }
        }

        private void ResetFields()
        {
            NewBookmarkName = string.Empty;
            NewBookmarkAddress = string.Empty;
            NewBookmarkIconPath = string.Empty;
        }
    }

}