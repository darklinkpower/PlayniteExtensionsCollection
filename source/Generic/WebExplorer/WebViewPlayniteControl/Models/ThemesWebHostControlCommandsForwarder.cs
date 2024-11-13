using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebViewCore.Application;

namespace WebExplorer.WebViewPlayniteControl.Models
{
    public class ThemesWebHostControlCommandsForwarder
    {
        private readonly BrowserHostViewModel _browserHostViewModel;
        public RelayCommand OpenAddressExternallyCommand => _browserHostViewModel.OpenAddressExternallyCommand;
        public RelayCommand GoBackCommand => _browserHostViewModel.GoBackCommand;
        public RelayCommand GoForwardCommand => _browserHostViewModel.GoForwardCommand;
        public RelayCommand<string> NavigateToAddressCommand => _browserHostViewModel.NavigateToAddressCommand;
        public RelayCommand NavigateToCurrentAddressCommand => _browserHostViewModel.NavigateToCurrentAddressCommand;
        public RelayCommand ReloadCommand => _browserHostViewModel.ReloadCommand;

        public ThemesWebHostControlCommandsForwarder(BrowserHostViewModel browserHostViewModel)
        {
            _browserHostViewModel = browserHostViewModel;
        }
    }
}