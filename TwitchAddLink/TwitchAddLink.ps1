function global:GetMainMenuItems()
{
	param($menuArgs)

	$menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
	$menuItem1.Description = "Automatic mode"
	$menuItem1.FunctionName = "TwitchAddLinkAutomatic"
	$menuItem1.MenuSection = "@Twitch Add Link"

	$menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
	$menuItem2.Description = "Semi-Automatic mode"
	$menuItem2.FunctionName = "TwitchAddLinkSemiAutomatic"
	$menuItem2.MenuSection = "@Twitch Add Link"
	
	return $menuItem1, $menuItem2
}

function Add-TwitchLink() {
	param (
		[String]$ExecutionMode
	)
	
	# Set GameDatabase
	$GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.Links.Name -NotContains "Twitch"}
	
	# Set counters
	$CountLinkAdded = 0
	$CountRemaining = $GameDatabase.Count
	
	foreach ($game in $GameDatabase) {
		
		# Decrease remaining games counter
		$CountRemaining--
		
		# Download headers
		try {
			$GameName = $($game.name).Replace("™","").Replace("’","`'").Replace("?","%3F").Replace("#","%23")
			$Uri = 'https://static-cdn.jtvnw.net/ttv-boxart/' + "$GameName" + '.jpg'
			$webrequest = Invoke-WebRequest $Uri -Method Head
		} catch {
			$PlayniteApi.Dialogs.ShowMessage("Couldn't download information for `"$($game.name)`".`nExtension will stop execution.`n`nTwitch link added to $CountLinkAdded games.", "Twitch Add Link");
			exit
		}

		# Check for valid Url by checking redirect
		if (!$webrequest.Headers.'X-404-Redirect')
		{
			$TwitchUrl = 'https://www.twitch.tv/directory/game/' + "$GameName"
		}
		else
		{
			# Download headers with modified game name
			try {
				$GameName = ( Get-Culture ).TextInfo.ToTitleCase( $GameName.ToLower() ).Replace(" The "," the ").Replace(": the ",": The ").Replace(" A "," a ").Replace(": a ",": A ").Replace(" Of "," of ").Replace(" And "," and ").Replace(" At "," at ")
				$GameName = $GameName.Replace(" Xv"," XV").Replace(" Xiv"," XIV").Replace(" Xii"," XII").Replace(" Xii"," XII").Replace(" Xi"," XI").Replace(" Ix"," IX").Replace(" Viii"," VIII").Replace(" Vii"," VII").Replace(" Vi"," VI").Replace(" Iv"," IV").Replace(" Iii"," III").Replace(" Ii"," II")
				$Uri = 'https://static-cdn.jtvnw.net/ttv-boxart/' + "$GameName" + '.jpg'
				$webrequest = Invoke-WebRequest $Uri -Method Head
			} catch {
				$PlayniteApi.Dialogs.ShowMessage("Couldn't download information for `"$($game.name)`".`nExtension will stop execution.`n`nTwitch link added to $CountLinkAdded games.", "Twitch Add Link");
				exit
			}
			if (!$webrequest.Headers.'X-404-Redirect')
			{
				$TwitchUrl = 'https://www.twitch.tv/directory/game/' + "$GameName"
			}
		}
		if ( (!$TwitchUrl) -and ($ExecutionMode -eq "SemiAutomatic") )
		{
			# Open Twitch Search in Browser
			$SearchUrl = 'https://www.twitch.tv/search?term=' + "$($game.name)"
			Start-Process $SearchUrl
			
			# Request Manual Input of Url
			while ($True) {
				$InputUrl = $PlayniteApi.Dialogs.SelectString(
					"Game Twich Url couldn't be found automatically for `"$($game.name)`".`nEnter correct Url:"
					, "Twitch - Add Link", ""
				);
				
				# Check if input was entered
				if ("$($InputUrl.result)" -eq "True")
				{
					if ("$($InputUrl.Selectedstring)" -match '^https://www.twitch.tv/directory/game/.+$')
					{
						$TwitchUrl = "$($InputUrl.Selectedstring)"
						break
					}
					else
					{
						$PlayniteApi.Dialogs.ShowMessage("Invalid Url.");
					}
				}
				elseif ($CountRemaining -gt 0)
				{
					# Ask user if wants to continue execution for remaining games
					$UserContinue = $PlayniteApi.Dialogs.ShowMessage("Do you want to continue the extension execution for the remaining games?", "Twitch Add Link", 4)
					if ($UserContinue -eq "Yes")
					{
						break
					}
					else
					{
						$PlayniteApi.Dialogs.ShowMessage("Twitch link added to $CountLinkAdded games.", "Twitch Add Link");
						exit
					}
				}
				else
				{
					break
				}
			}
		}
		
		# Add Twitch link
		if ($TwitchUrl)
		{
			$Link = [Playnite.SDK.Models.Link]::New("Twitch", $TwitchUrl)
			if ($game.Links)
			{
				$game.Links += $Link
			}
			else
			{
				# Fix in case game has never had links
				$game.Links = $Link
			}

			# Update game in database and increase counters
			$PlayniteApi.Database.Games.Update($game)
			$CountLinkAdded++
			$TwitchUrl = $null
			$__logger.Info("Twitch Add Link - Added Twitch link to `"$($game.name)`"")
		}
	}
	# Show finish dialogue with results
	$PlayniteApi.Dialogs.ShowMessage("Twitch link added to $CountLinkAdded games.", "Twitch Add Link");
}

function TwitchAddLinkAutomatic() {
	$ExecutionMode = "Automatic"
	Add-TwitchLink -ExecutionMode $ExecutionMode
}

function TwitchAddLinkSemiAutomatic() {
	$ExecutionMode = "SemiAutomatic"
	Add-TwitchLink -ExecutionMode $ExecutionMode
}