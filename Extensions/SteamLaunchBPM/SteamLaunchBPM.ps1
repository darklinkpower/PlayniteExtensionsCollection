function OnGameStarting
{
    param(
        $game
    )
    
    if ($PlayniteApi.ApplicationInfo.Mode -eq "Desktop")
    {
        $__logger.Warn("Playnite is running in desktop mode. Execution stopped.")
        return
    }
    
    # Get Steam executable path
    $Key = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::CurrentUser, [Microsoft.Win32.RegistryView]::Registry64)
    $RegSubKey =  $Key.OpenSubKey("Software\Valve\Steam")
    if ($RegSubKey)
    {
        $SteamPath = $RegSubKey.GetValue("SteamExe")
    }
    if ($null -eq $SteamPath)
    {
        $SteamPath = 'C:\Program Files (x86)\Steam\steam.exe'
        $__logger.Warn("Could not find Steam registry value. Default path will be used.")
    }

    if ([Playnite.SDK.BuiltinExtensions]::GetExtensionFromId($game.PluginId) -eq "SteamLibrary")
    {
        if ($game.InstallationStatus -eq 'Installed')
        {
            $__logger.Info("`"$($Game.Name)`" is a Steam game and is installed")
            $Steam = Get-Process 'steam' -ErrorAction 'SilentlyContinue'
            if ($Steam)
            {
                for ($i = 0; $i -lt 6; $i++) {
                    Start-Process "steam://ExitSteam"
                    $Steam = Get-Process 'steam' -ErrorAction 'SilentlyContinue'
                    if ($Steam)
                    {
                        Start-Sleep -Seconds 3
                    }
                    else
                    {
                        if (Test-Path $SteamPath)
                        {
                            Start-Process $SteamPath "-bigpicture"
                            $__logger.Info("Steam launched in `"Big Picture`" mode.")
            
                            # Sleep time in case Steam needs time to start before launching game from Playnite
                            Start-Sleep -Milliseconds 500
                        }
                        else
                        {
                            $__logger.Error("Steam executable not found in `"$SteamPath`".")
                        }
                        break
                    }
                }
            }
            elseif (Test-Path $SteamPath)
            {
                Start-Process $SteamPath "-bigpicture"
                $__logger.Info("Steam launched in `"Big Picture`" mode.")

                # Sleep time in case Steam needs time to start before launching game from Playnite
                Start-Sleep -Milliseconds 500
            }
            else
            {
                $__logger.Error("Steam executable not found in `"$SteamPath`".")
            }
        }
        else
        {
            $__logger.Info("`"$($Game.Name)`" is a Steam game but is not installed.")
        }
    }
}