using Playnite.SDK;
using System;
using System.Collections.Generic;

namespace InstallationStatusUpdater
{
    
    
    class DeviceListener : IDisposable
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private const int WM_DEVICECHANGE = 0x0219;                 // device change event      
        private const int DBT_DEVICEARRIVAL = 0x8000;               // system detected a new device      
        private const int DBT_DEVICEREMOVEPENDING = 0x8003;         // about to remove, still available      
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;        // device is gone    
        private static List<SetAction> setActions = new List<SetAction>();
        private static readonly InvisibleWindowForMessages window = new InvisibleWindowForMessages();
        
        public static void RegisterAction(Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            setActions.Add(new SetAction(action));
        }

        public void Dispose()
        {
            window.Dispose();
        }

        static DeviceListener()
        {
            window.InvokeAction += (s, e) =>
            {
                setActions.ForEach(x =>
                {
                    logger.Debug($"Action started from {e.EventName} event");
                    x.Action();
                });
            };
        }

        private class SetAction
        {
            public SetAction(Action action)
            {
                Action = action;
            }

            public Action Action { get; }
        }

        private class InvisibleWindowForMessages : System.Windows.Forms.NativeWindow, IDisposable
        {
            public InvisibleWindowForMessages()
            {
                CreateHandle(new System.Windows.Forms.CreateParams());
            }

            protected override void WndProc(ref System.Windows.Forms.Message m)
            {
                base.WndProc(ref m);

                switch (m.WParam.ToInt32())
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
                this.DestroyHandle();
            }
        }


    }
}
