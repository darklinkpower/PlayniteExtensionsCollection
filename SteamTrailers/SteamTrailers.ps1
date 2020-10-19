function global:GetGameMenuItems
{
    param($menuArgs)

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem.Description =  "480p Trailer"
    $menuItem.FunctionName = "SteamTrailers480p"
    $menuItem.MenuSection = "Steam Trailers"
   
    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem2.Description =  "HD Trailer"
    $menuItem2.FunctionName = "SteamTrailersMax"
    $menuItem2.MenuSection = "Steam Trailers"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem3.Description =  "Microtrailer"
    $menuItem3.FunctionName = "SteamTrailersMicro"
    $menuItem3.MenuSection = "Steam Trailers"

    return $menuItem, $menuItem2, $menuItem3
}

function Get-SteamAppList
{
    param (
        [string]$AppListPath
    )

    try {
        # Download Steam App list and convert App Names
        $Uri = 'https://api.steampowered.com/ISteamApps/GetAppList/v2/'
        [array]$AppListContent = (Invoke-WebRequest $Uri | ConvertFrom-Json).applist.apps
        foreach ($SteamApp in $AppListContent) {
            $SteamApp.name = $($SteamApp.name).ToLower() -replace '[^\p{L}\p{Nd}]', ''
        }
        
        # Save Json file locally
        ConvertTo-Json $AppListContent -Depth 2  -Compress | Out-File -Encoding 'UTF8' -FilePath $AppListPath
        $__logger.Info("Steam Trailers - Downloaded AppList")
    } catch {
        $ErrorMessage = $_.Exception.Message
        $__logger.Info("Steam Trailers - Error downloading Steam AppList database. Error: $ErrorMessage")
        $PlayniteApi.Dialogs.ShowMessage("Error downloading Steam AppList database. Error: $ErrorMessage", "Steam Trailers");
        exit
    }
}

function Get-SteamVideo() {
    param (
        [object]$Game,
        [string]$VideoQuality
    )
    # Check if it's a Steam Game and continue if true
    if ($game.Source.name -eq "Steam")
    {
        # Use GameId for Steam games
        $AppId = $game.GameId
    }

    else
    {
        # Look for Steam Store URL in links for other games
        foreach ($link in $game.Links) {
            switch -regex ($link.Url) {
                "https?://store.steampowered.com/app/(\d+)/?\w*/?" {
                $AppId = $matches[1]}
            }
        }
    }
    if (!$AppId)
    {
        # Get Steam AppList
        $AppListPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'AppList.json'
        if (!(Test-Path $AppListPath))
        {
            Get-SteamAppList -AppListPath $AppListPath
        }

        # Try to search for AppId by searching in local Steam AppList database
        [object]$AppList = [System.IO.File]::ReadAllLines($AppListPath) | ConvertFrom-Json
        $Gamename = $($game.name).ToLower() -replace '[^\p{L}\p{Nd}]', ''
        foreach ($SteamApp in $AppList) {
            if ($SteamApp.name -eq $Gamename) 
            {
                [string]$AppId = $SteamApp.appid
                break
            }
        }
        if (!$AppId)
        {
            # Download Steam AppList if game was not found in local Steam AppList database and local Steam AppList database is older than 2 days
            $AppListLastWrite = (get-item $AppListPath).LastWriteTime
            $TimeSpan = new-timespan -days 2
            if (((get-date) - $AppListLastWrite) -gt $TimeSpan)
            {
                Get-SteamAppList -AppListPath $AppListPath

                # Try to search for AppId again by searching in the new downloaded AppList
                [object]$AppList = [System.IO.File]::ReadAllLines($AppListPath) | ConvertFrom-Json
                foreach ($SteamApp in $AppList) {
                    if ($SteamApp.name -eq $Gamename) 
                    {
                        [string]$AppId = $SteamApp.appid
                        break
                    }
                }
            }
        }
    }
    
    # Continue only if AppId was obtained
    if ($AppId)
    {
        # Set Steam API url and download json file
        $SteamAPI = 'https://store.steampowered.com/api/appdetails?appids=' + "$AppId"
        try { 
            $json = Invoke-WebRequest -uri $SteamAPI -TimeoutSec '10' | ConvertFrom-Json
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
            $global:VideoMicro = 'https://steamcdn-a.akamaihd.net/steam/apps/' + "$VideoId" + '/microtrailer.webm'
            $global:VideoUrl = $json.$AppId.data.movies[0].webm.$VideoQuality
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

function Invoke-HtmlLaunch() {
    param (
        [object]$game,
        [string]$VideoTitle,
        [int]$VideoWidth,
        [int]$VideoHeight,
        [string]$VideoExtraArguments,
        [string]$VideoUrl
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
    
    <body>
      <div class='container'>
        <video id='video' class='video-js vjs-fill'  
          width='100%' height='100%'
          controls preload='auto'
          preload='auto'
          data-setup='{}'
          $VideoExtraArguments>
          <source src='$VideoUrl' type='video/webm'>
        </video>
        </div>
    </body>"
    
    # Open html in webview
    $webView = $PlayniteApi.WebViews.CreateView($VideoWidth, $VideoHeight)
    $webView.Navigate("data:text/html," + $html)
    $webView.OpenDialog()
}

function SteamTrailers480p
{
    $GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.platform.name -eq "PC"} | Select-Object -last 1
    if ($GameDatabase.count -ne 1)
    {
        $PlayniteApi.Dialogs.ShowMessage("PC game not selected", "Steam Trailers");
        exit
    }
    $VideoTitle = "480p Trailer"
    $VideoQuality = "480"
    $VideoWidth = 886
    $VideoHeight = 535
    Get-SteamVideo -Game $GameDatabase -VideoQuality $VideoQuality
    Invoke-HtmlLaunch -Game $GameDatabase -VideoTitle $VideoTitle -VideoWidth $VideoWidth -VideoHeight $VideoHeight -VideoUrl $VideoUrl
}

function SteamTrailersMax
{
    $GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.platform.name -eq "PC"} | Select-Object -last 1
    if ($GameDatabase.count -ne 1)
    {
        $PlayniteApi.Dialogs.ShowMessage("PC game not selected", "Steam Trailers");
        exit
    }
    $VideoTitle = "HD Trailer"
    $VideoQuality = "max"
    $VideoWidth = 1312
    $VideoHeight = 775
    Get-SteamVideo -Game $GameDatabase -VideoQuality $VideoQuality
    Invoke-HtmlLaunch -Game $GameDatabase -VideoTitle $VideoTitle -VideoWidth $VideoWidth -VideoHeight $VideoHeight -VideoUrl $VideoUrl
}

function SteamTrailersMicro
{
    $GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.platform.name -eq "PC"} | Select-Object -last 1
    if ($GameDatabase.count -ne 1)
    {
        $PlayniteApi.Dialogs.ShowMessage("PC game not selected", "Steam Trailers");
        exit
    }
    $VideoTitle = "Microtrailer"
    $VideoWidth = 886
    $VideoHeight = 535
    $VideoExtraArguments = "loop='true' autoplay muted"
    Get-SteamVideo -Game $GameDatabase
    try {
        Invoke-WebRequest $VideoMicro -Method Head
    } catch {
        $PlayniteApi.Dialogs.ShowMessage("Microtrailer for `"$($GameDatabase.name)`" not available", "Steam Trailers");
        exit
    }
    $VideoUrl = $VideoMicro
    Invoke-HtmlLaunch -Game $GameDatabase -VideoTitle $VideoTitle -VideoWidth $VideoWidth -VideoHeight $VideoHeight -VideoExtraArguments $VideoExtraArguments -VideoUrl $VideoUrl
}