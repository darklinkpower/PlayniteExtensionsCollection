using JastUsaLibrary.Features.InstallationHandler.Domain;
using JastUsaLibrary.Features.InstallationHandler.Infrastructure;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.InstallationHandler.Application
{

    public class InstallerService
    {
        private readonly List<IInstallerHandler> _handlers;
        private readonly IInstallerDetector _detector;
        private readonly IExecutableMetadataReader _executableMetadataReader;
        private readonly ILogger _logger;

        public InstallerService(
            IEnumerable<IInstallerHandler> handlers,
            IInstallerDetector detector,
            IExecutableMetadataReader executableMetadataReader,
            Playnite.SDK.ILogger logger)
        {
            _handlers = handlers?.ToList() ?? throw new ArgumentNullException(nameof(handlers));
            _detector = detector ?? throw new ArgumentNullException(nameof(detector));
            _executableMetadataReader = executableMetadataReader ?? throw new ArgumentNullException(nameof(_detector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (_handlers.Count == 0)
            {
                throw new ArgumentException("At least one installer handler must be provided.", nameof(handlers));
            }
        }

        public bool Install(InstallRequest request)
        {
            var content = _detector.ReadFileAsAscii(request.FilePath);
            var executableMetadata = _executableMetadataReader.ReadMetadata(request.FilePath);
            var handler = _handlers.FirstOrDefault(h => h.CanHandle(request.FilePath, content, executableMetadata));
            if (handler is null)
            {
                _logger.Warn($"No installer handler found for file: {request.FilePath}");
                return false;
            }

            try
            {
                _logger.Info($"Handler '{handler.Type}' will be used to install file: {request.FilePath}");
                var success = handler.Install(request);
                if (!success)
                {
                    _logger.Warn($"Handler '{handler.Type}' failed to install file: {request.FilePath}");
                }

                return success;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Exception occurred while installing file '{request.FilePath}' using handler '{handler.Type}'");
                return false;
            }
        }
    }
}