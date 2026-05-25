using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using SpecialKHelper.Core.Domain;
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
        private readonly SpecialKSignalWatcher _specialKSignalWatcher;
        private readonly ILogger _logger;
        private readonly IPlayniteAPI _playniteApi;
        private readonly Action _openSettingsViewAction;

        private readonly Func<SpecialKServiceStopMode> _getServiceStopMode;

        private readonly Dictionary<Guid, SpecialKLaunchSession> _sessions
            = new Dictionary<Guid, SpecialKLaunchSession>();

        public SpecialKGameSessionCoordinator(
            SpecialKServiceManager serviceManager,
            SpecialKSignalWatcher specialKSignalWatcher,
            ILogger logger,
            IPlayniteAPI playniteApi,
            Action openSettingsViewAction,
            Func<SpecialKServiceStopMode> getServiceStopMode)
        {
            _serviceManager = serviceManager ??
                throw new ArgumentNullException(nameof(serviceManager));
            _specialKSignalWatcher = specialKSignalWatcher ??
                throw new ArgumentNullException(nameof(specialKSignalWatcher));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _playniteApi = playniteApi ??
                throw new ArgumentNullException(nameof(playniteApi));
            _openSettingsViewAction = openSettingsViewAction ??
                throw new ArgumentNullException(nameof(openSettingsViewAction));
            _getServiceStopMode = getServiceStopMode ??
                throw new ArgumentNullException(nameof(getServiceStopMode));

            _specialKSignalWatcher.SignalReceived +=
                OnSpecialKSignalReceived;
        }

        private void OnSpecialKSignalReceived(
            object sender,
            SignalReceivedEventArgs e)
        {
            _logger.Info($"{e.SignalType} detected.");

            switch (e.SignalType)
            {
                case SignalType.InjectionDetected:

                    if (_getServiceStopMode() ==
                        SpecialKServiceStopMode.OnInjection)
                    {
                        HandleInjectionDetected();
                    }

                    break;

                case SignalType.InjectionExited:

                    _logger.Info("Injected game exited.");
                    break;
            }
        }

        private void HandleInjectionDetected()
        {
            var waitingSessions =
                _sessions.Values
                .Where(x =>
                    x.State ==
                    SpecialKLaunchState.WaitingForInjection)
                .OrderBy(x => x.CreatedUtc)
                .ToList();

            if (!waitingSessions.Any())
            {
                _logger.Warn(
                    "Injection signal received but no waiting sessions found.");

                return;
            }

            var session = waitingSessions.First();
            session.MarkInjected();

            _logger.Info( $"Session '{session.GameId}' marked injected.");

            var remainingWaiting =
                _sessions.Values.Any(
                    x => x.State ==
                    SpecialKLaunchState.WaitingForInjection);

            if (remainingWaiting)
            {
                _logger.Info(
                    "Other sessions still waiting for injection. Keeping services running.");

                return;
            }

            _logger.Info("No waiting sessions remain.");

            StopAllOwnedSessions();
        }

        public SpecialKLaunchSession Start(
            Game game)
        {
            if (_sessions.TryGetValue(
                game.Id,
                out var existing))
            {
                _logger.Warn(
                    $"Session already exists for '{game.Name}'.");

                return existing;
            }

            SpecialKLaunchSession session = null;

            var started32 = false;
            var started64 = false;

            try
            {
                if (_serviceManager.Service32BitsStatus
                    != SpecialKServiceStatus.Running)
                {
                    started32 =
                        _serviceManager.Start32BitsService();

                    _logger.Info(
                        "Started 32-bit service.");
                }

                if (_serviceManager.Service64BitsStatus
                    != SpecialKServiceStatus.Running)
                {
                    started64 =
                        _serviceManager.Start64BitsService();

                    _logger.Info(
                        "Started 64-bit service.");
                }

                session =
                    new SpecialKLaunchSession(
                        game.Id,
                        started32,
                        started64);

                _sessions[game.Id] =
                    session;

                _logger.Info(
                    $"Created session for '{game.Name}'.");

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
                    CleanupPartialStartup(
                        session);
                }

                throw;
            }

            return null;
        }

        public void Stop(
            Game game)
        {
            RemoveSession(
                game.Id,
                game.Name,
                true);
        }

        public void RemoveSession(
            Guid gameId,
            string gameName = null,
            bool applyStopPolicy = false)
        {
            if (!_sessions.TryGetValue(
                gameId,
                out var session))
            {
                _logger.Info($"No session found for '{gameName ?? gameId.ToString()}'.");

                return;
            }

            try
            {
                session.MarkStopped();

                if (applyStopPolicy &&
                    _getServiceStopMode() ==
                    SpecialKServiceStopMode.OnGameStop)
                {
                    StopSession(
                        session);
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
                _sessions.Remove(
                    gameId);

                _logger.Info(
                    $"Removed session '{gameName ?? gameId.ToString()}'.");
            }
        }

        public void RemoveInvalidSessions(
            IEnumerable<Guid> validGameIds)
        {
            var valid = new HashSet<Guid>(validGameIds);

            var invalidSessions =
                _sessions.Keys
                .Where(x => !valid.Contains(x))
                .ToList();

            foreach (var gameId in invalidSessions)
            {
                RemoveSession(gameId);
            }
        }

        private void StopAllOwnedSessions()
        {
            _logger.Info("Stopping all plugin-owned sessions.");
            foreach (var session in _sessions.Values.ToList())
            {
                try
                {
                    StopSession(session);
                    session.MarkStopped();
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        ex,
                        "Failed stopping session.");
                }
            }

            _sessions.Clear();
        }

        private void StopSession(
            SpecialKLaunchSession session)
        {
            if (session.Started32BitService &&
                _serviceManager.Service32BitsStatus ==
                SpecialKServiceStatus.Running)
            {
                _logger.Info("Stopping plugin-owned 32-bit service.");
                _serviceManager.Stop32BitsService();
            }

            if (session.Started64BitService &&
                _serviceManager.Service64BitsStatus ==
                SpecialKServiceStatus.Running)
            {
                _logger.Info("Stopping plugin-owned 64-bit service.");
                _serviceManager.Stop64BitsService();
            }
        }

        private void CleanupPartialStartup(
            SpecialKLaunchSession session)
        {
            try
            {
                StopSession(session);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Failed cleanup after startup failure.");
            }
        }

        private void LogSkFileNotFound(
            SpecialKFileNotFoundException e)
        {
            _logger.Error(
                e,
                "Special K file not found");

            _playniteApi.Notifications.Add(
                new NotificationMessage(
                    Guid.NewGuid().ToString(),
                    string.Format(
                        ResourceProvider.GetString(
                            "LOCSpecial_K_Helper_NotifcationErrorMessageSkFileNotFound"),
                        e.Message),
                    NotificationType.Error,
                    () => ProcessStarter.StartUrl(
                        @"https://github.com/darklinkpower/PlayniteExtensionsCollection/wiki/Special-K-Helper#file-not-found-notification-error")));
        }

        private void LogSkPathNotFound(
            SpecialKPathNotFoundException e)
        {
            _logger.Error(
                e,
                "Special K Path registry key not found");

            _playniteApi.Notifications.Add(
                new NotificationMessage(
                    "sk_registryNotFound",
                    ResourceProvider.GetString(
                        "LOCSpecial_K_Helper_NotifcationErrorMessageSkRegistryKeyNotFound"),
                    NotificationType.Error,
                    () => _openSettingsViewAction()));
        }
    }
}
