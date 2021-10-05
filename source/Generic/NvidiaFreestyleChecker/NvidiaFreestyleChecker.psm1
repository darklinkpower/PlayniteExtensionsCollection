function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )
    
    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCNVIDIA_Freestyle_Checker_MenuItemCheckFreestyleEnabledGamesDescription")
    $menuItem1.FunctionName = "Update-IsFreestyleEnabled"
    $menuItem1.MenuSection = "@NVIDIA Freestyle checker"

    return $menuItem1
}

function Update-IsFreestyleEnabled
{
    param(
        $scriptMainMenuItemActionArgs
    )

    $feature = $PlayniteApi.Database.Features.Add("NVIDIA Freestyle")

    $freestyleEnabled = 0
    $counterFeatureAdded = 0
    
    try {
        $uri = "https://www.nvidia.com/es-la/geforce/geforce-experience/games/"
        $webClient = New-Object System.Net.WebClient
        $webClient.Encoding = [System.Text.Encoding]::UTF8
        $webContent = $webClient.DownloadString($uri)
        $webClient.Dispose()
    } catch {
        $webClient.Dispose()
        $errorMessage = $_.Exception.Message
        $PlayniteApi.Dialogs.ShowErrorMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCNVIDIA_Freestyle_Checker_NvidiaJsonDownloadFailErrorMessage") -f $errorMessage), "NVIDIA Freestyle checker")
        return
    }
    
    # Use regex to get supported games
    $regex = '(?:<div class="gameName(?:.*?(?=freestyle))freestyle(?:.*?(?=">))">\s+)([^\n]+)'
    $urlMatches = ([regex]$regex).Matches($webContent)

    [System.Collections.Generic.List[string]]$supportedGames = @()
    foreach ($match in $urlMatches) {
        $supportedGame = $match.Groups[1].Value -replace '[^\p{L}\p{Nd}]', ''
        $supportedGames.Add($SupportedGame)
    }
    if ($supportedGames.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowErrorMessage([Playnite.SDK.ResourceProvider]::GetString("LOCNVIDIA_Freestyle_Checker_NoSupportedGamesErrorMessage"), "NVIDIA Freestyle checker")
        return
    }

    $gameDatabase = $PlayniteApi.Database.Games
    foreach ($game in $GameDatabase) {
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
        }
        
        if ($game.Features.Name -contains $featureName)
        {
            $freestyleEnabled++
            continue
        }

        $gameName = $($game.name) -replace '[^\p{L}\p{Nd}]', ''
        if ($supportedGames -contains $gameName)
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

            # Update game in database
            $PlayniteApi.Database.Games.Update($game)
            $__logger.Info("Feature added to `"$($game.name)`"")
            $counterFeatureAdded++
            $freestyleEnabled++
        }
    }

    # Show finish dialogue with results
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCNVIDIA_Freestyle_Checker_ResultsMessage") -f $freestyleEnabled, $featureName, $counterFeatureAdded), "NVIDIA Freestyle checker")
}