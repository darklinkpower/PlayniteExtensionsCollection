using Playnite.SDK;
using SpecialKHelper.Core.Application;
using SpecialKHelper.Core.Domain;
using SpecialKHelper.SpecialKHandler.Domain.Enums;
using SpecialKHelper.SpecialKHandler.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKHandler.Application
{
    public class SpecialKSignalWatcher : IDisposable
    {
        private readonly NamedEventSignalWatcher _watcher;

        private readonly Dictionary<string, SignalType> _signalMap =
            new Dictionary<string, SignalType>
            {
                {
                    @"Local\SKIF_InjectAckEx",
                    SignalType.InjectionDetected
                },
                {
                    @"Local\SKIF_InjectExitAckEx",
                    SignalType.InjectionExited
                }
            };

        private readonly ILogger _logger;

        public event EventHandler<SignalReceivedEventArgs> SignalReceived;

        public SpecialKSignalWatcher(
            ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.Info("Creating signal watcher.");
            _watcher = new NamedEventSignalWatcher();

            foreach (var signal in _signalMap)
            {
                _logger.Debug(
                    $"Registering signal '{signal.Key}' => {signal.Value}");

                _watcher.Register(signal.Key);
            }

            _watcher.SignalReceived += OnSignalReceived;
            _logger.Debug("Signal subscriptions initialized.");
        }

        public void Start()
        {
            _logger.Info("Starting signal watcher.");
            _watcher.Start();
        }

        private void OnSignalReceived(
            object sender,
            string eventName)
        {
            _logger.Debug($"Received raw signal '{eventName}'.");

            if (!_signalMap.TryGetValue(
                eventName,
                out var signalType))
            {
                _logger.Warn($"Unknown signal received '{eventName}'.");
                return;
            }

            _logger.Debug($"Mapped signal '{eventName}' to '{signalType}'.");

            SignalReceived?.Invoke(
                this,
                new SignalReceivedEventArgs(
                    signalType));

            _logger.Debug(
                "Signal dispatched.");
        }

        public void Dispose()
        {
            _logger.Info("Disposing signal watcher.");
            _watcher.SignalReceived -= OnSignalReceived;
            _watcher.Dispose();
            _logger.Debug("Signal watcher disposed.");
        }
    }
}
