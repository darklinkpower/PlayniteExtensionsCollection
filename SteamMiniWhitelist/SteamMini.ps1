function global:GetMainMenuItems
{
    param($menuArgs)

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Launch Steam in `"Mini`" mode"
    $menuItem1.FunctionName = "Start-SteamMini"
    $menuItem1.MenuSection = "@Steam Mini"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = "Launch Steam normally"
    $menuItem2.FunctionName = "Start-SteamNormal"
    $menuItem2.MenuSection = "@Steam Mini"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem3.Description = "Add to Steam Mini Whitelist"
    $menuItem3.FunctionName = "Add-SteamMiniFeature"
    $menuItem3.MenuSection = "@Steam Mini"

    $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem4.Description = "Remove from Steam Mini Whitelist"
    $menuItem4.FunctionName = "Remove-SteamMiniFeature"
    $menuItem4.MenuSection = "@Steam Mini"
    
    return $menuItem1, $menuItem2, $menuItem3, $menuItem4
}

function global:Add-SteamMiniFeature
{
    # Create Feature
    $featureName = "Steam Mini Whitelist"
    $feature = $PlayniteApi.Database.Features.Add($featureName)
    [guid[]]$featureIds = $feature.Id
    
    # Set GameDatabase
    $GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.PluginId -eq "cb91dfc9-b977-43bf-8e70-55f46e410fab"}
    
    # Set counters
    $FeatureAdded = 0
    
    # Start Execution for each game in the database
    foreach ($game in $GameDatabase) {
        if ($game.Features.name -contains "$featureName")
        {
            $__logger.Info("`"$($game.name)`" already had `"$featureName`" feature.")
        }
        else
        {
            # Add feature to game
            if ($game.FeatureIds) 
            {
                $game.FeatureIds += $featureIds
            } 
            else
            {
                # Fix in case game has null FeatureIds
                $game.FeatureIds = $featureIds
            }
            
            # Update game in database
            $PlayniteApi.Database.Games.Update($game)
            $FeatureAdded++
            $__logger.Info("Added `"$featureName`" feature to `"$($game.name)`".")
        }
    }
    
    # Show finish dialogue
    $PlayniteApi.Dialogs.ShowMessage("Added `"$featureName`" feature to $FeatureAdded games.","Steam Mini");
}

function global:Remove-SteamMiniFeature
{
    # Create Feature
    $featureName = "Steam Mini Whitelist"
    $feature = $PlayniteApi.Database.Features.Add($featureName)
    [guid[]]$featureIds = $feature.Id
    
    # Set GameDatabase
    $GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.PluginId -eq "cb91dfc9-b977-43bf-8e70-55f46e410fab"}
    
    # Set counters
    $FeatureRemoved = 0
    
    # Start Execution for each game in the database
    foreach ($game in $GameDatabase) {
        if ($game.Features.name -contains "$featureName")
        {
            # Remove feature from game
            $game.FeatureIds.Remove("$featureIds")
            $PlayniteApi.Database.Games.Update($game)
            $FeatureRemoved++
            $__logger.Info("Removed `"$featureName`" feature from `"$($game.name)`".")
        }
        else
        {
            $__logger.Info("`"$($game.name)`" doesn't have `"$featureName`" feature.")
        }
    }
    
    # Show results dialogue
    $PlayniteApi.Dialogs.ShowMessage("Removed `"$featureName`" feature from $FeatureRemoved games.","Steam Mini");
}

function Start-Steam
{
    param(
        $Arguments
    )
    
    # Get Steam executable path
    $Key = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::CurrentUser, [Microsoft.Win32.RegistryView]::Registry64)
    $RegSubKey =  $Key.OpenSubKey("Software\Valve\Steam")
    $SteamPath = $RegSubKey.GetValue("SteamExe")
    if ($null -eq $SteamPath)
    {
        $SteamPath = 'C:\Program Files (x86)\Steam\steam.exe'
        $__logger.Warn("Could not find Steam registry value. Default path will be used.")
    }

    # Start Steam
    $Steam = Get-Process 'steam' -ErrorAction 'SilentlyContinue'
    if ($Steam)
    {
        $PlayniteApi.Dialogs.ShowErrorMessage("Steam is already running.","Steam Mini");
    }
    elseif (Test-Path $SteamPath)
    {
        Start-Process $SteamPath "$Arguments"
        $__logger.Info("Steam launched with arguments `"$Arguments`".")
    }
    else
    {
        $PlayniteApi.Dialogs.ShowErrorMessage("Steam executable not found in `"$SteamPath`".","Steam Mini");
    }
}
function Start-SteamMini
{
    $__logger.Info("`"Start-SteamMini`" function Started.")
    $Arguments = "-no-browser -silent"
    Start-Steam $Arguments
}

function Start-SteamNormal
{
    $__logger.Info("`"Start-SteamNormal`" function Started.")
    $Arguments = " "
    Start-Steam $Arguments
}

function OnGameStarting
{
    param(
        $game
    )
    
    $__logger.Info("Execution started in `"Whitelist`" mode.")
    
    # Check if game contains Steam Mini feature
    $featureName = "Steam Mini Whitelist"
    if ($game.features.name -notcontains "$featureName")
    {
        $__logger.Info("`"$($game.name)`" not contains Steam Mini feature. Execution will stop.")
        exit
    }

    # Get Steam executable path
    $Key = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::CurrentUser, [Microsoft.Win32.RegistryView]::Registry64)
    $RegSubKey =  $Key.OpenSubKey("Software\Valve\Steam")
    $SteamPath = $RegSubKey.GetValue("SteamExe")
    if ($null -eq $SteamPath)
    {
        $SteamPath = 'C:\Program Files (x86)\Steam\steam.exe'
        $__logger.Warn("Could not find Steam registry value. Default path will be used.")
    }

    if ($game.PluginId -eq "cb91dfc9-b977-43bf-8e70-55f46e410fab")
    {
        if ($game.InstallationStatus -eq 'Installed')
        {
            $__logger.Info("`"$($Game.Name)`" is a Steam game and is installed")
            $Steam = Get-Process 'steam' -ErrorAction 'SilentlyContinue'
            if ($Steam)
            {
                $__logger.Info("Steam is already running and won't be started in `"mini`" mode.")
            }
            elseif (Test-Path $SteamPath)
            {
                Start-Process $SteamPath "-no-browser -silent"
                $__logger.Info("Steam launched in `"mini`" mode.")
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
    else
    {
        $__logger.Info("`"$($Game.Name)`" is not a Steam game.")
    }
}