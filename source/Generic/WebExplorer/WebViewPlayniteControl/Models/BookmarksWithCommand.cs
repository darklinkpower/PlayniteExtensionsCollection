using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebViewCore.Application;
using WebViewCore.Domain.Entities;

namespace WebExplorer.WebViewPlayniteControl.Models
{
    public class BookmarksWithCommand
    {
        public string Name { get; }
        public string Address { get; }
        public string IconPath { get; }
        public RelayCommand NavigateToAddressCommand { get; }

        public BookmarksWithCommand(Bookmark bookmark, BrowserHostViewModel browserHostViewModel)
        {
            Name = bookmark.Name;
            Address = bookmark.Address;
            IconPath = bookmark.IconPath;
            NavigateToAddressCommand = new RelayCommand(() => browserHostViewModel.NavigateToAddress(bookmark.Address));
        }
    }
}