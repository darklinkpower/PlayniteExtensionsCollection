function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $ExtensionName = "Nexus Mods checker"
    
    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCNexus_Mods_Checker_MenuItemAddLinkSelectedGamesDescription")
    $menuItem1.FunctionName = "Invoke-AddToSelectedGames"
    $menuItem1.MenuSection = "@$ExtensionName"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCNexus_Mods_Checker_MenuItemAddLinkAllGamesDescription")
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
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCNexus_Mods_Checker_GenericFileDownloadError") -f $url, $errorMessage))
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
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCNexus_Mods_Checker_NoGamesInSelectionMessage"), $ExtensionName)
    }

    $featureName = "Nexus Mods"
    $feature = $PlayniteApi.Database.Features.Add($featureName)
    
    $webContent = Get-DownloadString "https://data.nexusmods.com/file/nexus-data/games.json"
    if ($null -eq $webContent)
    {
        return
    }

    
    $nexusData = $webContent | ConvertFrom-Json
    if ($nexusData.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowErrorMessage([Playnite.SDK.ResourceProvider]::GetString("LOCNexus_Mods_Checker_NoGamesFoundInNexusErrorMessage"), $ExtensionName)
        return
    }

    $nexusModsGames = @{}
    foreach ($nexusGame in $nexusData) {
        $gameName = $nexusGame.name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
        if ($nexusModsGames.ContainsKey($gameName) -eq $false)
        {
            $nexusModsGames.Add($gameName, $nexusGame.nexusmods_url)
        }
    }

    $modsAvailable = 0
    $CounterFeatureAdded = 0
    $nexusLinkAdded = 0

    $PlayniteApi.Database.BeginBufferUpdate()
    try
    {
        foreach ($game in $gameDatabase) {
            if ($null -eq $game.Platforms)
            {
                continue
            }
    
            $isTargetSpecification = $false
            foreach ($platform in $game.Platforms) {
                if ($null -eq $platform.SpecificationId)
                {
                    continue
                }
    
                if ($platform.SpecificationId -eq "pc_windows")
                {
                    $isTargetSpecification = $true
                    break
                }
            }
    
            if ($isTargetSpecification -eq $false)
            {
                continue
            }
            
            $gameNameMatching = $game.Name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
            if ($nexusModsGames.ContainsKey($gameNameMatching) -eq $false)
            {
                continue;
            }
    
            $gameUpdated = $false
            $modsAvailable++
            if ($game.FeatureIds -notcontains $feature.Id)
            {
                # Add feature Id to game
                if ($game.FeatureIds)
                {
                    $game.FeatureIds += $feature.Id
                }
                else
                {
                    # Fix in case game has null FeatureIds
                    $game.FeatureIds = $feature.Id
                }
    
                $PlayniteApi.Database.Games.Update($game)
                $__logger.Info("$ExtensionName - Feature added to `"$($game.name)`"")
                $gameUpdated = $true
                $CounterFeatureAdded++
            }
    
            $link = [Playnite.SDK.Models.Link]::New("Nexus Mods", $nexusModsGames[$gameNameMatching])
            if ($null -eq $game.Links)
            {
                $game.Links = $link
                $__logger.Info("$ExtensionName - Link added to `"$($game.name)`"")
                $nexusLinkAdded++
                $gameUpdated = $true
            }
            elseif ($game.Links.Name -notcontains "Nexus Mods")
            {
                $game.Links.Add($link)
                $__logger.Info("$ExtensionName - Link added to `"$($game.name)`"")
                $nexusLinkAdded++
                $gameUpdated = $true
            }
    
            if ($gameUpdated -eq $true)
            {
                $PlayniteApi.Database.Games.Update($game)
            }
        }
    }
    finally
    {
        $PlayniteApi.Database.EndBufferUpdate()
    }

    # Show finish dialogue with results
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCNexus_Mods_Checker_ResultsMessage") -f $modsAvailable, $featureName, $CounterFeatureAdded, $nexusLinkAdded), $ExtensionName)
}