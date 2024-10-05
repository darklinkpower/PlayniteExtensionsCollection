using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper
{
    public class SteamHelper
    {
        private bool _isSteamBpmEnvVarSet = false;
        private readonly string _configurationDirectory;
        private readonly IPlayniteAPI _playniteApi;

        public SteamHelper (string configurationDirectory, IPlayniteAPI playniteApi)
        {
            _configurationDirectory = configurationDirectory;
            _playniteApi = playniteApi;
        }

        public bool IsEnvinronmentVariableSet()
        {
            return _isSteamBpmEnvVarSet;
        }

        public void SetBigPictureModeEnvVariable()
        {
            // Setting "SteamTenfoot" to "1" forces to use the Steam BPM overlay
            // but it will still not work if Steam BPM is not running
            var variable = Environment.GetEnvironmentVariable("SteamTenfoot", EnvironmentVariableTarget.Process);
            if (variable.IsNullOrEmpty() || variable != "1")
            {
                Environment.SetEnvironmentVariable("SteamTenfoot", "1", EnvironmentVariableTarget.Process);
            }

            _isSteamBpmEnvVarSet = true;
        }

        public void RemoveBigPictureModeEnvVariable()
        {
            var variable = Environment.GetEnvironmentVariable("SteamTenfoot", EnvironmentVariableTarget.Process);
            if (!variable.IsNullOrEmpty())
            {
                Environment.SetEnvironmentVariable("SteamTenfoot", string.Empty, EnvironmentVariableTarget.Process);
            }

            _isSteamBpmEnvVarSet = false;
        }

        private string GetConfiguredSteamId(Game game)
        {
            if (Steam.IsGameSteamGame(game))
            {
                return game.GameId;
            }

            if (!game.InstallDirectory.IsNullOrEmpty())
            {
                var appIdTextPath = Path.Combine(game.InstallDirectory, "steam_appid.txt");
                if (FileSystem.FileExists(appIdTextPath))
                {
                    return FileSystem.ReadStringFromFile(appIdTextPath);
                }
            }

            var historyFlagFile = Path.Combine(_configurationDirectory, "SteamId_" + game.Id.ToString());
            if (FileSystem.FileExists(historyFlagFile))
            {
                return FileSystem.ReadStringFromFile(historyFlagFile);
            }

            return string.Empty;
        }

        public void OpenGameSteamControllerConfig(Game game)
        {
            var steamId = GetConfiguredSteamId(game);
            if (steamId.IsNullOrEmpty())
            {
                _playniteApi.Dialogs.ShowErrorMessage(
                    string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_DialogMessageSteamControlIdNotFound"), game.Name),
                    "Special K Helper");
                return;
            }

            _playniteApi.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCSpecial_K_Helper_DialogMessageSteamControlNotice"), game.Name, steamId),
                "Special K Helper");

            ProcessStarter.StartUrl($"steam://currentcontrollerconfig/{steamId}");
        }
    }
}
