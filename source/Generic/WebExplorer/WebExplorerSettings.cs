using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebExplorer
{
    public class WebExplorerSettings : ObservableObject
    {
        private bool _defaultBookmarksInitialized = false;
        private bool _showLinksInContextMenu = true;
        private bool _enableSupportDetailsView = true;
        private bool _enableSupportGridView = true;
        private bool _enableSupportFullscreenMode = true;

        public bool DefaultBookmarksInitialized { get => _defaultBookmarksInitialized; set => SetValue(ref _defaultBookmarksInitialized, value); }
        public bool ShowLinksInContextMenu { get => _showLinksInContextMenu; set => SetValue(ref _showLinksInContextMenu, value); }
        public bool EnableSupportDetailsView { get => _enableSupportDetailsView; set => SetValue(ref _enableSupportDetailsView, value); }
        public bool EnableSupportGridView { get => _enableSupportGridView; set => SetValue(ref _enableSupportGridView, value); }
        public bool EnableSupportFullscreenMode { get => _enableSupportFullscreenMode; set => SetValue(ref _enableSupportFullscreenMode, value); }
    }
}