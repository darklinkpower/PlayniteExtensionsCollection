function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemGetRatingSelectedGamesDescription")
    $menuItem1.FunctionName = "SteamDbRating"
    $menuItem1.MenuSection = "@SteamDB Rating"

    return $menuItem1
}

function SteamDbRating
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    # Set Url Templates
    $SteamApiReviewsTemplate =  "https://store.steampowered.com/appreviews/{0}?json=1&purchase_type=all&language=all"
    
    #Set GameDatabase
    $GameDatabase = $PlayniteApi.MainView.SelectedGames
    
    # Set Counters
    $CountScoreAdded = 0
    
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
                $steamApiSearchUrl = $SteamApiReviewsTemplate -f $AppId
                $webClient = New-Object System.Net.WebClient
                $webClient.Encoding = [System.Text.Encoding]::UTF8
                $downloadedString = $webClient.DownloadString($steamApiSearchUrl)
                $webClient.Dispose()
                $json = $downloadedString | ConvertFrom-Json
            } catch {
                $__logger.Warn("SteamDbRating - `"$($game.name)`" information couldn't be downloaded")
                continue
            }
            
            # Check if game has review information/still in Steam Store to grab data
            if ( ($json.success -eq 2) -or ( $json.query_summary.total_reviews -eq 0) )
            {
                $__logger.Warn("SteamDbRating - `"$($game.name)`" has been removed from Steam or doesn't have reviews")
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
        }
    }
    
    # Show finish dialogue with results
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCResultsMessage") -f $GameDatabase.Count, $CountScoreAdded), "SteamDB Rating")
}