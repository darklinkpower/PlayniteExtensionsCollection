using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InstallationStatusUpdater
{
    /// <summary>
    /// Interaction logic for DeviceListenerWindow.xaml
    /// </summary>
    public partial class DeviceListenerWindow : Window, IDisposable
    {
        private const int WM_DEVICECHANGE = 0x0219;                 // device change event
        private const int DBT_DEVICEARRIVAL = 0x8000;               // system detected a new device
        private const int DBT_DEVICEREMOVEPENDING = 0x8003;         // about to remove, still available
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;        // device is gone

        public DeviceListenerWindow()
        {
            InitializeComponent();

            //Hack to raise the OnSourceInitialize event
            Width = 0;
            Height = 0;
            WindowStyle = WindowStyle.None;
            Focusable = false;
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
            switch (wParam.ToInt32())
            {
                //case WM_DEVICECHANGE:
                //    InvokeAction(this, new InvokeActionEventArgs("WM_DEVICECHANGE"));
                //    break;
                case DBT_DEVICEARRIVAL:
                    InvokeAction(this, new InvokeActionEventArgs("DBT_DEVICEARRIVAL"));
                    break;
                //case DBT_DEVICEREMOVEPENDING:
                //    InvokeAction(this, new InvokeActionEventArgs("DBT_DEVICEREMOVEPENDING"));
                //    break;
                case DBT_DEVICEREMOVECOMPLETE:
                    InvokeAction(this, new InvokeActionEventArgs("DBT_DEVICEREMOVECOMPLETE"));
                    break;
                default:
                    break;
            }

            return IntPtr.Zero;
        }

        public class InvokeActionEventArgs : EventArgs
        {
            private string eventName;

            internal InvokeActionEventArgs(string eventName)
            {
                this.eventName = eventName;
            }

            public string EventName
            {
                get { return eventName; }
            }
        }

        public event EventHandler<InvokeActionEventArgs> InvokeAction;

        public void Dispose()
        {
            this.Close();
        }
    }
}