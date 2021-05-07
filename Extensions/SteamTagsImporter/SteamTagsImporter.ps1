function GetMainMenuItems
{
    param(
        $menuArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Import Steam Tags for selected games (Maximum 5 tags)"
    $menuItem1.FunctionName = "Get-SteamTagsDefault"
    $menuItem1.MenuSection = "@Steam Tags Importer"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = "Import Steam Tags for selected games (Select maximum tags)"
    $menuItem2.FunctionName = "Get-SteamTagsManual"
    $menuItem2.MenuSection = "@Steam Tags Importer"

    return $menuItem1, $menuItem2
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
        $PlayniteApi.Dialogs.ShowMessage("Error downloading file `"$url`". Error: $errorMessage");
        return
    }
}

function Get-SteamAppList
{
    param (
        $appListPath
    )

    $uri = 'https://api.steampowered.com/ISteamApps/GetAppList/v2/'
    $steamAppList = Get-DownloadString $uri
    if ($null -ne $steamAppList)
    {
        [array]$appListContent = ($steamAppList | ConvertFrom-Json).applist.apps
        foreach ($steamApp in $appListContent) {
            $steamApp.name = $steamApp.name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
        }
        ConvertTo-Json $appListContent -Depth 2 -Compress | Out-File -Encoding 'UTF8' -FilePath $appListPath
        $__logger.Info("Downloaded AppList")
        $global:appListDownloaded = $true
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
    $appListPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'AppList.json'
    if (!(Test-Path $appListPath) -or ($forceDownload -eq $true))
    {
        Get-SteamAppList -AppListPath $appListPath
    }
    $global:appList = @{}
    [object]$appListJson = [System.IO.File]::ReadAllLines($appListPath) | ConvertFrom-Json
    foreach ($steamApp in $appListJson) {
        # Use a try block in case multple apps use the same name
        try {
            $appList.add($steamApp.name, $steamApp.appid)
        } catch {}
    }

    $__logger.Info(("Global applist set from {0}" -f $appListPath))
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
    if ($null -ne $appList[$gameName])
    {
        $appId = $appList[$gameName].ToString()
        $__logger.Info(("Game: {0}, appId {1} found via AppList" -f $game.Name, $appId))
        return $appId
    }
    
    if ((!$appId) -and ($appListDownloaded -eq $false))
    {
        # Download Steam AppList if game was not found in local Steam AppList database and local Steam AppList database is older than 2 days
        $appListPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'AppList.json'
        $AppListLastWrite = (Get-Item $appListPath).LastWriteTime
        $timeSpan = New-Timespan -days 2
        if (((Get-date) - $AppListLastWrite) -gt $timeSpan)
        {
            Set-GlobalAppList $true
            if ($null -ne $appList[$gameName])
            {
                $appId = $appList[$gameName].ToString()
                $__logger.Info(("Game: {0}, appId {1} found via AppList" -f $game.Name, $appId))
                return $appId
            }
            return $null
        }
    }
}

function Get-SteamTags
{
    param (
        [int]$tagsLimit
    )
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    if ($gameDatabase.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowMessage("No games selected")
        return
    }
    Set-GlobalAppList $false
    $regex = 'InitAppTagModal\([^[]+([^\n]+)'
    $CountertagsAdded = 0

    foreach ($game in $gameDatabase) {
        # Wait time to prevent reaching requests limit
        Start-Sleep -Milliseconds 1500

        $appId = Get-SteamAppId $game
        if (!$appId)
        {
            continue
        }
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
        } catch {
            $errorMessage = $_.Exception.Message
            $__logger.Info("Error downloading file `"$steamStoreUrl`". Error: $errorMessage")
            $PlayniteApi.Dialogs.ShowMessage("Error downloading page `"$steamStoreUrl`". Error: $errorMessage");
            break
        }

        $sourceRegex = ([regex]$regex).Matches($steamStoreUrlSource)
        if (!$sourceRegex.Groups)
        {
            continue
        }
        $gameTags = $sourceRegex.Groups[1].Value -replace "..$" | ConvertFrom-Json
        $gameTags = $gameTags | Select-Object -First $tagsLimit
        $tagsAdded = $false

        foreach ($gameTag in $gameTags) {
            $tag = $PlayniteApi.Database.Tags.Add($gameTag.Name)
    
            if ($game.tagIds -notcontains $tag.Id)
            {
                if ($game.tagIds)
                {
                    $game.tagIds += $tag.Id
                }
                else
                {
                    # Fix in case game has null tagIds
                    $game.tagIds = $tag.Id
                }
                $tagsAdded = $true
                $CountertagsAdded++
            }
        }
        if ($tagsAdded -eq $true)
        {
            $PlayniteApi.Database.Games.Update($game)
        }  
    }

    $PlayniteApi.Dialogs.ShowMessage("Added $CountertagsAdded tags to $($gameDatabase.Count) game(s).", "Steam Tags Importer")
}

function Get-SteamTagsDefault
{
    Get-SteamTags 5
}

function Get-SteamTagsManual
{
    $userInput = $PlayniteApi.Dialogs.SelectString("Enter number of tags to download", "Steam Tags Importer", "5")
    
    if ($userInput.result -eq "True")
    {
        try {
            $number = [System.Int32]::Parse($userInput.SelectedString)
        } catch {
            $PlayniteApi.Dialogs.ShowMessage("Invalid input", "Steam Tags Importer")
            return
        }
        if ($userInput.SelectedString -eq 0)
        {
            $PlayniteApi.Dialogs.ShowMessage("Invalid input", "Steam Tags Importer")
            return
        }
        Get-SteamTags $number
    }
    return    
}