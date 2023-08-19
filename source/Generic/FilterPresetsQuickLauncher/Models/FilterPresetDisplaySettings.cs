using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterPresetsQuickLauncher.Models
{
    public class FilterPresetDisplaySettings : ObservableObject
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public Guid Id { get; set; }
        public bool ShowInTopPanel { get; set; } = false;
        public bool ShowInSidebar { get; set; } = false;
        public string Image { get; set; }
        private string imageFullPath = null;
        [DontSerialize]
        public string ImageFullPath { get => imageFullPath; set => SetValue(ref imageFullPath, value); }
    }
}