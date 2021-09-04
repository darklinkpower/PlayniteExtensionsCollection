function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCLog_Open_MenuItemOpenLogDescription")
    $menuItem1.FunctionName = "Open-Log"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCLog_Open_MenuItemOpenExtensionsLogDescription")
    $menuItem2.FunctionName = "Open-ExtensionsLog"

    return $menuItem1, $menuItem2
}

function Open-Log
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $logPath = Join-Path $PlayniteApi.Paths.ConfigurationPath -ChildPath "playnite.log"
    if (Test-Path $logPath)
    {
        Start-Process $logPath
    }
}

function Open-ExtensionsLog
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $logPath = Join-Path $PlayniteApi.Paths.ConfigurationPath -ChildPath "extensions.log"
    if (Test-Path $logPath)
    {
        Start-Process $logPath
    }
}