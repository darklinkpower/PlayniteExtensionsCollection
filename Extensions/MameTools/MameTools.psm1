function GetMainMenuItems
{
    param(
        $menuArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemRenameRegionInNameDescription")
    $menuItem1.FunctionName = "Rename-WithInfo"
    $menuItem1.MenuSection = "@Mame Renamer"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemRenameNoRegionInNameDescription")
    $menuItem2.FunctionName = "Rename-NoInfo"
    $menuItem2.MenuSection = "@Mame Renamer"

    return $menuItem1, $menuItem2
}

function Rename-SelectedMameGames
{
    param (
        $keepRegionInfo
    )

    $mameSavedPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'mameSavedPath.txt'
    if (Test-Path $mameSavedPath)
    {
        $mamePath = [System.IO.File]::ReadAllLines($mameSavedPath)
    }
    else
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCMameExecutableSelectMessage"), "MAME renamer")
        $mamePath = $PlayniteApi.Dialogs.SelectFile("MAME executable|mame64.exe")
        if (!$mamePath)
        {
            exit
        }
        [System.IO.File]::WriteAllLines($mameSavedPath, $mamePath)
        $__logger.Info("MAME renamer - Saved `"$mamePath`" executable path.")
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCMameExecutablePathSavedMessage") -f $mamePath), "MAME renamer")
    }

    if (!(Test-Path $mamePath))
    {
        [System.IO.File]::Delete($mameSavedPath)
        $__logger.Info("MAME renamer - Executable not found in `"$mamePath`" and saved path was deleted.")
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCMameExecutableNotFoundMessage") -f $mamePath), "MAME renamer")
        exit
    }

    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.GameImagePath}
    $nameChanged = 0
    foreach ($game in $gameDatabase) {
        try {
            $fileName = [System.IO.Path]::GetFileNameWithoutExtension($game.GameImagePath)
            $arguments = @("-listxml", $fileName)
            [xml]$output = & $mamePath $arguments
            $nameInXml = $output.mame.machine[0].description
            if ($keepRegionInfo -eq $false)
            {
                $nameInXml = $nameInXml -replace " \(.+\)$", ""
            }
        } catch {
            continue
        }

        if ($game.Name -ne $nameInXml)
        {
            $game.Name = $nameInXml
            $PlayniteApi.Database.Games.Update($game)
            $__logger.Info("Changed name of `"$($game.GameImagePath)`" to `"$nameInXml`".")
            $nameChanged++
        }
    }
    
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCResultsMessage") -f $nameChanged), "MAME renamer")
    $__logger.Info("Changed the name of $nameChanged games.")
}

function Rename-WithInfo
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    Rename-SelectedMameGames $true
}

function Rename-NoInfo
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    Rename-SelectedMameGames $false
}