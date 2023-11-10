using GameRelations.Interfaces;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRelations.Models
{
    public class GameRelationsControlSettings : ObservableObject, IGameRelationsControlSettings
    {
        private bool displayGameNames = true;
        public bool DisplayGameNames { get => displayGameNames; set => SetValue(ref displayGameNames, value); }

        private int maxItems = 20;
        public int MaxItems { get => maxItems; set => SetValue(ref maxItems, value); }

        private bool isEnabled = true;
        public bool IsEnabled { get => isEnabled; set => SetValue(ref isEnabled, value); }

        private bool displayOnlyInstalled = false;
        public bool DisplayOnlyInstalled { get => displayOnlyInstalled; set => SetValue(ref displayOnlyInstalled, value); }

        private bool isVisible = true;
        [DontSerialize]
        public bool IsVisible { get => isVisible; set => SetValue(ref isVisible, value); }

        public GameRelationsControlSettings()
        {

        }

        public GameRelationsControlSettings(bool displayGameNames, int maxItems, bool isEnabled)
        {
            DisplayGameNames = displayGameNames;
            MaxItems = maxItems;
            IsEnabled = isEnabled;
        }
    }
}