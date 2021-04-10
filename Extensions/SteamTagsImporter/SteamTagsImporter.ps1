function GetMainMenuItems
{
    param(
        $menuArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Import Steam Tags for selected games"
    $menuItem1.FunctionName = "Get-SteamTags"
    $menuItem1.MenuSection = "@Steam Tags Importer"

    return $menuItem1
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
        $__logger.Info("Downloaded Steam AppList")
    }
    else
    {
        exit
    }
}

function Get-SteamAppId
{
    param (
        $game
    )

    # Use GameId for Steam games
    if ([Playnite.SDK.BuiltinExtensions]::GetExtensionFromId($game.PluginId) -eq "SteamLibrary")
    {
        return $game.GameId
    }
    else
    {
        # Look for Steam Store URL in links for other games
        foreach ($link in $game.Links) {
            switch -regex ($link.Url) {
                "https?://store.steampowered.com/app/(\d+)/?\w*/?" {
                return $matches[1]}
            }
        }
    }

    if (!$AppId)
    {
        # Get Steam AppList
        $appListPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'AppList.json'
        if (!(Test-Path $appListPath))
        {
            Get-SteamAppList -AppListPath $appListPath
        }

        # Try to search for AppId by searching in local Steam AppList database
        [object]$AppList = [System.IO.File]::ReadAllLines($appListPath) | ConvertFrom-Json
        $gameName = $game.name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
        foreach ($steamApp in $AppList) {
            if ($steamApp.name -eq $gameName) 
            {
                return $steamApp.appid
            }
        }
        if (!$AppId)
        {
            # Download Steam AppList if game was not found in local Steam AppList database and local Steam AppList database is older than 2 days
            $AppListLastWrite = (Get-Item $appListPath).LastWriteTime
            $TimeSpan = New-Timespan -days 2
            if (((Get-date) - $AppListLastWrite) -gt $TimeSpan)
            {
                Get-SteamAppList -AppListPath $appListPath
                [object]$AppList = [System.IO.File]::ReadAllLines($appListPath) | ConvertFrom-Json
                foreach ($SteamApp in $AppList) {
                    if ($SteamApp.name -eq $Gamename) 
                    {
                        return $SteamApp.appid
                    }
                }
            }
        }
    }
}

function Get-SteamTags
{

    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.Platform.name -eq "PC"}
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
        $gameTags = $gameTags | Select-Object -First 5
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