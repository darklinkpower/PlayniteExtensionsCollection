function GetGameMenuItems
{
    param(
        $scriptGameMenuItemActionArgs
    )

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_MenuItemSteamSdTrailerDescription")
    $menuItem.FunctionName = "SteamTrailers480p"
    $menuItem.MenuSection = "Video"
   
    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem2.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_MenuItemSteamHDTrailerDescription")
    $menuItem2.FunctionName = "SteamTrailersMax"
    $menuItem2.MenuSection = "Video"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem3.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_MenuItemSteamMicroTrailerDescription")
    $menuItem3.FunctionName = "SteamTrailersMicro"
    $menuItem3.MenuSection = "Video"

    return $menuItem, $menuItem2, $menuItem3
}

function Get-RequestStatusCode
{
    param (
        $url
    )
    
    try {
        $request = [System.Net.WebRequest]::Create($url)
        $request.Method = "HEAD"
        $response = $request.GetResponse()
        return $response.StatusCode
    } catch {
        $statusCode = $_.Exception.InnerException.Response.StatusCode
        $errorMessage = $_.Exception.Message
        $__logger.Info("Error connecting to server. Error: $errorMessage")
        if ($statusCode -ne 'NotFound')
        {
            $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_GenericConnectionErrorMessage") -f $ErrorMessage))
        }
        return $statusCode
    }
}

function Get-UriHeaders
{
    param (
        $url
    )
    try {
        $url = 'https://steamcdn-a.akamaihd.net/steam/apps/2567408603/microtrailer.webm'
        $request = [System.Net.WebRequest]::Create($url)
        $request.Method = "HEAD"
        $response = $request.GetResponse()
        return $response
    } catch {
        $errorMessage = $_.Exception.Message
        $__logger.Info("Error downloading headers. Error: $errorMessage")
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_GenericGetHeadersErrorMessage") -f $ErrorMessage))
        return
    }
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
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_GenericFileDownloadError") -f $url, $errorMessage))
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
        $steamAppListPath = Join-Path -Path $env:TEMP -ChildPath 'SteamAppList.json'
        if (!(Test-Path $steamAppListPath))
        {
            Get-SteamAppList -AppListPath $steamAppListPath
        }

        # Try to search for AppId by searching in local Steam AppList database
        [object]$steamAppList = [System.IO.File]::ReadAllLines($steamAppListPath) | ConvertFrom-Json
        $gameName = $game.name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
        foreach ($steamApp in $steamAppList) {
            if ($steamApp.name -eq $gameName) 
            {
                return $steamApp.appid
            }
        }
        if (!$AppId)
        {
            # Download Steam AppList if game was not found in local Steam AppList database and local Steam AppList database is older than 2 days
            $AppListLastWrite = (Get-Item $steamAppListPath).LastWriteTime
            $TimeSpan = New-Timespan -days 2
            if (((Get-date) - $AppListLastWrite) -gt $TimeSpan)
            {
                Get-SteamAppList -AppListPath $steamAppListPath
                [object]$steamAppList = [System.IO.File]::ReadAllLines($steamAppListPath) | ConvertFrom-Json
                foreach ($steamApp in $steamAppList) {
                    if ($steamApp.name -eq $Gamename) 
                    {
                        return $steamApp.appid
                    }
                }
            }
        }
    }
}

function Get-SteamVideo
{
    param (
        $game,
        $videoQuality
    )

    $isTargetSpecification = $false
    if ($null -ne $game.Platforms)
    {
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
    }
    if ($isTargetSpecification -eq $false)
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_PcGameNotSelectedMessage") -f $game.Name), "Steam Trailers")
        exit
    }

    $appId = Get-SteamAppId $game

    if ($appId)
    {
        # Set Steam API url and download json file
        $steamApi = "https://store.steampowered.com/api/appdetails?appids={0}" -f $appId
        try {
            $json = Get-DownloadString $steamApi | ConvertFrom-Json
        } catch {
            $ErrorMessage = $_.Exception.Message
            $PlayniteApi.Dialogs.ShowErrorMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_GameInformationDownloadFailMessage") -f $appId, $errorMessage), "Steam Trailers")
            exit
        }
        # Check if json has 'movie' information
        if ($json.$AppId.data.movies)
        {
            # Obtain video url from json file if available
            $VideoId = $json.$AppId.data.movies[0].id
            
            if (($videoQuality -eq "480") -or ($videoQuality -eq "max"))
            {
                $videoUrl = $json.$AppId.data.movies[0].webm.$VideoQuality
            }
            else
            {
                $videoUrl = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/microtrailer.webm" -f $VideoId
            }
            return $videoUrl
        }
        else
        {
            # Error message if no video found
            $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_NoVideoAvailableMessage"), "Steam Trailers")
            exit
        }
    }
    else
    {
        # Error message if no Steam AppId and no steam link for games from other sources
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_NoAppIdFoundMessage") -f $game.name), "Steam Trailers")
        exit
    }
}

function Invoke-HtmlLaunch
{
    param (
        $game,
        $VideoTitle,
        $webviewWidth,
        $webviewHeight,
        $VideoExtraArguments,
        $videoUrl
    )
    
    # Generate html
    $html = "
    <head>
      <title>$($game.name) - $VideoTitle</title>
      <link href='https://vjs.zencdn.net/7.8.2/video-js.min.css' rel='stylesheet' />
      <script src='https://vjs.zencdn.net/7.8.2/video.min.js'></script>
      <script type='text/css'>
        .container {
          width: 100%;
          height: 100vh;
        }
      </script>
    </head>
    
    <body style='margin:0'>
      <div class='container'>
        <video id='video' class='video-js vjs-fill'
          width='100%' height='100%'
          controls preload='auto'
          preload='auto'
          data-setup='{}'
          $VideoExtraArguments>
          <source src='$videoUrl' type='video/webm'>
        </video>
      </div>
    </body>"
    
    # Open html in webview
    $webView = $PlayniteApi.WebViews.CreateView($webviewWidth, $webviewHeight)
    $webView.Navigate("data:text/html," + $html)
    $webView.OpenDialog()
    $webView.Dispose()
}

function SteamTrailers480p
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $game = $PlayniteApi.MainView.SelectedGames | Select-Object -last 1
    $videoUrl = Get-SteamVideo -Game $game -VideoQuality "480"
    Invoke-HtmlLaunch -Game $game -VideoTitle "SD Trailer" -webviewWidth 880 -webviewHeight 528 -VideoExtraArguments "" -VideoUrl $videoUrl
}

function SteamTrailersMax
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $game = $PlayniteApi.MainView.SelectedGames | Select-Object -last 1
    $videoUrl = Get-SteamVideo -Game $game -VideoQuality "max"
    Invoke-HtmlLaunch -Game $game -VideoTitle "HD Trailer" -webviewWidth 1280 -webviewHeight 750 -VideoExtraArguments "" -VideoUrl $videoUrl
}

function SteamTrailersMicro
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $game = $PlayniteApi.MainView.SelectedGames | Select-Object -last 1
    $videoUrl = Get-SteamVideo -Game $game -VideoQuality "micro"
    $statusCode = Get-RequestStatusCode $videoUrl
    if ($statusCode -eq 'OK')
    {
        Invoke-HtmlLaunch -Game $game -VideoTitle "Microtrailer" -webviewWidth 880 -webviewHeight 528 -VideoExtraArguments "loop='true' autoplay muted" -VideoUrl $videoUrl
    }
    elseif ($statusCode -ne 'NotFound')
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_NoVideoAvailableMessage"), "Steam Trailers")
    }
}