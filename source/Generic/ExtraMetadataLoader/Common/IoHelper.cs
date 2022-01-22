using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.Common
{
    class IoHelper
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        public static bool MoveFile(string sourcePath, string destinationPath, bool copyOnly)
        {
            try
            {
                if (sourcePath.Equals(destinationPath, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Debug($"MoveFile. Source path and target path are the same: {sourcePath}");
                    return false;
                }
                if (!File.Exists(sourcePath))
                {
                    logger.Debug($"MoveFile. Source doesn't exists: {sourcePath}");
                    return false;
                }

                var targetDir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                else if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                if (copyOnly)
                {
                    File.Copy(sourcePath, destinationPath);
                }
                else
                {
                    File.Move(sourcePath, destinationPath);
                }
                
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error during MoveFile with source path: {sourcePath}. Destination path: {destinationPath}");
                return false;
            }
        }

        internal static bool DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    logger.Info($"Deleted file in {filePath}");
                }
                else
                {
                    logger.Info($"File doesn't exist in {filePath} and was not deleted");
                }

                return true;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error deleting file in {filePath}");
                return false;
            }
        }
    }
}
