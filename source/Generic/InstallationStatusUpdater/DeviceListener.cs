using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;

namespace InstallationStatusUpdater
{
    class DeviceListener : IDisposable
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static List<SetAction> setActions = new List<SetAction>();
        private static readonly DeviceListenerWindow window = new DeviceListenerWindow();
        
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


    }
}
