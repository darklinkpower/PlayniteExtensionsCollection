function Invoke-AddTag {
	param (
		[object]$game,
		[string]$featureName,
		[guid]$featureIds,
		[string]$FoundStatus
	)
	# Check if game already has the feature and if it has been removed
	if ($game.Features.name -eq $featureName)
	{
		if ($FoundStatus -eq $false)
		{
			$game.FeatureIds.Remove($featureIds)
			$__logger.Info("NVIDIA Geforce NowCompatibility Checker - Feature removed from `"$($game.name)`"")
			$global:FeatureRemoved++
			return
		}
		else
		{
			$global:FeatureInGame++
			return
		}
	}
	elseif ($FoundStatus -eq $true)
	{
		# Add feature Id to game
		if ($game.FeatureIds) 
		{
			$game.FeatureIds += $featureIds
		} 
		else 
		{
			# Fix in case game has null FeatureIds
			$game.FeatureIds = $featureIds
		}
		
		# Update game in database and increase counters
		$PlayniteApi.Database.Games.Update($game)
		$__logger.Info("NVIDIA Geforce NowCompatibility Checker - Feature added to `"$($game.name)`"")
		$global:FeatureInGame++
		$global:FeatureInGameAdded++
		return
	}
	else
	{
		return
	}
}

function global:NVIDIAGeforceNowCompatibilityChecker()
{
	# Set GameDatabase
	$GameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.Platform.name -eq "PC"}  | Where-Object {$_.source.name -match "(Steam|Epic|Uplay|Origin)"}
	
	# Create "NVIDIA GeForce NOW" Feature
	$featureName = "NVIDIA GeForce NOW"
	$feature = $PlayniteApi.Database.Features.Add($featureName)
	$global:featureIds = $feature.Id
	
	# Set NVIDIA GeForce NOW enabled games counters
	$global:FeatureInGame = 0
	$global:FeatureInGameAdded = 0
	$global:FeatureRemoved = 0
	
	# NVIDIA GeForce NOW compatible game list download and convert
	$NGFNowSupportedListUri = "https://static.nvidiagrid.net/supported-public-game-list/gfnpc.json"
	try {
		[array]$NGFNowSupportedList = Invoke-WebRequest $NGFNowSupportedListUri | ConvertFrom-Json | Where-Object {$_.status -eq "AVAILABLE"}

	} catch {
		$PlayniteApi.Dialogs.ShowMessage("Couldn't download NVIDIA Geforce NOW database file", "NVIDIA Geforce Now Compatibility Checker");
		exit
	}
	
	# Generate game names for matching and lists per store
	foreach ($SupportedGame in $NGFNowSupportedList) {
		$SupportedGame.title =  $SupportedGame.title -replace '[^\p{L}\p{Nd}]', ''
	}
	$NGFNowSupportedListSteam = $NGFNowSupportedList | Where-Object {$_.store -eq "Steam"}
	$NGFNowSupportedListEpic = $NGFNowSupportedList | Where-Object {$_.store -eq "Epic"}
	$NGFNowSupportedListUplay = $NGFNowSupportedList | Where-Object {$_.store -eq "UPLAY"}
	$NGFNowSupportedListOrigin = $NGFNowSupportedList | Where-Object {$_.store -eq "Origin"}
	
	foreach ($game in $GameDatabase) {
	
		# Generate game name for matching in lists
		$GameName = $($game.name) -replace '[^\p{L}\p{Nd}]', ''
		$FoundStatus = $false
		
		# Search for matches in support list
		switch ($game.source.name)
		{
			'Steam' {
				$SteamUrl = 'https://store.steampowered.com/app/' + "$($game.GameId)"
				foreach ($SupportedGame in $NGFNowSupportedListSteam) {
					if ($SupportedGame.SteamUrl -eq $SteamUrl) 
					{
						$FoundStatus = $true
						break
					}
				}
			}
			'Epic' {
				foreach ($SupportedGame in $NGFNowSupportedListEpic) {
					if ($SupportedGame.Title -eq $GameName) 
					{
						$FoundStatus = $true
						break
					}
				}
			}
			'Uplay' { 
				foreach ($SupportedGame in $NGFNowSupportedListUplay) {
					if ($SupportedGame.Title -eq $GameName) 
					{
						$FoundStatus = $true
						break
					}
				}
			}
			'Origin' { 
				foreach ($SupportedGame in $NGFNowSupportedListOrigin) {
					if ($SupportedGame.Title -eq $GameName) 
					{
						$FoundStatus = $true
						break
					}
				}
			}
			default {
				break
			}
		}

		# Invoke function to add tag
		Invoke-AddTag -Game $Game -FeatureName $featureName -FeatureIds $featureIds -FoundStatus $FoundStatus
	}

	# Generate report with numer of added games
	if ($FeatureInGameAdded -gt 0)
	{
		$AddedReport = "New NVIDIA Geforce Now enabled games found in library $FeatureInGameAdded"
	}
	else
	{
		$AddedReport = "No new games were added in this runtime"
	}
	
	# Show finish dialogue with results
	$PlayniteApi.Dialogs.ShowMessage("NVIDIA Geforce Now enabled games in library: $FeatureInGame`n$AddedReport`nNumber of removed games from NVIDIA Geforce Now: $FeatureRemoved", "NVIDIA Geforce Now Compatibility Checker");
}