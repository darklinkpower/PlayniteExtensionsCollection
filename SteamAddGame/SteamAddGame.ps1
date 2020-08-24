function global:SteamAddGame()
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
	$UserInput = $PlayniteApi.Dialogs.SelectString( "Enter Steam game Id or URL:", "Steam - Add game", "");
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
					$PlayniteApi.Dialogs.ShowMessage("Not a valid id or URL. Please input a valid steam id number or URL");
					exit
				}
			} catch {
				$PlayniteApi.Dialogs.ShowMessage("Couldn't download Game information.");
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
			$PlayniteApi.Dialogs.ShowMessage("`"$GameName`" added to Playnite.");
		}
		else
		{
			# Show error message if Steam AppId was not obtained
			$PlayniteApi.Dialogs.ShowMessage("Not a valid id or URL. Please input a valid steam id number or URL");
		}
	}
}