function global:SteamGameImporter()
{
	# Set Log Path
	# $LogPath = Join-Path -Path $($PlayniteApi.Paths.ApplicationPath) -ChildPath "PlayniteExtensionTests.log"
	
	# Set Source
	$SourceName = "Steam"
	$Source = $PlayniteApi.Database.Sources.Add($SourceName)
	
	# Set Platform
	$PlatformName = "PC"
	$Platform = $PlayniteApi.Database.Platforms.Add($PlatformName)
	
	# Input window for Steam Store URL or Steam AppId
	$UserInput = $PlayniteApi.Dialogs.SelectString( "Enter Steam game Id or URL:", "Steam Game Importer", "");
	if ($UserInput.SelectedString)
	{
		#Verify if input was Steam Store URL
		if ( $($UserInput.SelectedString) -match "https?://store.steampowered.com/app/\d+/?\w*/?")
		{
			switch -regex ($UserInput.SelectedString) {
			"https?://store.steampowered.com/app/(\d+)/?\w*/?" {
			$AppId = $matches[1]}
			}
		}
		#Verify if input was Steam Store AppId
		elseif ( ($AppId -eq $null) -and (($UserInput.SelectedString) -match '^\d+$') )
		{
			$AppId = "$($UserInput.SelectedString)"
		}
		#Verify if AppId was obtained
		if ($AppId)
		{
			# Verify is obtained AppId is valid and get game name with SteamAPI
			try {
				$SteamAPI = 'https://store.steampowered.com/api/appdetails?appids=' + "$AppId"
				$json = Invoke-WebRequest -uri $SteamAPI -TimeoutSec '10' | ConvertFrom-Json
				if ($json.$AppId.Success -eq "true")
				{
					$GameName = $json.$AppId.data.name
				}
				else
				{
					$PlayniteApi.Dialogs.ShowMessage("Not a valid id or URL. Please input a valid steam id number or URL", "Steam Game Importer");
					exit
				}
			} catch {
				$ErrorMessage = $_.Exception.Message
				$PlayniteApi.Dialogs.ShowMessage("Couldn't download Game information. Error: $ErrorMessage", "Steam Game Importer");
				exit
			}
			
			# Set game properties and save to database
			$newGame = New-Object "Playnite.SDK.Models.Game"
			$newGame.Name = "$GameName"
			$newGame.GameId = "$($AppId)"
			$newGame.SourceId = "$($Source.Id)"
			$newGame.PlatformId = "$($Platform.Id)"
			$newGame.PluginId = "CB91DFC9-B977-43BF-8E70-55F46E410FAB"
			$PlayniteApi.Database.Games.Add($newGame)
			
			# Trigger download Metadata not available yet via SDK. https://github.com/JosefNemec/Playnite/issues/1870
			
			#Show dialogue with report
			$PlayniteApi.Dialogs.ShowMessage("`"$GameName`" added to Playnite.", "Steam Game Importer");
		}
		else
		{
			# Show error message if Steam AppId was not obtained
			$PlayniteApi.Dialogs.ShowMessage("Not a valid id or URL. Please input a valid steam id number or URL", "Steam Game Importer");
		}
	}
}

function global:SteamGameImporterUserData()
{
	# Set Source
	$SourceName = "Steam"
	$Source = $PlayniteApi.Database.Sources.Add($SourceName)
	
	# Set Platform
	$PlatformName = "PC"
	$Platform = $PlayniteApi.Database.Platforms.Add($PlatformName)

	# Create cache of Steam games in Database
	$SteamGames = $PlayniteApi.Database.Games | Where-Object {$_.source.name -eq "Steam"}
	[System.Collections.Generic.List[string]]$SteamGamesInDatabase = @()
	foreach ($game in $SteamGames) {
		$SteamGamesInDatabase.Add($($game.GameId))
	}

	
	# Use Webview to log in to Steam
	$PlayniteApi.Dialogs.ShowMessage("Login to Steam to continue", "Steam Game Importer");
	$webView = $PlayniteApi.WebViews.CreateView(1020, 600)
	$webView.Navigate('https://steamcommunity.com/login/home/')
	$webView.OpenDialog()
	$webView.Close()

	try {
		# Download Steam User Data
		$webView = $PlayniteApi.WebViews.CreateOffscreenView()
		$webView.NavigateAndWait('https://store.steampowered.com/dynamicstore/userdata/')
		$SteamUserDataSource = $webView.GetPageSource() -replace '<html><head></head><body><pre style="word-wrap: break-word; white-space: pre-wrap;">','' -replace '</pre></body></html>',''
		$webView.Close()

		# Convert Json
		$SteamUserData = $SteamUserDataSource | ConvertFrom-Json
	} catch {
		$ErrorMessage = $_.Exception.Message
		$PlayniteApi.Dialogs.ShowMessage("Couldn't download Steam User Data. Error: $ErrorMessage", "Steam Game Importer");
		exit
	}

	# Get cache of AppIds that are not games
	$ExtensionPath = Join-Path -Path $PlayniteApi.Paths.ExtensionsDataPath -ChildPath 'SteamGameImporter'
	$CacheSteamNoGamesPath = Join-Path -Path $ExtensionPath -ChildPath 'Cache.txt'
	if (!(Test-Path $ExtensionPath))
	{
		New-Item -ItemType Directory -Path $ExtensionPath -Force
	}
	if (Test-Path $CacheSteamNoGamesPath)
	{
		[System.Collections.Generic.List[string]]$CacheSteamNoGames = [System.IO.File]::ReadAllLines($CacheSteamNoGamesPath)
	}
	$AddedGamesCount = 0

	foreach ($OwnedAppId in $SteamUserData.rgOwnedApps) {
		if ( ($SteamGamesInDatabase -contains $OwnedAppId) -or ($CacheSteamNoGames -contains $OwnedAppId) )
		{
			continue
		}
		else
		{
			try {
				$SteamApi = 'https://store.steampowered.com/api/appdetails?appids={0}' -f $OwnedAppId
				$Json = Invoke-WebRequest -uri $SteamApi -TimeoutSec '10' | ConvertFrom-Json
			} catch {
				$ErrorMessage = $_.Exception.Message
				$PlayniteApi.Dialogs.ShowMessage("Couldn't download Owned App Data. Error: $ErrorMessage", "Steam Game Importer");
				exit
			}
		}
		if ($Json.$OwnedAppId.data.type -eq "game")
		{
			# Create game in database
			$newGame = New-Object "Playnite.SDK.Models.Game"
			$newGame.Name = $Json.$OwnedAppId.data.name
			$newGame.GameId = $OwnedAppId
			$newGame.SourceId = "$($Source.Id)"
			$newGame.PlatformId = "$($Platform.Id)"
			$newGame.PluginId = "CB91DFC9-B977-43BF-8E70-55F46E410FAB"
			$PlayniteApi.Database.Games.Add($newGame)
			$AddedGamesCount++
		}
		else
		{
			"$OwnedAppId" | Out-File -Encoding 'UTF8' -FilePath $CacheSteamNoGamesPath -Append
		}

		# Sleep time to prevent error 429
		Start-Sleep -Seconds 1
	}

	# Finish dialogue with results
	$PlayniteApi.Dialogs.ShowMessage("Imported $AddedGamesCount games", "Steam Game Importer");
}