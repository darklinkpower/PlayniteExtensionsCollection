function SteamDbRating() {
	# Set Log Path
	# $LogPath = Join-Path $PlayniteApi.Paths.ApplicationPath -ChildPath "PlayniteExtensionTests.log"
	
	# Set Url Templates
	$SteamApiReviewsTemplate =  "https://store.steampowered.com/appreviews/{0}?json=1&purchase_type=all&language=all"
	
	#Set GameDatabase
	$GameDatabase = $PlayniteApi.MainView.SelectedGames
	
	# Set Counters
	$CountScoreAdded = 0
	$CountErrors = 0
	
	foreach ($game in $GameDatabase) {
		#Clear variables
		$AppId = $null
		
		# Check if it's a Steam Game to obtain AppId
		if ($game.Source.name -eq "Steam")
		{
			# Use GameId for Steam games
			$AppId = $game.GameId
		}
		else
		{
			# Look for Steam Store URL in links for other games
			foreach ($link in $game.Links) {
			switch -regex ($link.Url) {
				"https?://store.steampowered.com/app/(\d+)\S*" {
				$AppId = $matches[1]}
				}
			}
		}
		
		# Continue only if AppId was obtained
		if ($AppId)
		{
			# Set Steam Db url and web request
			try {
				$SteamApiSearchUrl = $SteamApiReviewsTemplate -f $AppId
				$json = Invoke-WebRequest -Uri $SteamApiSearchUrl | ConvertFrom-Json
			} catch {
				$__logger.Warn("SteamDbRating - `"$($game.name)`" information couldn't be downloaded")
				$CountErrors++
				continue
			}
			
			# Check if game has review information/still in Steam Store to grab data
			if ( ($json.success -eq 2) -or ( $json.query_summary.total_reviews -eq 0) )
			{
				$__logger.Warn("SteamDbRating - `"$($game.name)`" has been removed from Steam or doesn't have reviews")
				$CountErrors++
				continue
			}
			
			# Get data from json and do operations
			$VotesPositive = $json.query_summary.total_positive
			$VotesNegative = $json.query_summary.total_negative
			$VotesTotal = $VotesPositive + $VotesNegative
			$Average = $VotesPositive / $VotesTotal
			$SteamDbRating = ( $Average - ( $Average - 0.5 ) * ( [Math]::Pow(2,-([Math]::Log10( $VotesTotal + 1 ))) ) ) * 100
			$GameRating = [math]::Round($SteamDbRating)
			
			# Verify if obtained rating was more than 0 and doesn't match current game community score to prevent unnecesary actions.
			if ( ($GameRating -gt 0) -and ($GameRating -ne $($game.CommunityScore)) )
			{
				$game.CommunityScore = $GameRating
				$PlayniteApi.Database.Games.Update($game)
				$CountScoreAdded++
			}
		}
		else
		{
			# Log Error if no Steam AppId and no steam link for games from other sources
			$__logger.Warn("SteamDbRating - `"$($game.name)`" no SteamId found to download information")
			$CountErrors++
			continue
		}
	}
	
	# Error log
	if ($CountErrors -gt 0)
	{
		$ErrorReport = "There were $CountErrors errors. Check Playnite log for details."
	}
	else
	{
		$ErrorReport = "There were no errors"
	}
	
	# Show finish dialogue with results
	$PlayniteApi.Dialogs.ShowMessage("Number of processed games: $($GameDatabase.Count)`n`nGames that had a new Community Score added: $CountScoreAdded`n$ErrorReport", "SteamDB Rating - Results");
}