function global:SteamCloudSaveCheckAdd
{
    # Set Game database
    $GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.Features.name -notcontains "Cloud Saves"}

    # Set counters
    $TagExists = 0
    $TagAdded = 0
    
    # Create no cloud save tag
    $tagName = "No Cloud Saves"
    $tag = $PlayniteApi.Database.tags.Add($tagName)
    [guid[]]$tagIds = $tag.Id
    
    foreach ($game in $GameDatabase)
    {
        # Check if game already has the tag and skip if true
        if ($game.tags.name -eq "$tagName")
        {
            $TagExists++
            continue
        }
        else
        {
                
            # Add tag Id to game
            if ($game.tagIds) 
            {
                $game.tagIds += $tagIds
            }
            else 
            {
                # Fix in case game has null tagIds
                $game.tagIds = $tagIds
            }
            
            # Update game in database and increase counters
            $PlayniteApi.Database.Games.Update($game)
            $TagAdded++
        }
    }

    # Show finish dialogue with results
    $PlayniteApi.Dialogs.ShowMessage("`"$tagName`" tag added to $TagAdded games. `n$TagExists already had the tag");
}

function global:SteamCloudSaveCheckRemove
{
    # Set Game database
    $GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.tags.name -contains "No Cloud Saves"}

    # Set counters
    $TagRemoved = 0

    # Create no cloud save tag
    $tagName = "No Cloud Saves"
    $tag = $PlayniteApi.Database.tags.Add($tagName)
    [guid[]]$tagIds = $tag.Id

    foreach ($game in $GameDatabase) {
        $game.tagIds.Remove("$tagIds")
        $PlayniteApi.Database.Games.Update($game)
        $TagRemoved++
    }

    # Show finish dialogue with results
    $PlayniteApi.Dialogs.ShowMessage("`"$tagName`" removed from $TagRemoved games");
}