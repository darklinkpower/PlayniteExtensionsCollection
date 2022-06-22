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
    $pcgwApiTemplateSteam = "https://www.pcgamingwiki.com/w/api.php?action=cargoquery&tables=Infobox_game&fields=Engines%2C_pageName%3Dtitle&where=Steam_AppID%20HOLDS%20%22{0}%22&format=json"
    $pcgwApiTemplateGog = "https://www.pcgamingwiki.com/w/api.php?action=cargoquery&tables=Infobox_game&fields=Engines%2C_pageName%3Dtitle&where=GOGcom_ID%20HOLDS%20%22{0}%22&format=json"
    $CountertagAdded = 0
   
    $steamAppListPath = Join-Path -Path [System.IO.Path]::GetTempPath() -ChildPath 'SteamAppList.json'
    if (Test-Path $steamAppListPath)
    {
        $AppListLastWrite = (Get-Item $steamAppListPath).LastWriteTime
        $TimeSpan = New-Timespan -days 1
        if (((Get-Date) - $AppListLastWrite) -gt $TimeSpan)
        {
            Get-SteamAppList $steamAppListPath
        }
    }
    else
    {
        Get-SteamAppList $steamAppListPath
    }

    [object]$steamAppList = [System.IO.File]::ReadAllLines($steamAppListPath) | ConvertFrom-Json
    
    $webClient = New-Object System.Net.WebClient
    $webClient.Encoding = [System.Text.Encoding]::UTF8
    foreach ($game in $PlayniteApi.MainView.SelectedGames) {
        if ($null -eq $game.Platforms)
        {
            continue
        }
        else
        {
            $isTargetSpecification = $false
            foreach ($platform in $game.Platforms) {
                if ($platform.Name -eq "PC (Windows)" -or $platform.Name -eq "PC")
                {
                    $isTargetSpecification = $true
                    break
                }

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
                $__logger.Info("$ExtensionName - Game `"$($game.name)`" is not a PC game")
                continue
            }
        }

        if ($null -ne $game.Tags)
        {
            $engineTagPresent = $false
            foreach ($tag in $game.Tags) {
                if ($tag.Name.StartsWith("[Engine]"))
                {
                    $__logger.Info("$ExtensionName - Game `"$($game.name)`" already has engine tag $($tag.Name)")
                    $engineTagPresent = $true
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
            $downloadedString = $webClient.DownloadString($uri)
            $gameInfo = $DownloadedString | ConvertFrom-Json
        } catch {
            $ErrorMessage = $_.Exception.Message
            $PlayniteApi.Dialogs.ShowErrorMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCGame_Engine_Checker_DownloadErrorMessage") -f $game.name, $ErrorMessage), $ExtensionName)
            break
        }

        if ($gameInfo.cargoquery.Count -eq 0)
        {
            $__logger.Info("$ExtensionName - `"$($uri)`" did not produce any results")
            continue
        }
        elseif ($null -eq $gameInfo.cargoquery[0].title.Engines -or [string]::IsNullOrEmpty($gameInfo.cargoquery[0].title.Engines))
        {
            $__logger.Info("$ExtensionName - `"$($uri)`" does not have engine data")
            continue  
        }

        $engines = $gameInfo.cargoquery[0].title.Engines.Replace("Engine:", "[Engine] ").Split(",")
        $engineName = $engines[0]

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
            $__logger.Info("$ExtensionName - Added `"$engineName`" engine tag to `"$($game.name)`".")
            $CountertagAdded++
        }
    }

    # Show finish dialogue with results
    $webClient.Dispose()
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