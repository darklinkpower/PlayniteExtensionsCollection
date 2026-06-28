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
        private bool _started32Service = false;
        private bool _started64Service = false;

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
                StopServicesIfNotNeeded();
                return;
            }

            var session = waitingSessions.First();
            session.MarkInjected();

            _logger.Info( $"Session '{session.GameId}' marked injected.");
            StopServicesIfNotNeeded();
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

            try
            {
                if (_serviceManager.Service32BitsStatus
                    != SpecialKServiceStatus.Running)
                {
                    _started32Service =
                        _serviceManager.Start32BitsService();

                    _logger.Info(
                        "Started 32-bit service.");
                }

                if (_serviceManager.Service64BitsStatus
                    != SpecialKServiceStatus.Running)
                {
                    _started64Service =
                        _serviceManager.Start64BitsService();

                    _logger.Info(
                        "Started 64-bit service.");
                }

                var session = new SpecialKLaunchSession(
                    game.Id,
                    _started32Service,
                    _started64Service);

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
                StopServicesIfNotNeeded();
                throw;
            }

            return null;
        }

        public void RemoveSession(Game game)
        {
            RemoveSessionInternal(
                game.Id,
                game.Name);
        }

        private void RemoveSessionInternal(
            Guid gameId,
            string gameName = null)
        {
            if (!_sessions.TryGetValue(
                gameId,
                out var session))
            {
                _logger.Info($"No session found for '{gameName} - {gameId}'.");
                return;
            }

            session.MarkStopped();
            _sessions.Remove(gameId);
            _logger.Info($"Removed session '{gameName} - {gameId}'");
            StopServicesIfNotNeeded();
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
                RemoveSessionInternal(gameId);
            }
        }

        private void StopAllOwnedSessions()
        {
            _logger.Info("Stopping all plugin-owned sessions.");
            foreach (var session in _sessions.Values.ToList())
            {
                try
                {
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
            StopServicesIfNotNeeded();
        }

        private void StopServicesIfNotNeeded()
        {
            try
            {
                var stopMode = _getServiceStopMode();
                var shouldStopServices = false;
                if (stopMode == SpecialKServiceStopMode.OnGameStop)
                {
                    // Stop services if no sessions remain
                    shouldStopServices = !_sessions.Values.Any();
                }
                else if (stopMode == SpecialKServiceStopMode.OnInjection)
                {
                    // Stop services if no sessions remain waiting for injection
                    var anyRemainingWaiting =
                        _sessions.Values.Any(
                            x => x.State ==
                            SpecialKLaunchState.WaitingForInjection);
                    if (!anyRemainingWaiting)
                    {
                        _logger.Info("No waiting sessions remain.");
                        shouldStopServices = true;
                    }
                }

                if (shouldStopServices)
                {
                    if (_started32Service &&
                        _serviceManager.Service32BitsStatus ==
                        SpecialKServiceStatus.Running)
                    {
                        _logger.Info("Stopping plugin-owned 32-bit service.");
                        var success = _serviceManager.Stop32BitsService();
                        _started32Service = !success;
                    }

                    if (_started64Service &&
                        _serviceManager.Service64BitsStatus ==
                        SpecialKServiceStatus.Running)
                    {
                        _logger.Info("Stopping plugin-owned 64-bit service.");
                        var success = _serviceManager.Stop64BitsService();
                        _started64Service = !success;
                    }
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
