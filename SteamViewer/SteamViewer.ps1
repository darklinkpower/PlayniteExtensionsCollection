function GetGameMenuItems
{
    param(
        $menuArgs
    )

    $game = $menuArgs.Games | Select-Object -last 1
    if ($game.PluginId -eq "cb91dfc9-b977-43bf-8e70-55f46e410fab")
    {
        $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
        $menuItem.Description =  "View game in Steam library"
        $menuItem.FunctionName = "Open-InSteamLibrary"
    
        return $menuItem
    }
}

function Open-InSteamLibrary
{
    param(
        $menuArgs
    )
    
    $game = $menuArgs.Games | Select-Object -last 1
    $uri = "steam://nav/games/details/{0}" -f $game.GameId
    Start-Process $uri
}