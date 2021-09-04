function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCLinks_Sorter_MenuItemFormatSelectedDescription")
    $menuItem1.FunctionName = "Format-SelectedGames"
    $menuItem1.MenuSection = "@Links Sorter"
    
    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCLinks_Sorter_MenuItemFormatAllDescription")
    $menuItem2.FunctionName = "Format-AllGames"
    $menuItem2.MenuSection = "@Links Sorter"
    
    return $menuItem1, $menuItem2
}

function Format-Links
{
    param (
        $GameDatabase
    )

    $SortedGames = 0
    foreach ($Game in $GameDatabase) {
        $SortedLinks = $Game.Links | Sort-Object -Property @{Expression = "Url"; Descending = $false}
        $SortedUrlOrder = $SortedLinks | Select-Object -Property url | Out-String
        $OriginalUrlOrder = $Game.Links | Select-Object -Property url | Out-String
        if ($OriginalUrlOrder -ne $SortedUrlOrder)
        {
            $Game.Links = $SortedLinks
            $PlayniteApi.Database.Games.Update($game)
            $SortedGames++
        }
    }
    
    # Show finish dialogue with shortcut creation count
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCLinks_Sorter_ResultsMessage") -f $SortedGames), "Links Sorter");
}

function Format-SelectedGames
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    # Set GameDatabase
    $GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.Links}
    
    Format-Links $GameDatabase
}

function Format-AllGames
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    # Set GameDatabase
    $GameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.Links}
    
    Format-Links $GameDatabase
}