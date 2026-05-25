using Playnite.SDK.Models;
using SpecialKHelper.Core.Domain;
using SteamCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.Core.Application
{
    public class SteamEnvironmentHandler
    {
        private readonly SpecialKHelperSettingsViewModel _settings;
        private readonly SteamHelper _steamHelper;

        public SteamEnvironmentHandler(
            SpecialKHelperSettingsViewModel settings,
            SteamHelper steamHelper)
        {
            _settings = settings;
            _steamHelper = steamHelper;
        }

        public void OnGameStarting(Game game)
        {
            if (_steamHelper.IsEnvinronmentVariableSet())
            {
                if (_settings.Settings.SteamOverlayForBpm != SteamOverlay.BigPictureMode
                    || Steam.IsGameSteamGame(game)
                    || !SteamClient.GetIsSteamBpmRunning())
                {
                    _steamHelper.RemoveBigPictureModeEnvVariable();
                }
            }
            else if (_settings.Settings.SteamOverlayForBpm == SteamOverlay.BigPictureMode
                     && SteamClient.GetIsSteamBpmRunning())
            {
                _steamHelper.SetBigPictureModeEnvVariable();
            }
        }

        public void OnGameStopped()
        {
            if (_steamHelper.IsEnvinronmentVariableSet())
            {
                _steamHelper.RemoveBigPictureModeEnvVariable();
            }
        }
    }
}