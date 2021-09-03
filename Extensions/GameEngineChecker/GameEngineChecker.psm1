function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $ExtensionName = "Game Engine Checker"
    
    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCGame_Engine_Checker_MenuItemAddTagSelectedGamesDescription")
    $menuItem1.FunctionName = "Add-EngineTag"
    $menuItem1.MenuSection = "@$ExtensionName"

    return $menuItem1
}

function Add-EngineTag
{
    param(
        $scriptMainMenuItemActionArgs
    )

    $ExtensionName = "Game Engine Checker"
    $pcgwApiTemplateSteam = "https://www.pcgamingwiki.com/w/api.php?action=askargs&conditions=Steam+AppID::{0}&printouts=Uses_engine|Unity_engine_build&format=json"
    $pcgwApiTemplateGog = "https://www.pcgamingwiki.com/w/api.php?action=askargs&conditions=GOGcom+ID::{0}&printouts=Uses_engine|Unity_engine_build&format=json"
    $CountertagAdded = 0
   
    $steamAppListPath = Join-Path -Path $env:TEMP -ChildPath 'SteamAppList.json'
    if (Test-Path $steamAppListPath)
    {
        $AppListLastWrite = (get-item $steamAppListPath).LastWriteTime
        $TimeSpan = new-timespan -days 1
        if (((get-date) - $AppListLastWrite) -gt $TimeSpan)
        {
            Get-SteamAppList -AppListPath $steamAppListPath
        }
    }
    else
    {
        Get-SteamAppList -AppListPath $steamAppListPath
    }
    [object]$steamAppList = [System.IO.File]::ReadAllLines($steamAppListPath) | ConvertFrom-Json
    
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
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

        if ($null -ne $game.Tags)
        {
            $engineTagPresent = $false
            foreach ($tag in $game.Tags) {
                if ($tag -match "^[Engine]")
                {
                    break
                }
            }
            if ($engineTagPresent -eq $true)
            {
                continue
            }
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
            foreach ($steamApp in $steamAppList) {
                if ($steamApp.name -eq $gameName) 
                {
                    $steamAppId = $steamApp.appid
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
            $webClient = New-Object System.Net.WebClient
            $webClient.Encoding = [System.Text.Encoding]::UTF8
            $downloadedString = $webClient.DownloadString($uri)
            $webClient.Dispose()
            $gameInfo = $DownloadedString | ConvertFrom-Json
        } catch {
            $ErrorMessage = $_.Exception.Message
            $PlayniteApi.Dialogs.ShowErrorMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCGame_Engine_Checker_DownloadErrorMessage") -f $game.name, $ErrorMessage), $ExtensionName)
            break
        }

        $printouts = $gameInfo.query.results[0].PSObject.Properties.Value.printouts
        if ($printouts.Uses_engine.Count -gt 0)
        {
            $engineName = $printouts.Uses_engine[0].fulltext -replace "Engine:", "[Engine] "
            if (($engineName -eq "[Engine] Unity") -and ($printouts.Unity_engine_build.Count -gt 0))
            {
                $engineName = "[Engine] Unity {0}" -f $printouts.Unity_engine_build[-1].split('.')[0]
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
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCGame_Engine_Checker_ResultsMessage") -f $CountertagAdded), $ExtensionName)
}

function Get-SteamAppList
{
    param (
        [string]$steamAppListPath
    )

    $ExtensionName = "Game Engine Checker"

    try {
        $uri = 'https://api.steampowered.com/ISteamApps/GetAppList/v2/'
        $webClient = New-Object System.Net.WebClient
        $webClient.Encoding = [System.Text.Encoding]::UTF8
        $downloadedString = $webClient.DownloadString($uri)
        $webClient.Dispose()
        [array]$AppListContent = ($downloadedString | ConvertFrom-Json).applist.apps
        foreach ($steamApp in $AppListContent) {
            $steamApp.name = $steamApp.name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
        }
        
        ConvertTo-Json $AppListContent -Depth 2  -Compress | Out-File -Encoding 'UTF8' -FilePath $steamAppListPath
        $__logger.Info("$ExtensionName - Downloaded AppList")
    } catch {
        $ErrorMessage = $_.Exception.Message
        $__logger.Error("$ExtensionName - Error downloading Steam AppList database. Error: $ErrorMessage")
        $PlayniteApi.Dialogs.ShowErrorMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCGame_Engine_Checker_SteamAppListDownloadErrorMessage") -f $ErrorMessage), $ExtensionName)
        exit
    }
}