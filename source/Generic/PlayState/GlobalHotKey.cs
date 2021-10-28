using PlayState.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace PlayState
{
    // From https://stackoverflow.com/a/65412682
    public class GlobalHotKey : IDisposable
    {
        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private static readonly ListenerWindow window = new ListenerWindow();
        private IntPtr windowHandle = new WindowInteropHelper(window).Handle;
        private static int currentID;
        private static uint MOD_NOREPEAT = 0x4000;
        private static List<HotKeyWithAction> registeredHotKeys = new List<HotKeyWithAction>();

        /// <summary>
        /// Registers a global hotkey
        /// </summary>
        /// <param name="hotkey">Hotkey that contains Modifiers and key</param>
        /// <param name="action">Action to be called when hotkey is pressed</param>
        /// <returns>true, if registration succeeded, otherwise false</returns>
        public static bool RegisterHotKey(Hotkey hotkey, Action action)
        {
            return RegisterHotKey(hotkey.Modifiers, hotkey.Key, action);
        }

        public static bool RegisterHotKey(ModifierKeys aModifier, Key aKey, Action action)
        {
            if (aModifier == ModifierKeys.None)
            {
                throw new ArgumentException("Modifier must not be ModifierKeys.None");
            }
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var aVirtualKeyCode = KeyInterop.VirtualKeyFromKey(aKey);
            currentID = currentID + 1;
            bool aRegistered = RegisterHotKey(new WindowInteropHelper(window).Handle, currentID,
                                        (uint)aModifier | MOD_NOREPEAT,
                                        (uint)aVirtualKeyCode);

            if (aRegistered)
            {
                registeredHotKeys.Add(new HotKeyWithAction(aModifier, aKey, action));
            }
            return aRegistered;
        }

        public void Dispose()
        {
            // unregister all the registered hot keys.
            for (int i = currentID; i > 0; i--)
            {
                UnregisterHotKey(windowHandle, i);
            }

            // dispose the inner native window.
            window.Dispose();
        }

        static GlobalHotKey()
        {
            window.KeyPressed += (s, e) =>
            {
                registeredHotKeys.ForEach(x =>
                {
                    if (e.Modifier == x.Modifier && e.Key == x.Key)
                    {
                        x.Action();
                    }
                });
            };
        }



        private class HotKeyWithAction
        {

            public HotKeyWithAction(ModifierKeys modifier, Key key, Action action)
            {
                Modifier = modifier;
                Key = key;
                Action = action;
            }

            public ModifierKeys Modifier { get; }
            public Key Key { get; }
            public Action Action { get; }
        }

    }
}
