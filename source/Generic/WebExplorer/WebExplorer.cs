using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WebExplorer.WebViewPlayniteControl;
using WebViewCore.Application;
using WebViewCore.Infrastructure;

namespace WebExplorer
{
    public class WebExplorer : GenericPlugin
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private BrowserHostViewModel _sidebarViewModel;
        private readonly BookmarksIconRepository _BookmarksIconRepository;
        private readonly BookmarksManager _sidebarBookmarksManager;
        private readonly BookmarksManager _themesControlBookmarksManager;
        private readonly string _contextMenuLinksName;

        private WebExplorerSettingsViewModel settings { get; set; }
        private const string _pluginElementsSourceName = "WebExplorer";
        private const string _browserHostViewControlName = "BrowserHostViewControl";
        private const string _browserHostViewHiddenNavigationControlName = "BrowserHostOnlyBrowserVisibleControl";

        public override Guid Id { get; } = Guid.Parse("181ddd05-2168-4162-a116-b9c2a20c652c");

        public WebExplorer(IPlayniteAPI api) : base(api)
        {
            var cacheRootDirectory = Path.Combine(GetPluginUserDataPath(), "BrowserCache");
            var iconCacheDirectory = Path.Combine(cacheRootDirectory, "Icons");

            _BookmarksIconRepository = new BookmarksIconRepository(iconCacheDirectory);
            var sidebarBookmarksRepository = new FileBookmarksRepository("Sidebar", cacheRootDirectory);
            var themesBookmarksRepository = new FileBookmarksRepository("ThemesControl", cacheRootDirectory);

            _sidebarBookmarksManager = new BookmarksManager("Sidebar", sidebarBookmarksRepository, _BookmarksIconRepository);
            _themesControlBookmarksManager = new BookmarksManager("ThemesControl", themesBookmarksRepository, _BookmarksIconRepository);

            settings = new WebExplorerSettingsViewModel(this, _sidebarBookmarksManager, _themesControlBookmarksManager);

            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                SourceName = _pluginElementsSourceName,
                ElementList = new List<string> { _browserHostViewControlName, _browserHostViewHiddenNavigationControlName }
            });

            _contextMenuLinksName = $"{ResourceProvider.GetString("LOCLinksLabel")} (Playnite)";
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            try
            {
                if ((args.Name == _browserHostViewControlName || args.Name == _browserHostViewHiddenNavigationControlName)
                    && ShouldReturnHostControl(settings.Settings))
                {
                    var uiSettings = new WebBrowserUserInterfaceSettings();
                    if (args.Name == _browserHostViewHiddenNavigationControlName)
                    {
                        uiSettings.HideAll();
                    }

                    return new WebHostControl(
                        PlayniteApi,
                        uiSettings,
                        _BookmarksIconRepository,
                        _themesControlBookmarksManager,
                        () => OpenSettingsView());
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while returning ThemesWebHostControl");
            }

            return null;
        }

        private bool ShouldReturnHostControl(WebExplorerSettings settings)
        {
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen)
            {
                return settings.EnableSupportFullscreenMode;
            }

            if (PlayniteApi.MainView.ActiveDesktopView == DesktopView.Details)
            {
                return settings.EnableSupportDetailsView;
            }
            else if (PlayniteApi.MainView.ActiveDesktopView == DesktopView.Grid)
            {
                return settings.EnableSupportGridView;
            }

            return true;
        }

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            yield return new SidebarItem
            {
                Title = "Web View",
                Type = SiderbarItemType.View,
                Icon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"icon.png"),
                Opened = () => {
                    try
                    {
                        var cefSharpWebView = new CefSharpWebBrowserHost();
                        _sidebarViewModel = new BrowserHostViewModel(
                            cefSharpWebView,
                            _sidebarBookmarksManager,
                            new WebBrowserUserInterfaceSettings(),
                            () => OpenSettingsView());
                        return new BrowserHostView(_sidebarViewModel);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Failed to create sidebarView");
                        return null;
                    }
                },
                Closed = () => {
                    _sidebarViewModel?.Dispose();
                    _sidebarViewModel = null;
                }
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            if (!settings.Settings.ShowLinksInContextMenu || args.Games.Count != 1 || !args.Games[0].Links.HasItems())
            {
                return base.GetGameMenuItems(args);
            }

            return args.Games[0].Links
                .Where(x => IsValidLinkForBrowser(x))
                .Select(x => new GameMenuItem
            {
                Description = x.Name,
                MenuSection = _contextMenuLinksName,
                Action = (a) =>
                {
                    OpenLinkOnCef(x.Url);
                }
            });
        }

        private bool IsValidLinkForBrowser(Link link)
        {
            if (link.Url.IsNullOrEmpty())
            {
                return false;
            }

            if (link.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                || link.Url.StartsWith("www", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private void OpenLinkOnCef(string url)
        {
            using (var webView = PlayniteApi.WebViews.CreateView(1024, 700))
            {
                webView.Navigate(url);
                webView.OpenDialog();
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new WebExplorerSettingsView();
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (!settings.Settings.DefaultBookmarksInitialized)
            {
                settings.RestoreDefaultBookmarks();
                settings.Settings.DefaultBookmarksInitialized = true;
                SavePluginSettings(settings.Settings);
            }
        }
    }
}