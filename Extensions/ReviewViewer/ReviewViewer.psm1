function GetGameMenuItems
{
    param(
      $menuArgs
    )

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem.Description =  "Youtube Review"
    $menuItem.FunctionName = "Invoke-ReviewViewer"
    $menuItem.MenuSection = "Video"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem2.Description =  "Youtube Trailer"
    $menuItem2.FunctionName = "Invoke-TrailerViewer"
    $menuItem2.MenuSection = "Video"

    return $menuItem, $menuItem2
}

function Invoke-ReviewViewer
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    Invoke-YoutubeVideo "Review"
}
function Invoke-TrailerViewer
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    Invoke-YoutubeVideo "Trailer"
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
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCGenericFileDownloadError") -f $url, $errorMessage))
        return
    }
}

function Invoke-YoutubeVideo
{
    param (
        $videoType
    )

    $ExtensionName = "Review viewer"

    $game = $PlayniteApi.MainView.SelectedGames | Select-Object -last 1

    $query = "$($game.name -replace " ", "+" -replace "&", "and")+$videoType"
    $uri = "https://www.youtube.com/results?search_query={0}" -f $query
    $webContent = Get-DownloadString $uri
    if ($null -eq $webContent)
    {
        exit
    }

    $webContent -match '"videoId":"((.+?(?=")))"'
    if ($matches)
    {
        $youtubeLink = "https://www.youtube-nocookie.com/embed/{0}" -f $matches[1]
    }
    else
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCNoVideoFoundMessage"), $ExtensionName)
        exit
    }

    # Generate html
    $html = "
    <head>
        <title>$($game.name) - Review</title>
    </head>

    <body style='margin:0'>
        <div>
            <iframe width='100%' height='100%'
                src='$youtubeLink'
                frameborder='0'
                allow='accelerometer; clipboard-write; encrypted-media; gyroscope;'>
            </iframe>
        </div>
    </body>"

    $webView = $PlayniteApi.WebViews.CreateView(1280, 750)
    $webView.Navigate("data:text/html," + $html)
    $webView.OpenDialog()
    $webView.Dispose()
}