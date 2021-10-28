using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace PlayState
{
    /// <summary>
    /// Interaction logic for ListenerWindow.xaml
    /// </summary>
    public partial class ListenerWindow : Window, IDisposable
    {
        private static int WM_HOTKEY = 0x0312;
        public ListenerWindow()
        {
            InitializeComponent();

            //Hack to raise the OnSourceInitialize event
            Width = 0;
            Height = 0;
            WindowStyle = WindowStyle.None;
            ShowInTaskbar = false;
            ShowActivated = false;
            Show();
            Hide();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                var aWPFKey = KeyInterop.KeyFromVirtualKey(((int)lParam >> 16) & 0xFFFF);
                ModifierKeys modifier = (ModifierKeys)((int)lParam & 0xFFFF);
                if (KeyPressed != null)
                {
                    KeyPressed(this, new HotKeyPressedEventArgs(modifier, aWPFKey));
                }
            }

            return IntPtr.Zero;
        }

        public class HotKeyPressedEventArgs : EventArgs
        {
            private ModifierKeys _modifier;
            private Key _key;

            internal HotKeyPressedEventArgs(ModifierKeys modifier, Key key)
            {
                _modifier = modifier;
                _key = key;
            }

            public ModifierKeys Modifier
            {
                get { return _modifier; }
            }

            public Key Key
            {
                get { return _key; }
            }
        }

        public event EventHandler<HotKeyPressedEventArgs> KeyPressed;

        #region IDisposable Members

        public void Dispose()
        {
            this.Close();
        }

        #endregion

    }
}
