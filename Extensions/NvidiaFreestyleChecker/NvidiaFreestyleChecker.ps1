function GetMainMenuItems
{
    param($menuArgs)

    $ExtensionName = "NVIDIA Freestyle checker"
    
    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Check for Freestyle supported games in library"
    $menuItem1.FunctionName = "Update-IsFreestyleEnabled"
    $menuItem1.MenuSection = "@$ExtensionName"

    return $menuItem1
}

function Update-IsFreestyleEnabled
{
    $ExtensionName = "NVIDIA Freestyle checker"
    
    $featureName = "NVIDIA Freestyle"
    $feature = $PlayniteApi.Database.Features.Add($featureName)

    $FreestyleEnabled = 0
    $CounterFeatureAdded = 0
    
    try {
        $Uri = "https://www.nvidia.com/es-la/geforce/geforce-experience/games/"
        $WebContent = Invoke-WebRequest $Uri
    } catch {
        $ErrorMessage = $_.Exception.Message
        $PlayniteApi.Dialogs.ShowErrorMessage("Couldn't download NVIDIA Freestyle database. Error: $ErrorMessage", $ExtensionName);
        exit
    }
    
    # Use regex to get supported games
    $regex = '(?:<div class="gameName(?:.*?(?=freestyle))freestyle(?:.*?(?=">))">\s+)([^\n]+)'
    $UrlMatches = ([regex]$regex).Matches($WebContent.Content)

    [System.Collections.Generic.List[string]]$SupportedGames = @()
    foreach ($Match in $UrlMatches) {
        $SupportedGame = $Match.Groups[1].Value -replace '[^\p{L}\p{Nd}]', ''
        $SupportedGames.Add($SupportedGame)
    }
    if ($SupportedGames.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowMessage("Error: Not found any freestyle enabled game", $ExtensionName);
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
    $Results = "NVIDIA Freestyle supported games in library: $FreestyleEnabled`n`nAdded `"$featureName`" feature to $CounterFeatureAdded games"
    $PlayniteApi.Dialogs.ShowMessage($Results, $ExtensionName);
}