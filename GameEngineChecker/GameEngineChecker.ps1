function GetMainMenuItems
{
    param($menuArgs)

    $ExtensionName = "Game Engine Checker"
    
    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Add game engine tag to selected games"
    $menuItem1.FunctionName = "Add-EngineTag"
    $menuItem1.MenuSection = "@$ExtensionName"

    return $menuItem1
}

function Add-EngineTag
{
    $ExtensionName = "Game Engine Checker"
    $pcgwApiTemplateSteam = "https://www.pcgamingwiki.com/w/api.php?action=askargs&conditions=Steam+AppID::{0}&printouts=Uses_engine|Unity_engine_build&format=json"
    $pcgwApiTemplateGog = "https://www.pcgamingwiki.com/w/api.php?action=askargs&conditions=GOGcom+ID::{0}&printouts=Uses_engine|Unity_engine_build&format=json"
    $CountertagAdded = 0
   
    $AppListPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'AppList.json'
    if (Test-Path $AppListPath)
    {
        $AppListLastWrite = (get-item $AppListPath).LastWriteTime
        $TimeSpan = new-timespan -days 1
        if (((get-date) - $AppListLastWrite) -gt $TimeSpan)
        {
            Get-SteamAppList -AppListPath $AppListPath
        }
    }
    else
    {
        Get-SteamAppList -AppListPath $AppListPath
    }
    [object]$AppList = [System.IO.File]::ReadAllLines($AppListPath) | ConvertFrom-Json
    
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.Platform.name -eq "PC"}
    foreach ($game in $gameDatabase) {

        if ($game.tags.Name -match "Engine: ")
        {
            continue
        }

        $gameLibraryPlugin = [Playnite.SDK.BuiltinExtensions]::GetExtensionFromId($game.PluginId)
        if ($gameLibraryPlugin -eq 'SteamLibrary')
        {
            $uri = $pcgwApiTemplateSteam -f $game.GameId
        }
        elseif ($gameLibraryPlugin -eq 'GogLibrary')
        {
            $uri = $pcgwApiTemplateGog -f $game.GameId
        }
        else
        {
            $steamAppId = $null
            $gameName = $game.name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
            foreach ($SteamApp in $AppList) {
                if ($SteamApp.name -eq $gameName) 
                {
                    $steamAppId = $SteamApp.appid
                    break
                }
            }
            if ($null -eq $steamAppId)
            {
                $__logger.Info("$ExtensionName - SteamId of `"$($game.name)`" not found.")
                continue
            }
            $uri = $pcgwApiTemplateSteam -f $steamAppId
        }

        try {
            $gameInfo = Invoke-WebRequest $Uri | ConvertFrom-Json
        } catch {
            $ErrorMessage = $_.Exception.Message
            $__logger.Error("$ExtensionName - Couldn't download game information of `"$($game.name)`" from PCGW. Error: `"$ErrorMessage`".")
            $PlayniteApi.Dialogs.ShowErrorMessage("Couldn't download game information of `"$($game.name)`" from PCGW. Error: `"$ErrorMessage`"", $ExtensionName);
            break
        }

        $printouts = $gameInfo.query.results[0].PSObject.Properties.Value.printouts
        if ($printouts.Uses_engine.Count -gt 0)
        {
            $engineName = $printouts.Uses_engine[0].fulltext -replace "Engine:", "Engine: "
            if (($engineName -eq "Engine: Unity") -and ($printouts.Unity_engine_build.Count -gt 0))
            {
                $engineName = "Engine: Unity {0}" -f $printouts.Unity_engine_build[-1].split('.')[0]
            }
        }
        else
        {
            $__logger.Info("$ExtensionName - `"$($game.name)`" doesn't have Engine information in PCGW.")
            continue
        }

        $tag = $PlayniteApi.Database.Tags.Add($engineName)
    
        if ($game.tagIds -notcontains $tag.Id)
        {
            # Add tag Id to game
            if ($game.tagIds)
            {
                $game.tagIds += $tag.Id
            }
            else
            {
                # Fix in case game has null tagIds
                $game.tagIds = $tag.Id
            }
            
            # Update game in database and increase no media count
            $PlayniteApi.Database.Games.Update($game)
            $__logger.Info("$ExtensionName - Added `"$tagName`" engine tag to `"$($game.name)`".")
            $CountertagAdded++
        }
    }

    # Show finish dialogue with results
    $Results = "Finished.`nAdded engine tag to $CountertagAdded games."
    $PlayniteApi.Dialogs.ShowMessage($Results, $ExtensionName);
}

function Get-SteamAppList
{
    param (
        [string]$AppListPath
    )

    $ExtensionName = "Game Engine Checker"

    try {
        $Uri = 'https://api.steampowered.com/ISteamApps/GetAppList/v2/'
        [array]$AppListContent = (Invoke-WebRequest $Uri | ConvertFrom-Json).applist.apps
        foreach ($SteamApp in $AppListContent) {
            $SteamApp.name = $SteamApp.name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
        }
        
        ConvertTo-Json $AppListContent -Depth 2  -Compress | Out-File -Encoding 'UTF8' -FilePath $AppListPath
        $__logger.Info("$ExtensionName - Downloaded AppList")
    } catch {
        $ErrorMessage = $_.Exception.Message
        $__logger.Error("$ExtensionName - Error downloading Steam AppList database. Error: $ErrorMessage")
        $PlayniteApi.Dialogs.ShowErrorMessage("Error downloading Steam AppList database. Error: $ErrorMessage", "Steam Trailers");
        exit
    }
}