function GetMainMenuItems
{
    param(
        $menuArgs
    )

    $ExtensionName = "NVIDIA Freestyle checker"
    
    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemCheckFreestyleEnabledGamesDescription")
    $menuItem1.FunctionName = "Update-IsFreestyleEnabled"
    $menuItem1.MenuSection = "@$ExtensionName"

    return $menuItem1
}

function Update-IsFreestyleEnabled
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $ExtensionName = "NVIDIA Freestyle checker"
    
    $featureName = "NVIDIA Freestyle"
    $feature = $PlayniteApi.Database.Features.Add($featureName)

    $FreestyleEnabled = 0
    $CounterFeatureAdded = 0
    
    try {
        $uri = "https://www.nvidia.com/es-la/geforce/geforce-experience/games/"
        $webClient = New-Object System.Net.WebClient
        $webClient.Encoding = [System.Text.Encoding]::UTF8
        $WebContent = $webClient.DownloadString($uri)
        $webClient.Dispose()
    } catch {
        $errorMessage = $_.Exception.Message
        $PlayniteApi.Dialogs.ShowErrorMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCNvidiaJsonDownloadFailErrorMessage") -f $errorMessage), $ExtensionName)
        exit
    }
    
    # Use regex to get supported games
    $regex = '(?:<div class="gameName(?:.*?(?=freestyle))freestyle(?:.*?(?=">))">\s+)([^\n]+)'
    $UrlMatches = ([regex]$regex).Matches($WebContent)

    [System.Collections.Generic.List[string]]$SupportedGames = @()
    foreach ($Match in $UrlMatches) {
        $SupportedGame = $Match.Groups[1].Value -replace '[^\p{L}\p{Nd}]', ''
        $SupportedGames.Add($SupportedGame)
    }
    if ($SupportedGames.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowErrorMessage([Playnite.SDK.ResourceProvider]::GetString("LOCNoSupportedGamesErrorMessage"), $ExtensionName)
        exit
    }

    $GameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.Platform.name -eq "PC"}
    foreach ($Game in $GameDatabase) {

        if ($Game.Features.Name -contains $featureName)
        {
            $FreestyleEnabled++
            continue
        }

        $GameName = $($Game.name) -replace '[^\p{L}\p{Nd}]', ''
        if ($SupportedGames -contains $GameName)
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

            # Update game in database
            $PlayniteApi.Database.Games.Update($Game)
            $__logger.Info("$ExtensionName - Feature added to `"$($Game.name)`"")
            $CounterFeatureAdded++
            $FreestyleEnabled++
        }
    }

    # Show finish dialogue with results
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCResultsMessage") -f $FreestyleEnabled, $featureName, $CounterFeatureAdded), $ExtensionName)
}