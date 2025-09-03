using JastUsaLibrary.Features.InstallationHandler.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Features.InstallationHandler.Application
{

    public class InstallerService
    {
        private readonly IEnumerable<IInstallerHandler> _handlers;
        private readonly IInstallerDetector _detector;

        public InstallerService(IEnumerable<IInstallerHandler> handlers, IInstallerDetector detector)
        {
            _handlers = handlers;
            _detector = detector;
        }

        public void Install(InstallRequest request)
        {
            var content = _detector.ReadFileAsAscii(request.FilePath);
            var handler = _handlers.FirstOrDefault(h => h.CanHandle(request.FilePath, content));
            if (handler is null)
            {
                throw new InvalidOperationException("No handler available for file: " + request.FilePath);
            }

            try
            {
                handler.Install(request);
            }
            catch (Exception e)
            {
                // Log exception
            }

        }
    }
}