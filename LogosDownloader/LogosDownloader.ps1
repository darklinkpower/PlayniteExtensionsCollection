function GetGameMenuItems
{
    param(
        $menuArgs
    )

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem.Description =  "Download Logos Downloader for selected games"
    $menuItem.FunctionName = "Get-SteamLogos"
    $menuItem.MenuSection = "Logos Downloader"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem2.Description =  "Add logo from local file of selected game"
    $menuItem2.FunctionName = "Get-SteamLogosLocal"
    $menuItem2.MenuSection = "Logos Downloader"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem3.Description =  "Add logo from url of selected game"
    $menuItem3.FunctionName = "Get-SteamLogosUri"
    $menuItem3.MenuSection = "Logos Downloader"

    return $menuItem, $menuItem2, $menuItem3
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
        $__logger.Info("Steam Trailers - Downloaded AppList")
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

function Get-SteamLogos
{
    # Set GameDatabase
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.Platform.Name -eq "PC"}
    $logoUriTemplate = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/logo.png"
    $counter = 0
    foreach ($game in $gameDatabase) {
        $logoPath = $PlayniteApi.Database.DatabasePath + "\Files\" + "$($game.Id)\" + "Logo.png"
        if (Test-Path $logoPath)
        {
            continue
        }

        $steamAppId =  Get-SteamAppId $game
        if ($steamAppId)
        {
            $logoUri = $logoUriTemplate -f $steamAppId
            
            try {
                $webClient = New-Object System.Net.WebClient
                $webClient.Encoding = [System.Text.Encoding]::UTF8
                $webClient.DownloadFile($logoUri, $logoPath)
                $webClient.Dispose()
                $counter++
            } catch {
                $errorMessage = $_.Exception.Message
                $__logger.Info("Error downloading file `"$url`". Error: $errorMessage")
                $countErrors++
            }
        }
    }
    $results = "Downloaded logo of $counter games."
    if ($countErrors -gt 0)
    {
        $results += ". There were $countErrors errors, view Playnite log for details."
    }
    $PlayniteApi.Dialogs.ShowMessage($results, "Logos Downloader");
}

function Get-SteamLogosLocal
{
    # Set GameDatabase
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    if ($gameDatabase.count -gt 1)
    {
        $PlayniteApi.Dialogs.ShowMessage("More than one game is selected, please select only one game.", "Logos Downloader");
        return
    }

    foreach ($game in $gameDatabase) {
        $logoPath = $PlayniteApi.Database.DatabasePath + "\Files\" + "$($game.Id)\" + "Logo.png"
        $logoPathLocal = $PlayniteApi.Dialogs.SelectFile("logo|*.png")
        Copy-Item $logoPathLocal -Destination $logoPath -Force
        $PlayniteApi.Dialogs.ShowMessage("Added logo file to `"$($game.name)`"", "Logos Downloader");
    }
}

function Get-SteamLogosUri
{
    # Set GameDatabase
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    if ($gameDatabase.count -gt 1)
    {
        $PlayniteApi.Dialogs.ShowMessage("More than one game is selected, please select only one game.", "Logos Downloader");
        return
    }

    foreach ($game in $gameDatabase) {
        $logoPath = $PlayniteApi.Database.DatabasePath + "\Files\" + "$($game.Id)\" + "Logo.png"

        $logoUriInput = $PlayniteApi.Dialogs.SelectString("Enter logo Url:", "Logos Downloader", "");
        
        # Check if input was entered
        if ($logoUriInput.result -eq "True")
        {
            $logoUri = $logoUriInput.Selectedstring
            try {
                $webClient = New-Object System.Net.WebClient
                $webClient.DownloadFile($logoUri, $logoPath)
                $webClient.Dispose()
                $PlayniteApi.Dialogs.ShowMessage("Added logo file to `"$($game.name)`"", "Logos Downloader");
            } catch {
                $errorMessage = $_.Exception.Message
                $__logger.Info("Error downloading file `"$url`". Error: $errorMessage")
                $PlayniteApi.Dialogs.ShowMessage("Error downloading file `"$url`". Error: $errorMessage");
            }
        }
    }
}