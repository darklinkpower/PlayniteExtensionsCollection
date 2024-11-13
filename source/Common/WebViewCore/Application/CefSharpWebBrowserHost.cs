using Playnite.SDK;
using Playnite.SDK.Events;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WebViewCore.Domain.Events;

namespace WebViewCore.Application
{
    public class CefSharpWebBrowserHost : IDisposable
    {
        private readonly List<string> _addressHistory = new List<string>();
        private readonly DispatcherTimer _checkTimer;
        private readonly string _blankHtmlBase64Address;
        private readonly object _browserInstance;
        private readonly PropertyInfo _addressProperty;
        private readonly PropertyInfo _isLoadingProperty;
        private readonly MethodInfo _disposeMethod;
        private bool _lastIsLoading = false;
        private string _lastAddress = string.Empty;
        private int _currentAddressIndex = -1;
        private bool _disposed = false;

        public event EventHandler<AddressChangedEventArgs> AddressChanged;
        public event EventHandler<IsLoadingChangedEventArgs> IsLoadingChanged;

        public string Address => GetCurrentAddress();
        public bool IsLoading => _lastIsLoading;

        public CefSharpWebBrowserHost()
        {
            var cefSharpWpfAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "CefSharp.Wpf");
            if (cefSharpWpfAssembly == null)
            {
                throw new Exception("CefSharp assembly is not loaded in the current AppDomain.");
            }

            var browserType = cefSharpWpfAssembly.GetType("CefSharp.Wpf.ChromiumWebBrowser");
            if (browserType == null)
            {
                throw new Exception("Unable to find ChromiumWebBrowser type in CefSharp assembly.");
            }

            _addressProperty = browserType.GetProperty("Address", BindingFlags.Public | BindingFlags.Instance);
            if (_addressProperty == null)
            {
                throw new Exception("Unable to find Address property in ChromiumWebBrowser type.");
            }

            _isLoadingProperty = browserType.GetProperty("IsLoading", BindingFlags.Public | BindingFlags.Instance);
            if (_isLoadingProperty == null)
            {
                throw new Exception("Unable to find IsLoading property in ChromiumWebBrowser type.");
            }

            _disposeMethod = browserType.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance);
            if (_disposeMethod == null)
            {
                throw new Exception("Unable to find Dispose method in ChromiumWebBrowser type.");
            }

            _browserInstance = Activator.CreateInstance(browserType);
            _checkTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(450)
            };

            _blankHtmlBase64Address = "data:text/html;base64," + "<html><body/></html>".Base64Encode();
            _checkTimer.Tick += CheckTimer_Tick;
            _checkTimer.Start();
        }

        private void CheckTimer_Tick(object sender, EventArgs e)
        {
            CheckState();
        }

        public Control GetWebViewControl() => _browserInstance as Control;

        public string GetCurrentAddress()
        {
            if (_browserInstance is null)
            {
                return string.Empty;
            }

            var currentValue = _addressProperty.GetValue(_browserInstance) as string;
            if (currentValue == _blankHtmlBase64Address)
            {
                return string.Empty;
            }

            return currentValue;
        }

        private string GetCurrentAddressInternal()
        {
            if (_browserInstance is null)
            {
                return string.Empty;
            }

            return _addressProperty.GetValue(_browserInstance) as string;
        }

        public void NavigateTo(string url)
        {
            if (!IsNavigatingInHistory(url))
            {
                AddToHistory(url);
            }

            _addressProperty.SetValue(_browserInstance, url);
        }

        public void Reload()
        {
            var currentAddress = GetCurrentAddressInternal();
            NavigateTo(_blankHtmlBase64Address);
            NavigateTo(currentAddress);
        }

        public void NavigateToEmptyPage()
        {
            NavigateTo(_blankHtmlBase64Address);
        }

        public void GoBack()
        {
            if (_currentAddressIndex > 0)
            {
                _currentAddressIndex--;
                NavigateTo(_addressHistory[_currentAddressIndex]);
            }
        }

        public void GoForward()
        {
            if (_currentAddressIndex < _addressHistory.Count - 1)
            {
                _currentAddressIndex++;
                NavigateTo(_addressHistory[_currentAddressIndex]);
            }
        }

        public bool CanGoBack() => _currentAddressIndex > 0;
        public bool CanGoForward() => _currentAddressIndex < _addressHistory.Count - 1;

        public void ClearHistory()
        {
            _addressHistory.Clear();
            _currentAddressIndex = -1;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_browserInstance != null)
                {
                    _disposeMethod?.Invoke(_browserInstance, null);
                }

                _checkTimer.Tick -= CheckTimer_Tick;
                _checkTimer.Stop();
                _disposed = true;
            }
        }

        private void WebView_LoadingChanged(object sender, WebViewLoadingChangedEventArgs e)
        {
            var currentIsLoading = e.IsLoading;
            if (currentIsLoading != _lastIsLoading)
            {
                var oldIsLoading = _lastIsLoading;
                _lastIsLoading = currentIsLoading;

                IsLoadingChanged?.Invoke(this, new IsLoadingChangedEventArgs(oldIsLoading, currentIsLoading));
            }
        }

        private Control GetBrowserInstanceFromWebView(IWebView webView, Type browserType)
        {
            var window = webView.WindowHost;
            if (window.Content is Panel panel)
            {
                return FindBrowserInstance(panel, browserType);
            }
            else
            {
                return null;
            }
        }

        private Control FindBrowserInstance(Panel panel, Type browserType)
        {
            if (panel is null)
            {
                return null;
            }

            foreach (UIElement child in panel.Children)
            {
                if (child.GetType() == browserType && child is Control castedBrowserInstance)
                {
                    // Disconnect it from its previous parent
                    panel.Children.Remove(castedBrowserInstance);
                    return castedBrowserInstance;
                }

                if (child is Panel nestedPanel)
                {
                    var nestedResult = FindBrowserInstance(nestedPanel, browserType);
                    if (nestedResult != null)
                    {
                        return nestedResult;
                    }
                }
            }

            return null;
        }

        private void CheckState()
        {
            CheckAddress();
            CheckIsLoading();
        }

        public bool GetIsLoading()
        {
            if (_browserInstance is null)
            {
                return false;
            }

            var currentValue = _isLoadingProperty.GetValue(_browserInstance);
            return currentValue != null && Convert.ToBoolean(currentValue);
        }

        private void CheckIsLoading()
        {
            var currentIsLoading = GetIsLoading();
            if (currentIsLoading != _lastIsLoading)
            {
                var oldIsLoading = _lastIsLoading;
                _lastIsLoading = currentIsLoading;

                IsLoadingChanged?.Invoke(this, new IsLoadingChangedEventArgs(oldIsLoading, currentIsLoading));
            }
        }

        private void CheckAddress()
        {
            var currentAddress = GetCurrentAddressInternal();
            if (currentAddress != _lastAddress)
            {
                if (currentAddress == _blankHtmlBase64Address)
                {
                    var oldAddress = _lastAddress;
                    _lastAddress = currentAddress;
                    AddressChanged?.Invoke(this, new AddressChangedEventArgs(oldAddress, string.Empty));
                }
                else
                {
                    var oldAddress = _lastAddress;
                    _lastAddress = currentAddress;
                    if (currentAddress != _blankHtmlBase64Address && !IsNavigatingInHistory(currentAddress))
                    {
                        AddToHistory(currentAddress);
                    }

                    AddressChanged?.Invoke(this, new AddressChangedEventArgs(oldAddress, currentAddress));
                }
            }
        }

        private void AddToHistory(string newAddress)
        {
            if (_currentAddressIndex == -1 || newAddress != _addressHistory[_currentAddressIndex])
            {
                // Remove any "forward" addresses when a new address is visited
                if (_currentAddressIndex < _addressHistory.Count - 1)
                {
                    _addressHistory.RemoveRange(_currentAddressIndex + 1, _addressHistory.Count - (_currentAddressIndex + 1));
                }

                // Add new address and move the index forward
                _addressHistory.Add(newAddress);
                _currentAddressIndex = _addressHistory.Count - 1;
            }
        }

        private bool IsNavigatingInHistory(string address)
        {
            return _currentAddressIndex >= 0 && _currentAddressIndex < _addressHistory.Count
                   && _addressHistory[_currentAddressIndex] == address;
        }
    }


}