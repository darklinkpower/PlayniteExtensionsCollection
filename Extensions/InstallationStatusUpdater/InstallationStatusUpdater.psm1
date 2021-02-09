function GetMainMenuItems
{
    param(
        $menuArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemPathUpdaterDescription")
    $menuItem1.FunctionName = "InstallationPathUpdater"
    $menuItem1.MenuSection = "@Installation Status Updater"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemStatusUpdaterDescription")
    $menuItem2.FunctionName = "InstallationStatusUpdater"
    $menuItem2.MenuSection = "@Installation Status Updater"
    
    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem3.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuAddIgnoreFeatureDescription")
    $menuItem3.FunctionName = "Add-IgnoreFeature"
    $menuItem3.MenuSection = "@Installation Status Updater"

    $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem4.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuRemoveIgnoreFeatureDescription")
    $menuItem4.FunctionName = "Remove-IgnoreFeature"
    $menuItem4.MenuSection = "@Installation Status Updater"

    return $menuItem1, $menuItem2, $menuItem3, $menuItem4
}

function Invoke-InstallationStatusCheck
{
    # Set GameDatabase
    $GameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.PluginId -eq "00000000-0000-0000-0000-000000000000"} | Where-Object { ($_.GameImagePath) -or ($_.PlayAction.Type -eq "File") }
    
    # Set Counters
    $global:MarkedInstalled = 0
    $global:MarkedUninstalled = 0
    
    # Create collection for processed games
    [System.Collections.Generic.List[Object]]$global:GamesProcessed = @()

    # Set skip game feature
    $featureName = "Ignore in Installation Status Updater"

    foreach ($game in $GameDatabase) {

        if ($game.features.name -contains "$featureName")
        {
            continue
        }
        
        # Set game file path
        if ($game.GameImagePath)
        {
            $GameFilePath = $game.GameImagePath
        }
        elseif ($game.PlayAction.Path -and $game.InstallDirectory)
        {
            if($game.InstallDirectory -eq '{InstallDir}')
            {
                $GameFilePath = $game.PlayAction.Path
            }
            else
            {
                $GameFile = [System.IO.Path]::GetFileName($game.PlayAction.Path)
                $GameFilePath = $game.InstallDirectory.TrimEnd('\') + '\' +  $GameFile  
            }
        }
        else
        {
            $GameFilePath = $game.PlayAction.Path
        }
        
        # Check if game path is not valid in games marked as installed
        if (($($game.InstallationStatus) -eq 'Installed') -and (![System.IO.File]::Exists($GameFilePath)))
        {
            $game.IsInstalled = $False
            $PlayniteApi.Database.Games.Update($game)
            $global:MarkedUninstalled++
            $__logger.Info("InstallationStatusUpdater - `"$($game.name)`" marked as uninstalled")
            $GamesProcessed.Add($game)
        }
        
        # Check if game path is valid in games marked as uninstalled
        elseif (($($Game.InstallationStatus) -eq 'Uninstalled') -and ([System.IO.File]::Exists($GameFilePath)))
        {
            $game.IsInstalled = $True
            $PlayniteApi.Database.Games.Update($game)
            $global:MarkedInstalled++
            $__logger.Info("InstallationStatusUpdater - `"$($game.name)`" marked as installed")
            $GamesProcessed.Add($game)
        }
    }
}
function OnApplicationStarted
{
    Invoke-InstallationStatusCheck
}
function InstallationStatusUpdater
{
    param(
        $scriptMainMenuItemActionArgs
    )

    Invoke-InstallationStatusCheck

    # Show finish dialogue with results and ask if user wants to export results
    if ($GamesProcessed.count -gt 0)
    {
        $ExportChoice = $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCStatusUpdaterExportChoiceMessage") -f $MarkedUninstalled, $MarkedInstalled), "Installation Status Updater", 4)
        if ($ExportChoice -eq "Yes")
        {
            $ExportPath = $PlayniteApi.Dialogs.SaveFile("CSV|*.csv|Formated TXT|*.txt")
            if ($ExportPath)
            {
                if ($ExportPath -match ".csv$")
                {
                    $GamesProcessed | Select-Object Name, GameImagePath, InstallationStatus, Platform | ConvertTo-Csv -NoTypeInformation | Out-File $ExportPath -Encoding 'UTF8'
                }
                else
                {
                    $GamesProcessed | Select-Object Name, GameImagePath, InstallationStatus, Platform | Format-Table -AutoSize | Out-File $ExportPath -Encoding 'UTF8'
                }
                $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCExportSuccessMessage"), "Installation Status Updater");
            }
        }
    }
    else
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCNoChangesResultsMessage"), "Installation Status Updater")
    }
}

function InstallationPathUpdater
{
    param(
        $scriptMainMenuItemActionArgs
    )

    # Set GameDatabase
    $GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object { ($_.GameImagePath) }
    
    # Set Counters
    $countPathChanged = 0
    
    # Create collection for processed games
    [System.Collections.Generic.List[Object]]$GamesProcessed = @()
    
    # Select new directory to point installations
    $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCPathUpdaterSelectDirectoryMessage"), "Installation Path Updater")
    $NewDir = $PlayniteApi.Dialogs.SelectFolder()
    if (!$NewDir)
    {
        exit
    }

    foreach ($game in $GameDatabase) {
        
        # Get game filename and used paths
        $FileNameExt = [System.IO.Path]::GetFileName($($Game.GameImagePath))
        $GameImagePathOld = $($Game.GameImagePath)
        $GameImagePathNew = Join-Path -Path $NewDir -ChildPath $FileNameExt
    
        # Update new path to game
        $game.GameImagePath = $GameImagePathNew
        $game.InstallDirectory = $NewDir
        $PlayniteApi.Database.Games.Update($game)
        $countPathChanged++
        
        # Log information and add to processed games collection
        $__logger.Info("Installation Path Updater - Game: `"$($game.name)`" | OldPath: `"$GameImagePathOld`" | NewPath: `"$GameImagePathNew`"")
        $GameObject = [PSCustomObject]@{

            Name  = $game.name
            PathOld = $GameImagePathOld
            PathNew = $GameImagePathNew
        }
        $GamesProcessed.Add($GameObject)
    }
    
    # Show finish dialogue with results and ask if user wants to export results
    if ($countPathChanged -gt 0)
    {
        $ExportChoice = $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCPathUpdaterExportChoiceMessage") -f $countPathChanged), "Installation Path Updater", 4)
        if ($ExportChoice -eq "Yes")
        {
            $ExportPath = $PlayniteApi.Dialogs.SaveFile("CSV|*.csv|Formated TXT|*.txt")
            if ($ExportPath)
            {
                if ($ExportPath -match "\.csv$")
                {
                    $GamesProcessed | Select-Object Name, PathOld, PathNew | ConvertTo-Csv -NoTypeInformation | Out-File $ExportPath -Encoding 'UTF8'
                }
                else
                {
                    $GamesProcessed | Select-Object Name, PathOld, PathNew | Format-Table -AutoSize | Out-File $ExportPath -Encoding 'UTF8'
                }
                $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCExportSuccessMessage"), "Installation Path Updater")
            }
        }
    }
    else
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCNoChangesResultsMessage"), "Installation Path Updater")
    }
    Invoke-InstallationStatusCheck
}

function Add-IgnoreFeature
{
    param(
        $scriptMainMenuItemActionArgs
    )

    # Create Feature
    $featureName = "Ignore in Installation Status Updater"
    $feature = $PlayniteApi.Database.Features.Add($featureName)
    [guid[]]$featureIds = $feature.Id
    
    # Set GameDatabase
    $GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.PluginId -eq "00000000-0000-0000-0000-000000000000"}
    
    # Set counters
    $featureAdded = 0
    
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
            $featureAdded++
            $__logger.Info("Added `"$featureName`" feature to `"$($game.name)`".")
        }
    }
    
    # Show finish dialogue
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCStatusUpdaterAddIgnoreFeatureMessage") -f $featureName, $featureAdded),"Installation Status Updater")
}

function Remove-IgnoreFeature
{
    param(
        $scriptMainMenuItemActionArgs
    )

    # Create Feature
    $featureName = "Ignore in Installation Status Updater"
    $feature = $PlayniteApi.Database.Features.Add($featureName)
    [guid[]]$featureIds = $feature.Id
    
    # Set GameDatabase
    $GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.PluginId -eq "00000000-0000-0000-0000-000000000000"}
    
    # Set counters
    $featureRemoved = 0
    
    # Start Execution for each game in the database
    foreach ($game in $GameDatabase) {
        if ($game.Features.name -contains "$featureName")
        {
            # Remove feature from game
            $game.FeatureIds.Remove("$featureIds")
            $PlayniteApi.Database.Games.Update($game)
            $featureRemoved++
            $__logger.Info("Removed `"$featureName`" feature from `"$($game.name)`".")
        }
        else
        {
            $__logger.Info("`"$($game.name)`" doesn't have `"$featureName`" feature.")
        }
    }
    
    # Show results dialogue
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCStatusUpdaterRemoveIgnoreFeatureMessage") -f $featureName, $featureRemoved),"Installation Status Updater")
}