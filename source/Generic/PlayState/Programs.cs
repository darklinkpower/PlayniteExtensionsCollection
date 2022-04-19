using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace PlayState
{
    public static class Programs
    {
        private static ILogger logger = LogManager.GetLogger();
        public static string GetUwpWorkdirFromGameId(string gameId)
        {
            try
            {
                var manager = new PackageManager();
                IEnumerable<Package> packages = manager.FindPackagesForUser(WindowsIdentity.GetCurrent().User.Value);
                foreach (var package in packages)
                {
                    if (package.IsFramework || package.IsResourcePackage || package.SignatureKind != PackageSignatureKind.Store)
                    {
                        continue;
                    }

                    try
                    {
                        if (package.InstalledLocation == null)
                        {
                            continue;
                        }

                        if (package.Id.FamilyName == gameId)
                        {
                            return package.InstalledLocation.Path;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    catch
                    {
                        // InstalledLocation accessor may throw Win32 exception for unknown reason
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to get list of installed UWP apps.");
            }

            return null;
        }
    }
}