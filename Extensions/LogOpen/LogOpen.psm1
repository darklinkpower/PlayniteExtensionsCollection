function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCLog_Open_MenuItemOpenLogDescription")
    $menuItem1.FunctionName = "Open-Log"

    return $menuItem1
}

function Open-Log
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $logPath = Join-Path $PlayniteApi.Paths.ConfigurationPath -ChildPath "Playnite.log"
    if (Test-Path $logPath)
    {
        Start-Process $logPath
    }
}