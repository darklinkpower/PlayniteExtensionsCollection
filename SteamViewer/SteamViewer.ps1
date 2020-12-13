function GetGameMenuItems
{
    param(
        $menuArgs
    )

    $extensionName = "Steam Viewer"
    $subSection = "Components"
    
    $menuItem9 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem9.Description =  "Activate product"
    $menuItem9.FunctionName = "Start-ComponentActivateProduct"
    $menuItem9.MenuSection = "$extensionName|$subSection"

    $menuItem10 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem10.Description =  "Downloads"
    $menuItem10.FunctionName = "Start-ComponentDownloads"
    $menuItem10.MenuSection = "$extensionName|$subSection"
    
    $menuItem11 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem11.Description =  "Friends"
    $menuItem11.FunctionName = "Start-ComponentFriends"
    $menuItem11.MenuSection = "$extensionName|$subSection"

    $menuItem12 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem12.Description =  "News"
    $menuItem12.FunctionName = "Start-ComponentNews"
    $menuItem12.MenuSection = "$extensionName|$subSection"

    $menuItem13 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem13.Description =  "Screenshots"
    $menuItem13.FunctionName = "Start-ComponentScreenshots"
    $menuItem13.MenuSection = "$extensionName|$subSection"

    $menuItem14 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem14.Description =  "Settings"
    $menuItem14.FunctionName = "Start-ComponentSettings"
    $menuItem14.MenuSection = "$extensionName|$subSection"

    $game = $menuArgs.Games | Select-Object -last 1
    if ($game.PluginId -eq "cb91dfc9-b977-43bf-8e70-55f46e410fab")
    {
        $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
        $menuItem1.Description =  "Store Page"
        $menuItem1.FunctionName = "Start-StorePage"
        $menuItem1.MenuSection = $extensionName

        $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
        $menuItem2.Description =  "Community Hub"
        $menuItem2.FunctionName = "Start-CommunityHub"
        $menuItem2.MenuSection = $extensionName

        $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
        $menuItem3.Description =  "Discussions"
        $menuItem3.FunctionName = "Start-Discussions"
        $menuItem3.MenuSection = $extensionName

        $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
        $menuItem4.Description =  "Guides"
        $menuItem4.FunctionName = "Start-Guides"
        $menuItem4.MenuSection = $extensionName
        
        $menuItem5 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
        $menuItem5.Description =  "News"
        $menuItem5.FunctionName = "Start-News"
        $menuItem5.MenuSection = $extensionName

        $menuItem6 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
        $menuItem6.Description =  "Achievements"
        $menuItem6.FunctionName = "Start-Achievements"
        $menuItem6.MenuSection = $extensionName

        $menuItem7 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
        $menuItem7.Description =  "Points Shop"
        $menuItem7.FunctionName = "Start-PointsShop"
        $menuItem7.MenuSection = $extensionName

        $menuItem8 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
        $menuItem8.Description =  "View in Steam library"
        $menuItem8.FunctionName = "Start-Library"
        $menuItem8.MenuSection = $extensionName

        return $menuItem1, $menuItem2, $menuItem3, $menuItem4, $menuItem5, $menuItem6, $menuItem7, $menuItem8, $menuItem9, $menuItem10, $menuItem11, $menuItem12, $menuItem13, $menuItem14
    }
    else
    {
        return $menuItem9, $menuItem10, $menuItem11, $menuItem12, $menuItem13, $menuItem14  
    }
}

function Start-Library
{
    param(
        $menuArgs
    )
    
    $game = $menuArgs.Games | Select-Object -last 1
    $uri = "steam://nav/games/details/{0}" -f $game.GameId
    Start-Process $uri
}

function Start-CommunityHub
{
    param(
        $menuArgs
    )
    
    $game = $menuArgs.Games | Select-Object -last 1
    $url =  "https://steamcommunity.com/app/{0}/" -f $game.GameId
    $uri = "steam://openurl/{0}" -f $url
    Start-Process $uri
}

function Start-Discussions
{
    param(
        $menuArgs
    )
    
    $game = $menuArgs.Games | Select-Object -last 1
    $url =  "https://steamcommunity.com/app/{0}/discussions/" -f $game.GameId
    $uri = "steam://openurl/{0}" -f $url
    Start-Process $uri
}

function Start-Guides
{
    param(
        $menuArgs
    )
    
    $game = $menuArgs.Games | Select-Object -last 1
    $url =  "https://steamcommunity.com/app/{0}/guides/" -f $game.GameId
    $uri = "steam://openurl/{0}" -f $url
    Start-Process $uri
}

function Start-Achievements
{
    param(
        $menuArgs
    )
    
    $game = $menuArgs.Games | Select-Object -last 1
    $url =  "https://steamcommunity.com/stats/{0}/achievements/" -f $game.GameId
    $uri = "steam://openurl/{0}" -f $url
    Start-Process $uri
}

function Start-StorePage
{
    param(
        $menuArgs
    )
    
    $game = $menuArgs.Games | Select-Object -last 1
    $url =  "https://store.steampowered.com/app/{0}/" -f $game.GameId
    $uri = "steam://openurl/{0}" -f $url
    Start-Process $uri
}

function Start-News
{
    param(
        $menuArgs
    )
    
    $game = $menuArgs.Games | Select-Object -last 1
    $url =  "https://store.steampowered.com/news/?appids={0}" -f $game.GameId
    $uri = "steam://openurl/{0}" -f $url
    Start-Process $uri
}

function Start-PointsShop
{
    param(
        $menuArgs
    )
    
    $game = $menuArgs.Games | Select-Object -last 1
    $url =  "https://store.steampowered.com/points/shop/app/{0}" -f $game.GameId
    $uri = "steam://openurl/{0}" -f $url
    Start-Process $uri
}

function Start-ComponentActivateProduct
{
    $uri =  "steam://open/{0}" -f "activateproduct"
    Start-Process $uri
}

function Start-ComponentDownloads
{
    $uri =  "steam://open/{0}" -f "downloads"
    Start-Process $uri
}

function Start-ComponentFriends
{
    $uri =  "steam://open/{0}" -f "friends"
    Start-Process $uri
}

function Start-ComponentNews
{
    $uri =  "steam://open/{0}" -f "news"
    Start-Process $uri
}

function Start-ComponentScreenshots
{
    $uri =  "steam://open/{0}" -f "screenshots"
    Start-Process $uri
}

function Start-ComponentSettings
{
    $uri =  "steam://open/{0}" -f "settings"
    Start-Process $uri
}