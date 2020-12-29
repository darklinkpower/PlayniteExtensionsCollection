function GetMainMenuItems
{
    param(
        $menuArgs
    )

    $ExtensionName = "Nexus Mods checker"
    
    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Add Nexus Mods feature and links to selected games"
    $menuItem1.FunctionName = "Invoke-AddToSelectedGames"
    $menuItem1.MenuSection = "@$ExtensionName"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = "Add Nexus Mods feature and links to all games"
    $menuItem2.FunctionName = "Invoke-AddToAllGames"
    $menuItem2.MenuSection = "@$ExtensionName"

    return $menuItem1, $menuItem2
}

function Invoke-AddToSelectedGames
{
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.Platform.name -eq "PC"}
    Add-NexusFeatureLinks $gameDatabase
}

function Invoke-AddToAllGames
{
    $gameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.Platform.name -eq "PC"}
    Add-NexusFeatureLinks $gameDatabase
}

function Add-NexusFeatureLinks
{
    param(
        $gameDatabase
    )

    $ExtensionName = "Nexus Mods checker"
    
    if ($gameDatabase.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowMessage("There are no games in selection", $ExtensionName);
    }

    $featureName = "Nexus Mods"
    $feature = $PlayniteApi.Database.Features.Add($featureName)

    $uri = "https://www.nexusmods.com/games"
    try {
        $webContent = Invoke-WebRequest $uri
    } catch {
        $ErrorMessage = $_.Exception.Message
        $PlayniteApi.Dialogs.ShowErrorMessage("Couldn't download file. Error: $ErrorMessage", $ExtensionName);
        exit
    }
    $webContent.RawContent -match 'var json = ((.*?(?=}];))}])'
    if ($matches)
    {
        $nexusGames = $matches[1] | ConvertFrom-Json
        foreach ($nexusGame in $nexusGames) {
            $nexusGame.name = $nexusGame.name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
        }
    }
    else
    {
        $PlayniteApi.Dialogs.ShowErrorMessage("No games where found on Nexus", $ExtensionName);
        exit
    }

    $modsAvailable = 0
    $CounterFeatureAdded = 0
    $nexusLinkAdded = 0
    foreach ($game in $gameDatabase) {

        $gameNameMatching = $game.Name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
        foreach ($nexusGame in $nexusGames) {
            if ($nexusGame.name -eq $gameNameMatching)
            {
                $modsAvailable++
                
                if ($game.FeatureIds -notcontains $feature.Id)
                {
                    # Add feature Id to game
                    if ($Game.FeatureIds)
                    {
                        $Game.FeatureIds += $feature.Id
                    }
                    else
                    {
                        # Fix in case game has null FeatureIds
                        $Game.FeatureIds = $feature.Id
                    }
                    $PlayniteApi.Database.Games.Update($game)
                    $__logger.Info("$ExtensionName - Feature added to `"$($game.name)`"")
                    $CounterFeatureAdded++
                }

                if ($game.Links)
                {
                    if ($game.Links.Name -notcontains "Nexus Mods")
                    {
                        $link = [Playnite.SDK.Models.Link]::New("Nexus Mods", $nexusGame.nexusmods_url)
                        $game.Links.Add($link)
                        $PlayniteApi.Database.Games.Update($game)
                        $__logger.Info("$ExtensionName - Link added to `"$($game.name)`"")
                        $nexusLinkAdded++
                    }
                }
                else
                {
                    $link = [Playnite.SDK.Models.Link]::New("Nexus Mods", $nexusGame.nexusmods_url)
                    $game.Links = $link
                    $PlayniteApi.Database.Games.Update($game)
                    $__logger.Info("$ExtensionName - Link added to `"$($game.name)`"")
                    $nexusLinkAdded++
                }

                break
            }
        }
    }

    # Show finish dialogue with results
    $Results = "Games with mods available on Nexus Mods: $modsAvailable`n`nAdded `"$featureName`" feature to $CounterFeatureAdded games.`nAdded Nexus Mods links to $nexusLinkAdded games."
    $PlayniteApi.Dialogs.ShowMessage($Results, $ExtensionName);
}