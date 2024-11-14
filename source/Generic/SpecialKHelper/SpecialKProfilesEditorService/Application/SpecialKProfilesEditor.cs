using Playnite.SDK;
using SpecialKHelper.SpecialKHandler.Application;
using SpecialKHelper.SpecialKProfilesEditorService.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpecialKHelper.SpecialKProfilesEditorService.Application
{
    public class SpecialKProfilesEditor
    {
        private readonly SpecialKServiceManager _specialKServiceManager;
        private readonly IPlayniteAPI _playniteApi;

        public SpecialKProfilesEditor(SpecialKServiceManager specialKServiceManager, IPlayniteAPI playniteApi)
        {
            _specialKServiceManager = specialKServiceManager;
            _playniteApi = playniteApi;
        }

        public void OpenEditorWindow(string searchTerm = null)
        {
            string skifPath;
            try
            {
                skifPath = _specialKServiceManager.GetInstallDirectory();
            }
            catch (Exception e)
            {
                _playniteApi.Notifications.Add(new NotificationMessage(
                    "sk_registryNotFound",
                    ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkRegistryKeyNotFound"),
                    NotificationType.Error
                ));
                return;
            }

            var window = _playniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Height = 700;
            window.Width = 900;
            window.Title = ResourceProvider.GetString("LOCSpecial_K_Helper_WindowTitleSkProfileEditor");

            window.Content = new SpecialKProfileEditorView();
            window.DataContext = new SpecialKProfileEditorViewModel(_playniteApi, skifPath, searchTerm);
            window.Owner = _playniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.ShowDialog();
        }
    }
}