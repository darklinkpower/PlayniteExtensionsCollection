using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebViewCore.Application;
using WebViewCore.Domain.Events;

namespace WebExplorer.WebViewPlayniteControl.Models
{
    public class ThemesWebHostControlInformationForwarder : ObservableObject, IDisposable
    {
        private readonly BrowserHostViewModel _browserHostViewModel;
        private readonly CefSharpWebBrowserHost _cefSharpWebBrowserHost;
        private bool _disposed = false;
        private string _address;
        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set => SetValue(ref _isLoading, value); }

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                _browserHostViewModel.Address = value;
                OnPropertyChanged();
            }
        }

        public ThemesWebHostControlInformationForwarder(BrowserHostViewModel browserHostViewModel, CefSharpWebBrowserHost cefSharpWebBrowserHost)
        {
            _browserHostViewModel = browserHostViewModel;
            _cefSharpWebBrowserHost = cefSharpWebBrowserHost;
            UpdateProperties();
            _cefSharpWebBrowserHost.IsLoadingChanged += CefSharpWebBrowserHost_IsLoadingChanged;
            _cefSharpWebBrowserHost.AddressChanged += CefSharpWebBrowserHost_AddressChanged;
        }

        private void CefSharpWebBrowserHost_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            UpdateProperties();
        }

        private void CefSharpWebBrowserHost_IsLoadingChanged(object sender, IsLoadingChangedEventArgs e)
        {
            UpdateProperties();
        }

        private void UpdateProperties()
        {
            Address = _cefSharpWebBrowserHost.Address;
            IsLoading = _cefSharpWebBrowserHost.IsLoading;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cefSharpWebBrowserHost.IsLoadingChanged -= CefSharpWebBrowserHost_IsLoadingChanged;
                _cefSharpWebBrowserHost.AddressChanged -= CefSharpWebBrowserHost_AddressChanged;
                _disposed = true;
            }
        }
    }
}