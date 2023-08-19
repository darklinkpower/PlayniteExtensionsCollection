using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FilterPresetsQuickLauncher
{
    public static class WindowHelper
    {
        private static Window mainWindow = Application.Current.MainWindow;

        public static void BringMainWindowToForeground()
        {
            var currentWindowState = mainWindow.WindowState;
            if (currentWindowState == WindowState.Minimized)
            {
                //Hack to restore window to foreground when minimized https://stackoverflow.com/a/11941579
                mainWindow.WindowState = WindowState.Minimized;
                mainWindow.Show();
                if (currentWindowState == WindowState.Normal)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }
                else
                {
                    mainWindow.WindowState = WindowState.Maximized;
                }
            }
            else
            {
                mainWindow.Activate();
            }

            mainWindow.Focus();
        }
    }
}
