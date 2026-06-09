using Microsoft.Extensions.Logging;
using Playnite.SDK;
using Playnite.SDK.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Policy;
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
        private readonly string _blankHtmlBase64Address;
        private readonly object _browserInstance;
        private readonly Dispatcher _browserDispatcher;
        private readonly PropertyInfo _addressProperty;
        private readonly PropertyInfo _isLoadingProperty;
        private readonly MethodInfo _disposeMethod;
        private bool _lastIsLoading = false;
        private string _lastAddress = string.Empty;
        private int _currentAddressIndex = -1;
        private bool _disposed = false;
        private Func<object, bool> _isLoadingGetter;
        private Func<object, string> _addressChangedGetter;
        private readonly EventInfo _addressEvent;
        private readonly Delegate _addressHandler;
        private readonly EventInfo _loadingEvent;
        private readonly Delegate _loadingHandler;
        private readonly EventInfo _loadErrorEvent;
        private readonly Delegate _loadErrorHandler;

        public event EventHandler<AddressChangedEventArgs> AddressChanged;
        public event EventHandler<IsLoadingChangedEventArgs> IsLoadingChanged;

        public string Address => GetCurrentAddress();
        public bool IsLoading => _lastIsLoading;

        public CefSharpWebBrowserHost()
        {
            var cefSharpWpfAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "CefSharp.Wpf")
                    ?? throw new Exception("CefSharp assembly is not loaded in the current AppDomain.");
            var browserType = cefSharpWpfAssembly.GetType("CefSharp.Wpf.ChromiumWebBrowser")
                ?? throw new Exception("Unable to find ChromiumWebBrowser type in CefSharp assembly.");

            _addressProperty = browserType.GetProperty("Address", BindingFlags.Public | BindingFlags.Instance)
                ?? throw new Exception("Unable to find Address property in ChromiumWebBrowser type.");  

            _isLoadingProperty = browserType.GetProperty("IsLoading", BindingFlags.Public | BindingFlags.Instance)
                ?? throw new Exception("Unable to find IsLoading property in ChromiumWebBrowser type.");    

            _disposeMethod = browserType.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance)
                ?? throw new Exception("Unable to find Dispose method in ChromiumWebBrowser type.");

            _blankHtmlBase64Address = "data:text/html;base64," + "<html><body/></html>".Base64Encode();

            _browserInstance = Activator.CreateInstance(browserType);
            _browserDispatcher = ((DispatcherObject)_browserInstance).Dispatcher;

            _addressEvent = browserType.GetEvent("AddressChanged");
            _addressHandler = CreateReflectedEventHandler(
                _addressEvent,
                BrowserAddressChanged);

            _addressEvent.AddEventHandler(
                _browserInstance,
                _addressHandler);

            _loadingEvent = browserType.GetEvent("LoadingStateChanged");
            _loadingHandler = CreateReflectedEventHandler(
                _loadingEvent,
                BrowserLoadingStateChanged);

            _loadingEvent.AddEventHandler(
                _browserInstance,
                _loadingHandler);

            _loadErrorEvent = browserType.GetEvent("LoadError");
            _loadErrorHandler = CreateReflectedEventHandler(
                _loadErrorEvent,
                BrowserLoadError);

            _loadErrorEvent.AddEventHandler(
                _browserInstance,
                _loadErrorHandler);
        }

        private void BrowserLoadingStateChanged(object sender, object args)
        {
            if (_isLoadingGetter is null)
            {
                var property = args.GetType().GetProperty("IsLoading");
                _isLoadingGetter = obj => (bool)property.GetValue(obj);
            }

            var isLoading = _isLoadingGetter(args);
            if (isLoading == _lastIsLoading)
            {
                return;
            }

            var oldValue = _lastIsLoading;
            _lastIsLoading = isLoading;

            _browserDispatcher.BeginInvoke(
                new Action(() =>
                {
                    IsLoadingChanged?.Invoke(
                        this,
                        new IsLoadingChangedEventArgs(
                            oldValue,
                            isLoading));
                }));
        }

        private Delegate CreateReflectedEventHandler(
            EventInfo eventInfo,
            Action<object, object> callback)
        {
            var handlerType = eventInfo.EventHandlerType;
            var invokeMethod = handlerType.GetMethod("Invoke");

            var parameters = invokeMethod
                .GetParameters()
                .Select(p => System.Linq.Expressions.Expression.Parameter(p.ParameterType))
                .ToArray();

            var callbackTarget = System.Linq.Expressions.Expression.Constant(callback);

            var callbackCall = System.Linq.Expressions.Expression.Call(
                callbackTarget,
                callback.GetType().GetMethod(nameof(Action<object, object>.Invoke)),
                System.Linq.Expressions.Expression.Convert(parameters[0], typeof(object)),
                System.Linq.Expressions.Expression.Convert(parameters[1], typeof(object)));

            var lambda = System.Linq.Expressions.Expression.Lambda(
                handlerType,
                callbackCall,
                parameters);

            return lambda.Compile();
        }

        private void BrowserLoadError(
            object sender,
            object args)
        {
            var argsType = args.GetType();

            var failedUrl =
                argsType.GetProperty("FailedUrl")
                    ?.GetValue(args) as string;

            var errorCode =
                argsType.GetProperty("ErrorCode")
                    ?.GetValue(args);

            if (string.IsNullOrWhiteSpace(failedUrl))
            {
                return;
            }

            if (!Uri.TryCreate(failedUrl, UriKind.Absolute, out var uri))
            {
                return;
            }

            if (errorCode?.ToString() != "UnknownUrlScheme")
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(failedUrl)
                {
                    UseShellExecute = true
                });

                if (_currentAddressIndex >= 0 &&
                    _currentAddressIndex < _addressHistory.Count)
                {
                    var previousAddress = _addressHistory[_currentAddressIndex];

                    _browserDispatcher.Invoke(
                        new Action(() =>
                        {
                            _addressProperty.SetValue(
                                _browserInstance,
                                _blankHtmlBase64Address);
                            _addressProperty.SetValue(
                                _browserInstance,
                                previousAddress);
                        }));
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex, $"Failed to launch URI: {failedUrl}");
            }
        }

        private void BrowserAddressChanged(
            object sender,
            object args)
        {
            if (_addressChangedGetter is null)
            {
                var property = args.GetType().GetProperty("NewValue");
                _addressChangedGetter = obj => (string)property.GetValue(obj);
            }

            var currentAddress = _addressChangedGetter(args);
            ProcessAddressChange(currentAddress);
        }

        private void ProcessAddressChange(string currentAddress)
        {
            if (currentAddress == _lastAddress)
            {
                return;
            }

            if (currentAddress == _blankHtmlBase64Address)
            {
                var oldAddress = _lastAddress;
                _lastAddress = currentAddress;

                AddressChanged?.Invoke(
                    this,
                    new AddressChangedEventArgs(
                        oldAddress,
                        string.Empty));

                return;
            }

            var previousAddress = _lastAddress;
            _lastAddress = currentAddress;

            if (!IsLoading && !IsNavigatingInHistory(currentAddress))
            {
                AddToHistory(currentAddress);
            }

            AddressChanged?.Invoke(
                this,
                new AddressChangedEventArgs(
                    previousAddress,
                    currentAddress));
        }

        public Control GetWebViewControl() => _browserInstance as Control;

        public string GetCurrentAddress()
        {
            if (_browserInstance is null)
            {
                return string.Empty;
            }

            return _browserDispatcher.Invoke(() =>
            {
                 var currentValue = _addressProperty.GetValue(_browserInstance) as string;
                 if (currentValue == _blankHtmlBase64Address)
                 {
                     return string.Empty;
                 }

                 return currentValue;
             });
        }

        private string GetCurrentAddressInternal()
        {
            if (_browserInstance is null)
            {
                return string.Empty;
            }

            return _browserDispatcher.Invoke(
                () => _addressProperty.GetValue(_browserInstance) as string);
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

        public void Dispose()
        {
            if (!_disposed)
            {
                _addressEvent?.RemoveEventHandler(
                     _browserInstance,
                     _addressHandler);

                _loadingEvent?.RemoveEventHandler(
                    _browserInstance,
                    _loadingHandler);

                _loadErrorEvent?.RemoveEventHandler(
                    _browserInstance,
                    _loadErrorHandler);

                if (_browserInstance != null)
                {
                    _disposeMethod?.Invoke(_browserInstance, null);
                }

                _disposed = true;
            }
        }


    }


}