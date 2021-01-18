function global:GetGameMenuItems
{
    param(
        $menuArgs
    )

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem.Description =  "Steam SD Trailer"
    $menuItem.FunctionName = "SteamTrailers480p"
    $menuItem.MenuSection = "Video"
   
    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem2.Description =  "Steam HD Trailer"
    $menuItem2.FunctionName = "SteamTrailersMax"
    $menuItem2.MenuSection = "Video"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem3.Description =  "Steam Microtrailer"
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
            $PlayniteApi.Dialogs.ShowMessage("Error connecting to server. Error: $errorMessage");
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
        $PlayniteApi.Dialogs.ShowMessage("Error downloading headers. Error: $errorMessage");
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

function Get-SteamVideo
{
    param (
        $game,
        $videoQuality
    )

    if ($game.platform.name -ne "PC")
    {
        $PlayniteApi.Dialogs.ShowMessage("PC game not selected", "Steam Trailers");
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
            $PlayniteApi.Dialogs.ShowMessage("Couldn't download game information. Error: $ErrorMessage");
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
            $PlayniteApi.Dialogs.ShowMessage("Video for `"$($game.name)`" not available", "Steam Trailers");
            exit
        }
    }
    else
    {
        # Error message if no Steam AppId and no steam link for games from other sources
        $PlayniteApi.Dialogs.ShowMessage("`"$($game.name)`" is not a Steam game and no information was found to obtain video data", "Steam Trailers");
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
    $game = $PlayniteApi.MainView.SelectedGames | Select-Object -last 1
    $videoUrl = Get-SteamVideo -Game $game -VideoQuality "480"
    Invoke-HtmlLaunch -Game $game -VideoTitle "SD Trailer" -webviewWidth 880 -webviewHeight 528 -VideoExtraArguments "" -VideoUrl $videoUrl
}

function SteamTrailersMax
{
    $game = $PlayniteApi.MainView.SelectedGames | Select-Object -last 1
    $videoUrl = Get-SteamVideo -Game $game -VideoQuality "max"
    Invoke-HtmlLaunch -Game $game -VideoTitle "HD Trailer" -webviewWidth 1280 -webviewHeight 750 -VideoExtraArguments "" -VideoUrl $videoUrl
}

function SteamTrailersMicro
{
    $game = $PlayniteApi.MainView.SelectedGames | Select-Object -last 1
    $videoUrl = Get-SteamVideo -Game $game -VideoQuality "micro"
    $statusCode = Get-RequestStatusCode $videoUrl
    if ($statusCode -eq 'OK')
    {
        Invoke-HtmlLaunch -Game $game -VideoTitle "Microtrailer" -webviewWidth 880 -webviewHeight 528 -VideoExtraArguments "loop='true' autoplay muted" -VideoUrl $videoUrl
    }
    elseif ($statusCode -ne 'NotFound')
    {
        $PlayniteApi.Dialogs.ShowMessage("Microtrailer for `"$($game.name)`" not available", "Steam Trailers");
    }
}