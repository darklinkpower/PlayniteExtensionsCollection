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
    
    if ([Playnite.SDK.BuiltinExtensions]::GetExtensionFromId($game.PluginId) -ne "SteamLibrary")
    {
        return
    }

    if ($game.InstallationStatus -ne 'Installed')
    {
        return
    }

    # Get Steam executable path
    $Key = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::CurrentUser, [Microsoft.Win32.RegistryView]::Registry64)
    $RegSubKey =  $Key.OpenSubKey("Software\Valve\Steam")
    if ($RegSubKey)
    {
        $steamPath = $RegSubKey.GetValue("SteamExe")
    }
    if ($null -eq $steamPath)
    {
        $steamPath = 'C:\Program Files (x86)\Steam\steam.exe'
        $__logger.Warn("Could not find Steam registry value. Default path will be used.")
    }

    if (!(Test-Path $steamPath))
    {
        $__logger.Error("Steam executable not found in `"$steamPath`".")
        return
    }

    $steamIsRunning = $false
    $steam = Get-Process 'steam' -ErrorAction 'SilentlyContinue'
    if ($Steam)
    {
        Start-Process $steamPath "-shutdown"
        $__logger.Info("Steam was shutdown.")
        for ($i = 0; $i -lt 6; $i++) {
            $Steam = Get-Process 'steam' -ErrorAction 'SilentlyContinue'
            if ($Steam)
            {
                $steamIsRunning = $true
                Start-Sleep -Seconds 3
            }
            else
            {
                $steamIsRunning = $false
                break
            }
        }
        $steamIsRunning
    }
    if ($steamIsRunning -eq $false)
    {
        Start-Process $steamPath "-bigpicture"
        $__logger.Info("Steam launched in `"Big Picture`" mode.")

        # Sleep time in case Steam needs time to start before launching game from Playnite
        Start-Sleep -Milliseconds 500
    }
    else
    {
        $__logger.Warn("Steam was detected as running and was not launched via the extension.")
    }
}