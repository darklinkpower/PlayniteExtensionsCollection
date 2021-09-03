function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $ExtensionName = "Nexus Mods checker"
    
    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemAddLinkSelectedGamesDescription")
    $menuItem1.FunctionName = "Invoke-AddToSelectedGames"
    $menuItem1.MenuSection = "@$ExtensionName"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemAddLinkAllGamesDescription")
    $menuItem2.FunctionName = "Invoke-AddToAllGames"
    $menuItem2.MenuSection = "@$ExtensionName"

    return $menuItem1, $menuItem2
}

function Invoke-AddToSelectedGames
{
    param(
        $scriptMainMenuItemActionArgs
    )

    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    Add-NexusFeatureLinks $gameDatabase
}

function Invoke-AddToAllGames
{
    param(
        $scriptMainMenuItemActionArgs
    )

    $gameDatabase = $PlayniteApi.Database.Games
    Add-NexusFeatureLinks $gameDatabase
}

function Get-DownloadString
{
    param (
        $url
    )
    
    try {
        $webClient = New-Object System.Net.WebClient
        $webClient.Encoding = [System.Text.Encoding]::UTF8
        $DownloadedString = $webClient.DownloadString($url)
        $webClient.Dispose()
        return $DownloadedString
    } catch {
        $errorMessage = $_.Exception.Message
        $__logger.Info("Error downloading file `"$url`". Error: $errorMessage")
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCGenericFileDownloadError") -f $url, $errorMessage))
        return
    }
}

function Add-NexusFeatureLinks
{
    param(
        $gameDatabase
    )

    $ExtensionName = "Nexus Mods checker"
    
    if ($gameDatabase.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCNoGamesInSelectionMessage"), $ExtensionName)
    }

    $featureName = "Nexus Mods"
    $feature = $PlayniteApi.Database.Features.Add($featureName)
    
    $webContent = Get-DownloadString "https://www.nexusmods.com/games"
    if ($null -eq $webContent)
    {
        return
    }
    $webContent -match 'var json = ((.*?(?=}]))}])'
    if ($matches)
    {
        $nexusGames = $matches[1] | ConvertFrom-Json
        foreach ($nexusGame in $nexusGames) {
            $nexusGame.name = $nexusGame.name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
        }
    }
    else
    {
        $PlayniteApi.Dialogs.ShowErrorMessage([Playnite.SDK.ResourceProvider]::GetString("LOCNoGamesFoundInNexusErrorMessage"), $ExtensionName)
        exit
    }

    $modsAvailable = 0
    $CounterFeatureAdded = 0
    $nexusLinkAdded = 0
    foreach ($game in $gameDatabase) {
        if ($null -eq $game.Platforms)
        {
            continue
        }
        else
        {
            $isTargetSpecification = $false
            foreach ($platform in $game.Platforms) {
                if ($null -eq $platform.SpecificationId)
                {
                    continue
                }
                if ($plaform.SpecificationId -eq "pc_windows")
                {
                    $isTargetSpecification = $true
                    break
                }
            }
            if ($isTargetSpecification -eq $false)
            {
                continue
            }
        }
        
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
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCResultsMessage") -f $modsAvailable, $featureName, $CounterFeatureAdded, $nexusLinkAdded), $ExtensionName)
}