using Playnite.SDK;
using PlayState.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlayState
{
    // Obtained from https://github.com/felixkmh/QuickSearch-for-Playnite
    public partial class PlayStateSettingsView : UserControl
    {
        public PlayStateSettingsView()
        {
            InitializeComponent();
        }

        private void SetHotkey(KeyEventArgs e, TextBox tbHotkey, Button setHotkeyButton, HotKey hotkeyGesture)
        {
            // Get modifiers and key data
            var modifiers = Keyboard.Modifiers;
            var key = e.Key;

            // When Alt is pressed, SystemKey is used instead
            if (key == Key.System)
            {
                key = e.SystemKey;
            }

            // Pressing delete, backspace or escape without modifiers clears the current value
            if (modifiers == ModifierKeys.None ||
                (key == Key.Delete || key == Key.Back || key == Key.Escape))
            {
                // Hotkey = null;
                return;
            }

            // If no actual key was pressed - return
            if (key == Key.LeftCtrl ||
                key == Key.RightCtrl ||
                key == Key.LeftAlt ||
                key == Key.RightAlt ||
                key == Key.LeftShift ||
                key == Key.RightShift ||
                key == Key.LWin ||
                key == Key.RWin ||
                key == Key.Clear ||
                key == Key.OemClear ||
                key == Key.Apps)
            {
                return;
            }

            tbHotkey.Text = $"{modifiers} + {key}";

            hotkeyGesture.Key = key;
            hotkeyGesture.Modifiers = modifiers;
            tbHotkey.Focusable = false;
            setHotkeyButton.Focus();
            setHotkeyButton.Content = ResourceProvider.GetString("LOCPlayState_SettingChangeHotkeyButtonLabel");
        }
    }
}