function GetGameMenuItems
{
    param(
      $menuArgs
    )

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem.Description =  "Review"
    $menuItem.FunctionName = "Invoke-ReviewViewer"
    $menuItem.MenuSection = "Video"
   
    return $menuItem
}

function Invoke-ReviewViewer
{
  $ExtensionName = "Review viewer"
  
  $game = $PlayniteApi.MainView.SelectedGames | Select-Object -last 1
    
    try {
        $query = "$($game.name -replace " ", "+")+review"
        $uri = "https://www.youtube.com/results?search_query={0}" -f $query
        $webContent = Invoke-WebRequest $uri
    } catch {
        $ErrorMessage = $_.Exception.Message
        $PlayniteApi.Dialogs.ShowErrorMessage("Error during Youtube search. Error: $ErrorMessage", $ExtensionName);
        exit
    }

    $webContent.Content -match '"videoId":"((.+?(?=")))"'
    if ($matches)
    {
        $youtubeLink = "https://www.youtube-nocookie.com/embed/{0}" -f $matches[1]
    }
    else
    {
        $PlayniteApi.Dialogs.ShowMessage("No video found in Youtube search.", $ExtensionName);
        exit
    }

    # Generate html
    $html = "
    <head>
      <title>$($game.name) - Review</title>
      <script type='text/css'>
        .container {
          width: 100%;
          height: 100vh;
        }
      </script>
    </head>
    
    <body>
      <div class='container'>
        <iframe width='100%' height='100%'
          src='$youtubeLink'
          frameborder='0'
          allow='accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture' allowfullscreen>
        </iframe>
      </div>
    </body>"

    $webView = $PlayniteApi.WebViews.CreateView(1200, 720)
    $webView.Navigate("data:text/html," + $html)
    $webView.OpenDialog()
    $webView.Dispose()
}