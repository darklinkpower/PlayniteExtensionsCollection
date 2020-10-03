function global:GetMainMenuItems()
{
    param($menuArgs)

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Path Updater - Point to new path of selected games"
	$menuItem1.FunctionName = "InstallationPathUpdater"
	$menuItem1.MenuSection = "@Installation Status Updater"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = "Status Updater - Check installation status"
	$menuItem2.FunctionName = "InstallationStatusUpdater"
	$menuItem2.MenuSection = "@Installation Status Updater"
	
	return $menuItem1, $menuItem2
}

function global:Invoke-InstallationStatusCheck()
{
	# Set GameDatabase
	$GameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.PluginId -eq "00000000-0000-0000-0000-000000000000"} | Where-Object {($_.GameImagePath) -or (($_.InstallDirectory) -and ($_.PlayAction.Type -eq "File"))}
	
	# Set Counters
	$global:MarkedInstalled = 0
	$global:MarkedUninstalled = 0
	
	# Create collection for processed games
	[System.Collections.Generic.List[Object]]$global:GamesProcessed = @()

	foreach ($game in $GameDatabase) {
		
		# Set game file path
		if ($game.GameImagePath)
		{
			$GameFilePath = $game.GameImagePath
		}
		elseif ($game.PlayAction.Path)
		{
			$GameFilePath = $game.InstallDirectory.TrimEnd('\') + '\' +  $game.PlayAction.Path.TrimStart('\')
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
function OnApplicationStarted()
{
	Invoke-InstallationStatusCheck
}
function InstallationStatusUpdater()
{
	Invoke-InstallationStatusCheck

	# Show finish dialogue with results and ask if user wants to export results
	if ($GamesProcessed.count -gt 0)
	{
		$ExportChoice = $PlayniteApi.Dialogs.ShowMessage("$MarkedUninstalled previously installed games were marked as uninstalled`n$MarkedInstalled previously uninstalled games were marked as installed`n`nDo you want to export results?", "Installation Status Updater", 4)
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
				$PlayniteApi.Dialogs.ShowMessage("Results exported successfully.", "Installation Status Updater");
			}
		}
	}
	else
	{
		$PlayniteApi.Dialogs.ShowMessage("$MarkedUninstalled previously installed games were marked as uninstalled`n$MarkedInstalled previously uninstalled games were marked as installed", "Installation Status Updater")
	}
}

function global:InstallationPathUpdater()
{
	# Set GameDatabase
	$GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object { ($_.GameImagePath) }
	
	# Set Counters
	$CountPathChanged = 0
	
	# Create collection for processed games
	[System.Collections.Generic.List[Object]]$GamesProcessed = @()
	
	# Select new directory to point installations
	$PlayniteApi.Dialogs.ShowMessage("Select the new folder path to point the selected games", "Installation Path Updater")
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
		$CountPathChanged++
		
		# Log information and add to processed games collection
		$__logger.Info("Installation Path Updater - Game: `"$($game.name)`" | OldPath: `"$GameImagePathOld`" | NewPath: `"$GameImagePathNew`"")
		$GameObject = [PSCustomObject]@{

			Name  = "$($game.name)"
			PathOld = "$GameImagePathOld"
			PathNew = "$GameImagePathNew"
		}
		$GamesProcessed.Add($GameObject)
	}
	
	# Show finish dialogue with results and ask if user wants to export results
	if ($CountPathChanged -gt 0)
	{
		$ExportChoice = $PlayniteApi.Dialogs.ShowMessage("Changed install path of $CountPathChanged games.`n`nDo you want to export results?", "Installation Path Updater", 4)
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
				$PlayniteApi.Dialogs.ShowMessage("Results exported successfully.", "Installation Path Updater");
			}
		}
	}
	else
	{
		$PlayniteApi.Dialogs.ShowMessage("Changed install path of $CountPathChanged games.", "Installation Path Updater")
	}
	Invoke-InstallationStatusCheck
}