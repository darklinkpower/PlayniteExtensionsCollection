function GetGameMenuItems
{
    param(
        $scriptGameMenuItemActionArgs
    )

    $extensionName = "Steam Viewer"
    $subSection = [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemComponentsSection")
    
    $menuItem9 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem9.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemComponentActivateProductDescription")
    $menuItem9.FunctionName = "Start-ComponentActivateProduct"
    $menuItem9.MenuSection = "$extensionName|$subSection"

    $menuItem10 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem10.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemComponentDownloadsDescription")
    $menuItem10.FunctionName = "Start-ComponentDownloads"
    $menuItem10.MenuSection = "$extensionName|$subSection"
    
    $menuItem11 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem11.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemComponentFriendsDescription")
    $menuItem11.FunctionName = "Start-ComponentFriends"
    $menuItem11.MenuSection = "$extensionName|$subSection"

    $menuItem12 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem12.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemComponentNewsDescription")
    $menuItem12.FunctionName = "Start-ComponentNews"
    $menuItem12.MenuSection = "$extensionName|$subSection"

    $menuItem13 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem13.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemComponentScreenshotsDescription")
    $menuItem13.FunctionName = "Start-ComponentScreenshots"
    $menuItem13.MenuSection = "$extensionName|$subSection"

    $menuItem14 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem14.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemComponentSettingsDescription")
    $menuItem14.FunctionName = "Start-ComponentSettings"
    $menuItem14.MenuSection = "$extensionName|$subSection"

    $game = $scriptGameMenuItemActionArgs.Games[-1]
    if ([Playnite.SDK.BuiltinExtensions]::GetExtensionFromId($game.PluginId) -ne "SteamLibrary")
    {
        return $menuItem9, $menuItem10, $menuItem11, $menuItem12, $menuItem13, $menuItem14
    }

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem1.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemGameStorePageDescription")
    $menuItem1.FunctionName = "Start-StorePage"
    $menuItem1.MenuSection = $extensionName

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem2.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemGameCommunityHubDescription")
    $menuItem2.FunctionName = "Start-CommunityHub"
    $menuItem2.MenuSection = $extensionName

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem3.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemGameDiscussionsDescription")
    $menuItem3.FunctionName = "Start-Discussions"
    $menuItem3.MenuSection = $extensionName

    $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem4.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemGameGuidesDescription")
    $menuItem4.FunctionName = "Start-Guides"
    $menuItem4.MenuSection = $extensionName
    
    $menuItem5 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem5.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemGameNewsDescription")
    $menuItem5.FunctionName = "Start-News"
    $menuItem5.MenuSection = $extensionName

    $menuItem6 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem6.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemGameAchievementsDescription")
    $menuItem6.FunctionName = "Start-Achievements"
    $menuItem6.MenuSection = $extensionName

    $menuItem7 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem7.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemGamePointsShopDescription")
    $menuItem7.FunctionName = "Start-PointsShop"
    $menuItem7.MenuSection = $extensionName

    $menuItem8 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem8.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Viewer_MenuItemGameLibraryDescription")
    $menuItem8.FunctionName = "Start-Library"
    $menuItem8.MenuSection = $extensionName

    return $menuItem1, $menuItem2, $menuItem3, $menuItem4, $menuItem5, $menuItem6, $menuItem7, $menuItem8, $menuItem9, $menuItem10, $menuItem11, $menuItem12, $menuItem13, $menuItem14
}

function Start-Library
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    $game = $scriptGameMenuItemActionArgs.Games[-1]
    $uri = "steam://nav/games/details/{0}" -f $game.GameId
    Start-Process $uri
}

function Start-UrlTemplateOnSteamWithGameId {
    param (
        $scriptGameMenuItemActionArgs,
        [string]$steamUrlTemplate
    )

    $game = $scriptGameMenuItemActionArgs.Games[-1]
    $steamUrl =  $steamUrlTemplate -f $game.GameId
    $uri = "steam://openurl/{0}" -f $steamUrl
    Start-Process $uri
}

function Start-CommunityHub
{
    param(
        $scriptGameMenuItemActionArgs
    )

    Start-UrlTemplateOnSteamWithGameId $scriptGameMenuItemActionArgs "https://steamcommunity.com/app/{0}/"
}

function Start-Discussions
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Start-UrlTemplateOnSteamWithGameId $scriptGameMenuItemActionArgs "https://steamcommunity.com/app/{0}/discussions/"
}

function Start-Guides
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Start-UrlTemplateOnSteamWithGameId $scriptGameMenuItemActionArgs "https://steamcommunity.com/app/{0}/guides/"
}

function Start-Achievements
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Start-UrlTemplateOnSteamWithGameId $scriptGameMenuItemActionArgs "https://steamcommunity.com/stats/{0}/achievements/"
}

function Start-StorePage
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Start-UrlTemplateOnSteamWithGameId $scriptGameMenuItemActionArgs "https://store.steampowered.com/app/{0}/"
}

function Start-News
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Start-UrlTemplateOnSteamWithGameId $scriptGameMenuItemActionArgs "https://store.steampowered.com/news/?appids={0}"
}

function Start-PointsShop
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Start-UrlTemplateOnSteamWithGameId $scriptGameMenuItemActionArgs "https://store.steampowered.com/points/shop/app/{0}"
}

function Open-SteamComponent {
    param (
        [string] $componentName
    )
    
    $uri =  "steam://open/{0}" -f $componentName
    Start-Process $uri
}

function Start-ComponentActivateProduct
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Open-SteamComponent "activateproduct"
}

function Start-ComponentDownloads
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Open-SteamComponent "downloads"
}

function Start-ComponentFriends
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Open-SteamComponent "friends"
}

function Start-ComponentNews
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Open-SteamComponent "news"
}

function Start-ComponentScreenshots
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Open-SteamComponent "screenshots"
}

function Start-ComponentSettings
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Open-SteamComponent "settings"
}