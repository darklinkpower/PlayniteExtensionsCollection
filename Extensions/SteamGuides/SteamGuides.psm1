function Add-SteamGuides()
{
    # Set GameDatabase
    $GameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.Source.Name -eq "Steam"}
    $Counter = 0
    foreach ($Game in $GameDatabase) {
        if ($Game.Links.Name -contains "Guides")
        {
            continue
        }
        $Url = "https://steamcommunity.com/app/{0}/guides/" -f $Game.GameId
        $Link = [Playnite.SDK.Models.Link]::New("Guides", $Url)
        if ($Game.Links)
        {
            $Game.Links.Add($Link)
        }
        else
        {
            # Fix in case game has null property
            $Game.Links = $Link
        }
        $PlayniteApi.Database.Games.Update($game)
        $Counter++
    }
    $PlayniteApi.Dialogs.ShowMessage("Added Guides link to $Counter games", "Links Sorter");
}