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
            TbHotkey.PreviewKeyDown += HotkeyTextBox_PreviewKeyDown;
            TbInformationHotkey.PreviewKeyDown += InformationHotkeyTextBox_PreviewKeyDown;
        }

        private void HotkeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is PlayStateSettingsViewModel settings)
            {
                e.Handled = true;
                SetHotkey(e, TbHotkey, SetHotkeyButton, settings.Settings.SavedHotkeyGesture);
            }
        }

        private void SetHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            TbHotkey.Focusable = true;
            TbHotkey.Focus();
            SetHotkeyButton.Content = ResourceProvider.GetString("LOCPlayState_SettingPressHotkeyPromtButtonLabel");
        }

        private void SetHotkey(KeyEventArgs e, TextBox tbHotkey, Button setHotkeyButton, Hotkey hotkeyGesture)
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

        private void InformationHotkeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is PlayStateSettingsViewModel settings)
            {
                e.Handled = true;
                SetInformationHotkey(e, TbInformationHotkey, SetInformationHotkeyButton, settings.Settings.SavedInformationHotkeyGesture);
            }
        }

        private void SetInformationHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            TbInformationHotkey.Focusable = true;
            TbInformationHotkey.Focus();
            SetInformationHotkeyButton.Content = ResourceProvider.GetString("LOCPlayState_SettingPressHotkeyPromtButtonLabel");
        }

        private void SetInformationHotkey(KeyEventArgs e, TextBox tbInformationHotkey, Button setInformationHotkeyButton, Hotkey informationHotkeyGesture)
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

            tbInformationHotkey.Text = $"{modifiers} + {key}";

            informationHotkeyGesture.Key = key;
            informationHotkeyGesture.Modifiers = modifiers;
            tbInformationHotkey.Focusable = false;
            setInformationHotkeyButton.Focus();
            setInformationHotkeyButton.Content = ResourceProvider.GetString("LOCPlayState_SettingChangeHotkeyButtonLabel");
        }
    }
}