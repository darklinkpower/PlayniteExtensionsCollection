function global:GetMainMenuItems()
{
	param($menuArgs)

	$menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
	$menuItem1.Description = "Fill Region of games"
	$menuItem1.FunctionName = "GameRegionFiller"
	$menuItem1.MenuSection = "@Game Region Filler"
	
	return $menuItem1
}

function global:GameRegionFiller()
{
	# Set GameDatabase
	$GameDatabase = $PlayniteApi.Database.Games | Where-Object {( ($_.GameImagePath) -and (-not ($_.Region)) )}
	
	# Set Counters
	$RegionAdded = 0
	
	# Create collection for processed games
	[System.Collections.Generic.List[Object]]$GamesProcessed = @()
	
	# Set Single region codes
	$Australia = "Australia"
	$Brazil = "Brazil"
	$Canada = "Canada"
	$China = "China"
	$France = "France"
	$Germany = "Germany"
	$Hong_Kong = "Hong Kong"
	$Italy = "Italy"
	$Japan = "Japan"
	$Korea = "Korea"
	$Netherlands = "Netherlands"
	$Spain = "Spain"
	$Sweden = "Sweden"
	$USA = "USA"
	
	# Set Multi region codes
	$World = "World"
	$Europe = "Europe"
	$Asia = "Asia"
	$Japan_USA = "Japan, USA"
	$Japan_Europe = "Japan, Europe"
	$USA_Europe = "USA, Europe"
	$USA_Australia = "USA, Australia"
	$USA_Korea = "USA, Korea"
	
	# Create regions array
	[System.Collections.Generic.List[String]]$RegionsArray = @(
		"$Australia",
		"$Brazil",
		"$Canada",
		"$China",
		"$France",
		"$Germany",
		"$Hong_Kong",
		"$Italy",
		"$Japan",
		"$Korea",
		"$Netherlands",
		"$Spain",
		"$Sweden",
		"$USA",
		"$World",
		"$Europe",
		"$Asia",
		"$Japan_USA",
		"$Japan_Europe",
		"$USA_Europe",
		"$USA_Australia",
		"$USA_Korea"
	)

	# Create Regions in database
	foreach ($region in $RegionsArray) {
		New-Variable -Name "Object_$($region)" -Value $($PlayniteApi.Database.Regions.Add($region)) -Force
		New-Variable -Name "Guid_$($region)" -Value $($(Get-Variable -Name "Object_$($region)").Value).Id -Force
	}
	
	foreach ($game in $GameDatabase) {
		
		# Get game filename
		$FileName = [System.IO.Path]::GetFileNameWithoutExtension($($Game.GameImagePath))
		$FileNameExt = [System.IO.Path]::GetFileName($($Game.GameImagePath))

		# Check if game file matches any common region
		if ($FileName -match "\((W|World)\)")
			{ $RegionFound = $World }
		elseif ($FileName -match "\((U|US|USA)\)")
			{ $RegionFound = $USA }
		elseif ($FileName -match "\((E|EU|Europe)\)")
			{ $RegionFound = $Europe }
		elseif ($FileName -match "\((J|JP|Japan)\)")
			{ $RegionFound = $Japan }
		
		# Check if game file matches any single region
		elseif ($FileName -match "\((A|AU|Australia)\)")
			{ $RegionFound = $Australia }
		elseif ($FileName -match "\((B|Brazil)\)")
			{ $RegionFound = $Brazil }
		elseif ($FileName -match "\((C|Canada)\)")
			{ $RegionFound = $Canada }
		elseif ($FileName -match "\((Ch|China)\)")
			{ $RegionFound = $China }
		elseif ($FileName -match "\((F|France)\)")
			{ $RegionFound = $France }
		elseif ($FileName -match "\((G|Germany)\)")
			{ $RegionFound = $Germany }
		elseif ($FileName -match "\((HK|Hong Kong)\)")
			{ $RegionFound = $Hong_Kong }
		elseif ($FileName -match "\((I|Italy)\)")
			{ $RegionFound = $Italy }
		elseif ($FileName -match "\((K|Korea)\)")
			{ $RegionFound = $Korea }
		elseif ($FileName -match "\((D|Nl|Netherlands)\)")
			{ $RegionFound = $Netherlands }
		elseif ($FileName -match "\((S|Spain)\)")
			{ $RegionFound = $Spain }
		elseif ($FileName -match "\((Sw|Sweden)\)")
			{ $RegionFound = $Sweden }
		
		# Check if game file matches any multi region
		elseif ($FileName -match "\((As|Asia)\)")
			{ $RegionFound = $Asia }
		elseif ($FileName -match "\((Japan, USA)\)")
			{ $RegionFound = $Japan_USA }		
		elseif ($FileName -match "\((Japan, Europe)\)")
			{ $RegionFound = $Japan_Europe }
		elseif ($FileName -match "\((USA, Europe)\)")
			{ $RegionFound = $USA_Europe }	
		elseif ($FileName -match "\((USA, Australia)\)")
			{ $RegionFound = $USA_Australia }		
		elseif ($FileName -match "\((USA, Korea)\)")
			{ $RegionFound = $USA_Korea }
		else {
			continue
		}
		
		# Apply region to game
		$game.RegionId = [guid]$(Get-Variable -Name "Guid_$($RegionFound)" -ValueOnly)
		$PlayniteApi.Database.Games.Update($game)
		
		# Log information, increase count and add to processed games collection
		$__logger.Info("Game Region Filler - Game: `"$($game.name)`" Added Region: `"$RegionFound`" Filename: `"$FileNameExt`"")
		$RegionAdded++
		$GamesProcessed.Add($game)
	}
	
	# Remove unused regions from database
	foreach ($region in $RegionsArray) {
		[string]$RegionGuid = [guid]$(Get-Variable -Name "Guid_$($region)" -ValueOnly)
		[int]$RegionCount = $($PlayniteApi.Database.Games | Where-Object {$_.RegionId -eq $RegionGuid}).count
		if ($RegionCount -eq 0)
		{
			$PlayniteApi.Database.Regions.Remove("$RegionGuid")
		}
	}

	# Show finish dialogue with results and ask if user wants to export results
	if ($RegionAdded -gt 0)
	{
		$ExportChoice = $PlayniteApi.Dialogs.ShowMessage("Added region to $RegionAdded games.`n`nDo you want to export results?", "Game Region Filler", 4)
		if ($ExportChoice -eq "Yes")
		{
			$ExportPath = $PlayniteApi.Dialogs.SaveFile("CSV|*.csv|Formated TXT|*.txt")
			if ($ExportPath)
			{
				if ($ExportPath -match ".csv$")
				{
					$GamesProcessed | Select-Object Name, GameImagePath, Region, Platform | ConvertTo-Csv -NoTypeInformation | Out-File $ExportPath -Encoding 'UTF8'
				}
				else
				{
					$GamesProcessed | Select-Object Name, GameImagePath, Region, Platform | Format-Table -AutoSize | Out-File $ExportPath -Encoding 'UTF8'
				}
				$PlayniteApi.Dialogs.ShowMessage("Results exported successfully.", "Game Region Filler");
			}
		}
	}
	else
	{
		$PlayniteApi.Dialogs.ShowMessage("Added region to $RegionAdded games.")
	}
}