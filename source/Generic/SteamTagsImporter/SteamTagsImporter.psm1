function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Tags_Importer_MenuItemGet-SteamTagsDefaultDescription")
    $menuItem1.FunctionName = "Get-SteamTagsDefault"
    $menuItem1.MenuSection = "@Steam Tags Importer"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Tags_Importer_MenuItemGet-SteamTagsAllDescription")
    $menuItem2.FunctionName = "Get-SteamTagsAll"
    $menuItem2.MenuSection = "@Steam Tags Importer"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem3.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Tags_Importer_MenuItemGet-SteamTagsManualDescription")
    $menuItem3.FunctionName = "Get-SteamTagsManual"
    $menuItem3.MenuSection = "@Steam Tags Importer"

    return $menuItem1, $menuItem2, $menuItem3
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
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Tags_Importer_GenericFileDownloadErrorMessage") -f $url, $errorMessage));
        $webClient.Dispose()
        return
    }
}

function Get-SteamAppList
{
    param (
        $steamAppListPath
    )

    $uri = 'https://api.steampowered.com/ISteamApps/GetAppList/v2/'
    $steamAppList = Get-DownloadString $uri
    if ($null -ne $steamAppList)
    {
        [array]$appListContent = ($steamAppList | ConvertFrom-Json).applist.apps
        foreach ($steamApp in $appListContent) {
            $steamApp.name = $steamApp.name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
        }
        ConvertTo-Json $appListContent -Depth 2 -Compress | Out-File -Encoding 'UTF8' -FilePath $steamAppListPath
        $__logger.Info("Downloaded AppList")
        $global:steamAppListDownloaded = $true
    }
    else
    {
        exit
    }
}

function Set-GlobalAppList
{
    param (
        [bool]$forceDownload
    )
    
    # Get Steam AppList
    $steamAppListPath = Join-Path -Path $env:TEMP -ChildPath 'SteamAppList.json'
    if (!(Test-Path $steamAppListPath) -or ($forceDownload -eq $true))
    {
        Get-SteamAppList $steamAppListPath
    }
    $global:steamAppList = @{}
    [object]$appListJson = [System.IO.File]::ReadAllLines($steamAppListPath) | ConvertFrom-Json
    foreach ($steamApp in $appListJson) {
        # Use a try block in case multple apps use the same name
        try {
            $steamAppList.add($steamApp.name, $steamApp.appid)
        } catch {}
    }

    $__logger.Info(("Global applist set from {0}" -f $steamAppListPath))
}

function Get-SteamAppId
{
    param (
        $game
    )

    $gamePlugin = [Playnite.SDK.BuiltinExtensions]::GetExtensionFromId($game.PluginId).ToString()
    $__logger.Info(("Get-SteammAppId start. Game: {0}, Plugin: {1}" -f $game.Name, $gamePlugin))

    # Use GameId for Steam games
    if ($gamePlugin -eq "SteamLibrary")
    {
        $__logger.Info(("Game: {0}, appId {1} found via pluginId" -f $game.Name, $game.GameId))
        return $game.GameId
    }
    elseif ($null -ne $game.Links)
    {
        # Look for Steam Store URL in links for other games
        foreach ($link in $game.Links) {
            if ($link.Url -match "https?://store.steampowered.com/app/(\d+)/?")
            {
                $__logger.Info(("Game: {0}, appId {1} found via links" -f $game.Name, $link.Url))
                return $matches[1]
            }
        }
    }

    $gameName = $game.Name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
    if ($null -ne $steamAppList[$gameName])
    {
        $appId = $steamAppList[$gameName].ToString()
        $__logger.Info(("Game: {0}, appId {1} found via AppList" -f $game.Name, $appId))
        return $appId
    }
    
    if ((!$appId) -and ($appListDownloaded -eq $false))
    {
        # Download Steam AppList if game was not found in local Steam AppList database and local Steam AppList database is older than 2 days
        $steamAppListPath = Join-Path -Path $env:TEMP -ChildPath 'SteamAppList.json'
        $AppListLastWrite = (Get-Item $steamAppListPath).LastWriteTime
        $timeSpan = New-Timespan -days 2
        if (((Get-date) - $AppListLastWrite) -gt $timeSpan)
        {
            Set-GlobalAppList $true
            if ($null -ne $steamAppList[$gameName])
            {
                $appId = $steamAppList[$gameName].ToString()
                $__logger.Info(("Game: {0}, appId {1} found via AppList" -f $game.Name, $appId))
                return $appId
            }
            return $null
        }
    }
}

function Get-StorePageTags {
    param (
        $appId,
        $tagsLimit
    )
    
    $tags = [System.Collections.Generic.List[string]]::New()
    $steamStoreUrl = "https://store.steampowered.com/app/{0}/" -f $appId

    try {
        $cookieContainer = [System.Net.CookieContainer]::New(2)
        
        $cookie = [System.Net.Cookie]::New("wants_mature_content", "1", "/app/" + $appId, "store.steampowered.com")
        $cookieContainer.add($cookie)

        $cookie = [System.Net.Cookie]::New("birthtime", "628495201", "/", "store.steampowered.com")
        $cookieContainer.add($cookie)
        
        $request = [System.Net.HttpWebRequest]::Create($steamStoreUrl)
        $request.CookieContainer = $cookieContainer
        $request.Timeout = 15000

        $response = $request.GetResponse()
        $responseStream = $response.GetResponseStream()
        $streamReader = [System.IO.StreamReader]::New($responseStream)
        $steamStoreUrlSource = $streamReader.ReadToEnd()

        $response.Close()
        $streamReader.Close()
        $response.Dispose()
        $streamReader.Dispose()
    } catch {
        $errorMessage = $_.Exception.Message
        $__logger.Info("Error downloading file `"$steamStoreUrl`". Error: $errorMessage")
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_ThemeConstantsUpdatedMessage") -f $steamStoreUrl, $errorMessage));
        $response.Close()
        $response.Dispose()
        $streamReader.Close()
        $streamReader.Dispose()
    }

    $sourceRegex = ([regex]'InitAppTagModal\([^[]+([^\n]+)').Matches($steamStoreUrlSource)
    if ($null -ne $sourceRegex.Groups)
    {
        $gameTags = $sourceRegex.Groups[1].Value -replace "..$" | ConvertFrom-Json
        $gameTags | Select-Object -First $tagsLimit | Select-Object -Property "name" | ForEach-Object {$tags.Add($_.name)}
    }

    return $tags
}

function Get-SteamTags
{
    param (
        [int]$tagsLimit
    )
    
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    if ($gameDatabase.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Tags_Importer_NoGamesSelectedMessage"))
        return
    }
    if ($null -eq $steamAppList)
    {
        Set-GlobalAppList $false
    }
    $counterTagsAdded = 0
    $webClient = New-Object System.Net.WebClient
    $webClient.Encoding = [System.Text.Encoding]::UTF8

    foreach ($game in $gameDatabase) {
        # Wait time to prevent reaching requests limit
        Start-Sleep -Milliseconds 1500

        $appId = Get-SteamAppId $game
        if ($null -eq $appId)
        {
            continue
        }

        $tagsAdded = $false
        if ($tagsLimit -le 10)
        {
            $gameTags = [System.Collections.Generic.List[string]]::New()
            $steamStoreUrl = "https://store.steampowered.com/broadcast/ajaxgetbatchappcapsuleinfo?appids={0}&l=english" -f $appId
            try {
                $json = $webClient.DownloadString($steamStoreUrl)
                $__logger.Info("Downloaded string from `"$steamStoreUrl`"")
            } catch {
                $errorMessage = $_.Exception.Message
                $__logger.Info("Error downloading file `"$steamStoreUrl`". Error: $errorMessage")
                continue
            }

            if ($null -ne $json)
            {
                $data = $json | ConvertFrom-Json
                if ($data.success -eq 1)
                {
                    $data.apps[0].tags | Select-Object -First $tagsLimit | Select-Object -Property "name" | ForEach-Object {$gameTags.Add($_.name)}
                }
            }
        }
        else
        {
            $gameTags = Get-StorePageTags $appId $tagsLimit
        }

        foreach ($gameTag in $gameTags) {
            $tag = $PlayniteApi.Database.Tags.Add($gameTag)
            if ($game.tagIds -notcontains $tag.Id)
            {
                if ($null -ne $game.tagIds)
                {
                    $game.tagIds += $tag.Id
                }
                else
                {
                    # Fix in case game has null tagIds
                    $game.tagIds = $tag.Id
                }
                $tagsAdded = $true
                $counterTagsAdded++
            }
        }

        if ($tagsAdded -eq $true)
        {
            $PlayniteApi.Database.Games.Update($game)
        }  
    }

    $webClient.Dispose()
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Tags_Importer_TagsAddedResultsMessage") -f $counterTagsAdded, $($gameDatabase.Count)), "Steam Tags Importer")
}

function Get-SteamTagsDefault
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    Get-SteamTags 5
}

function Get-SteamTagsAll
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    Get-SteamTags 99
}

function Get-SteamTagsManual
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $userInput = $PlayniteApi.Dialogs.SelectString(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Tags_Importer_InputTagsNumberDownloadMessage")), "Steam Tags Importer", "5")
    
    if ($userInput.result -eq "True")
    {
        try {
            $number = [System.Int32]::Parse($userInput.SelectedString)
        } catch {
            $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Tags_Importer_InvalidInputErrorMessage")), "Steam Tags Importer")
            return
        }
        if ($userInput.SelectedString -eq 0)
        {
            $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Tags_Importer_InvalidInputErrorMessage")), "Steam Tags Importer")
            return
        }
        Get-SteamTags $number
    }
    return    
}