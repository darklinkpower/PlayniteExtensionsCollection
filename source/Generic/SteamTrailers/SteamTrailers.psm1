function GetGameMenuItems
{
    param(
        $scriptGameMenuItemActionArgs
    )

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_MenuItemSteamSdTrailerDescription")
    $menuItem.FunctionName = "SteamTrailers480p"
    $menuItem.MenuSection = [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_MenuSectionDescriptionVideos")
   
    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem2.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_MenuItemSteamHDTrailerDescription")
    $menuItem2.FunctionName = "SteamTrailersMax"
    $menuItem2.MenuSection = [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_MenuSectionDescriptionVideos")

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem3.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_MenuItemSteamMicroTrailerDescription")
    $menuItem3.FunctionName = "SteamTrailersMicro"
    $menuItem3.MenuSection = [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_MenuSectionDescriptionVideos")

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
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_GenericFileDownloadErrorMessage") -f $url, $errorMessage))
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

function Get-GameIsSpecificationId
{
    param (
        [Playnite.SDK.Models.Game] $game,
        [string] $targetSpecificationId
    )
    
    if ($null -ne $game.Platforms)
    {
        foreach ($platform in $game.Platforms) {
            if ($null -eq $platform.SpecificationId)
            {
                continue
            }
            if ($platform.SpecificationId -eq $targetSpecificationId)
            {
                return $true
            }
        }
    }

    return $false
}

function Get-SteamVideo
{
    param (
        [string] $appId,
        [string] $videoQuality
    )

    # Set Steam API url and download json file
    $steamApi = "https://store.steampowered.com/api/appdetails?appids={0}" -f $appId
    try {
        $json = Get-DownloadString $steamApi | ConvertFrom-Json
    } catch {
        $ErrorMessage = $_.Exception.Message
        $PlayniteApi.Dialogs.ShowErrorMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_GameInformationDownloadFailMessage") -f $appId, $errorMessage), "Steam Trailers")
        return $null
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
        return $null
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

function Get-VideoUrl
{
    param(
        [Playnite.SDK.Models.Game] $game,
        [string] $videoQuality
    )
    
    if ($null -eq $steamAppList)
    {
        Set-GlobalAppList $false
    }
       
    $isPcGame = Get-GameIsSpecificationId $game "pc_windows"
    if ($isPcGame -eq $false)
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_PcGameNotSelectedMessage") -f $game.Name), "Steam Trailers")
        return
    }
    
    $appId = Get-SteamAppId $game
    if ($null -eq $appId)
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Trailers_NoAppIdFoundMessage") -f $game.name), "Steam Trailers")
        return
    }

    $videoUrl = Get-SteamVideo $appId $videoQuality
    return $videoUrl
}

function SteamTrailers480p
{
    param(
        $scriptGameMenuItemActionArgs
    )
    

    $game = $scriptGameMenuItemActionArgs.Games | Select-Object -last 1
    $videoUrl = Get-VideoUrl $game "480"
    if ($null -ne $videoUrl)
    {
        Invoke-HtmlLaunch -Game $game -VideoTitle "SD Trailer" -webviewWidth 880 -webviewHeight 528 -VideoExtraArguments "" -VideoUrl $videoUrl
    }
}

function SteamTrailersMax
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    $game = $scriptGameMenuItemActionArgs.Games | Select-Object -last 1
    $videoUrl = Get-VideoUrl $game "max"
    if ($null -ne $videoUrl)
    {
        Invoke-HtmlLaunch -Game $game -VideoTitle "HD Trailer" -webviewWidth 1280 -webviewHeight 750 -VideoExtraArguments "" -VideoUrl $videoUrl
    }
}

function SteamTrailersMicro
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    $game = $scriptGameMenuItemActionArgs.Games | Select-Object -last 1
    $videoUrl = Get-VideoUrl $game "micro"
    if ($null -ne $videoUrl)
    {
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
}