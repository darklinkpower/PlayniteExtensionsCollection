function GetMainMenuItems
{
    param(
        $menuArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Clean description of selected PC games"
    $menuItem1.FunctionName = "Invoke-FormatSelectedGameDescriptions"
    $menuItem1.MenuSection = "@Steam descriptions cleaner"
    
    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = "Clean description of all PC games in database"
    $menuItem2.FunctionName = "Invoke-FormatAllGameDescriptions"
    $menuItem2.MenuSection = "@Steam descriptions cleaner"
    
    return $menuItem1, $menuItem2
}

function Format-SteamDescription
{
    param (
        $game
    )
    
    $descriptionChangedCount = 0
    if ($null -eq $game.Description)
    {
        return $descriptionChangedCount
    }

    $regex = '(?:[\s\S]+)<h1>About the Game<\/h1>([\s\S]+)'
    $RegexMatch = ([regex]$regex).Matches($game.description)
    if ($RegexMatch.count -eq 1)
    {
        $game.description = '<h1>About the Game</h1>' + $RegexMatch.groups[1].value
        $PlayniteApi.Database.Games.Update($game)
        $descriptionChangedCount++
        $__logger.Info("Cleaned description of `"$($game.name)`"")
    }
    return $descriptionChangedCount
}

function Invoke-FormatGamesCollectionDescription
{
    param (
        $gameCollection
    )

    $descriptionChangedCount = 0
    foreach ($game in $gameCollection) {
        $descriptionChanged = Format-SteamDescription $gameCollection
        $descriptionChangedCount += $descriptionChanged
    }

    # Show finish dialogue with results
    $PlayniteApi.Dialogs.ShowMessage("Changed $descriptionChangedCount games description ", "Description Remove `"About Game`"")
}

function Invoke-FormatAllGameDescriptions
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $gameCollection = $PlayniteApi.Database.Games | Where-Object { ($_.description) -and ($_.platform.name -eq "PC") }
    Invoke-FormatGamesCollectionDescription $gameCollection
}

function Invoke-FormatSelectedGameDescriptions
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $gameCollection = $PlayniteApi.Mainview.Selectedgames | Where-Object { ($_.description) -and ($_.platform.name -eq "PC") }
    Invoke-FormatGamesCollectionDescription $gameCollection
}