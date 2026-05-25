using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using SpecialKHelper.SpecialKHandler.Application;
using SpecialKHelper.SpecialKHandler.Domain.Enums;
using SpecialKHelper.SpecialKHandler.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.Core.Application
{
    public class SpecialKGameSessionCoordinator
    {
        private readonly SpecialKServiceManager _serviceManager;
        private readonly ILogger _logger;
        private readonly IPlayniteAPI _playniteApi;
        private readonly Action _openSettingsViewAction;
        private readonly Dictionary<Guid, SpecialKLaunchSession> _sessions
            = new Dictionary<Guid, SpecialKLaunchSession>();

        public SpecialKGameSessionCoordinator(
            SpecialKServiceManager serviceManager,
            ILogger logger,
            IPlayniteAPI playniteApi,
            Action openSettingsViewAction)
        {
            _serviceManager = serviceManager
                ?? throw new ArgumentNullException(nameof(serviceManager));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _playniteApi = playniteApi
                ?? throw new ArgumentNullException(nameof(playniteApi));
            _openSettingsViewAction = openSettingsViewAction
                ?? throw new ArgumentNullException(nameof(openSettingsViewAction));
        }

        public SpecialKLaunchSession Start(Game game)
        {
            if (_sessions.TryGetValue(game.Id, out var existing))
            {
                _logger.Warn($"Game session already exists for '{game.Name}'.");
                return existing;
            }

            SpecialKLaunchSession session = null;
            var started32BitService = false;
            var Started64BitService = false;
            try
            {
                if (_serviceManager.Service32BitsStatus != SpecialKServiceStatus.Running)
                {
                    started32BitService = _serviceManager.Start32BitsService();

                }

                if (_serviceManager.Service64BitsStatus != SpecialKServiceStatus.Running)
                {
                    Started64BitService = _serviceManager.Start64BitsService();
                }

                session = new SpecialKLaunchSession
                (
                    game.Id,
                    started32BitService,
                    Started64BitService
                );

                _sessions[game.Id] = session;

                return session;
            }
            catch (SpecialKFileNotFoundException e)
            {
                LogSkFileNotFound(e);
            }
            catch (SpecialKPathNotFoundException e)
            {
                LogSkPathNotFound(e);
            }
            catch
            {
                if (session != null)
                {
                    CleanupPartialStartup(session);
                }

                throw;
            }

            return session;
        }

        public void Stop(Game game)
        {
            if (!_sessions.TryGetValue(game.Id, out var session))
            {
                _logger.Info($"No session found for '{game.Name}'.");
                return;
            }

            try
            {
                if (session.Started32BitService &&
                    _serviceManager.Service32BitsStatus == SpecialKServiceStatus.Running)
                {
                    _logger.Info("Stopping plugin-owned 32-bit service.");
                    _serviceManager.Stop32BitsService();
                }

                if (session.Started64BitService &&
                    _serviceManager.Service64BitsStatus == SpecialKServiceStatus.Running)
                {
                    _logger.Info("Stopping plugin-owned 64-bit service.");
                    _serviceManager.Stop64BitsService();
                }
            }
            catch (SpecialKFileNotFoundException e)
            {
                LogSkFileNotFound(e);
            }
            catch (SpecialKPathNotFoundException e)
            {
                LogSkPathNotFound(e);
            }
            finally
            {
                _sessions.Remove(game.Id);
            }
        }

        private void LogSkFileNotFound(SpecialKFileNotFoundException e)
        {
            _logger.Error(e, $"Special K file not found");
            _playniteApi.Notifications.Add(new NotificationMessage(
                Guid.NewGuid().ToString(),
                string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkFileNotFound"), e.Message),
                NotificationType.Error,
                () => ProcessStarter.StartUrl(@"https://github.com/darklinkpower/PlayniteExtensionsCollection/wiki/Special-K-Helper#file-not-found-notification-error")
            ));
        }

        private void LogSkPathNotFound(SpecialKPathNotFoundException e)
        {
            _logger.Error(e, "Special K Path registry key not found");
            _playniteApi.Notifications.Add(new NotificationMessage(
                "sk_registryNotFound",
                ResourceProvider.GetString("LOCSpecial_K_Helper_NotifcationErrorMessageSkRegistryKeyNotFound"),
                NotificationType.Error,
                () => _openSettingsViewAction?.Invoke()
            ));
        }

        private void CleanupPartialStartup(
            SpecialKLaunchSession session)
        {
            try
            {
                if (session.Started32BitService &&
                    _serviceManager.Service32BitsStatus ==
                    SpecialKServiceStatus.Running)
                {
                    _serviceManager.Stop32BitsService();
                }

                if (session.Started64BitService &&
                    _serviceManager.Service64BitsStatus ==
                    SpecialKServiceStatus.Running)
                {
                    _serviceManager.Stop64BitsService();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Failed cleanup after startup failure.");
            }
        }
    }
}
