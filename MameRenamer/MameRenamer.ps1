function global:GetMainMenuItems()
{
    param($menuArgs)

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Rename selected MAME games"
    $menuItem1.FunctionName = "Rename-WithInfo"
    $menuItem1.MenuSection = "@Mame Renamer"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = "Rename selected MAME games (Without region information)"
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
        $PlayniteApi.Dialogs.ShowMessage("Select the MAME executable", "MAME renamer");
        $mamePath = $PlayniteApi.Dialogs.SelectFile("MAME executable|mame64.exe")
        if (!$mamePath)
        {
            exit
        }
        [System.IO.File]::WriteAllLines($mameSavedPath, $mamePath)
        $__logger.Info("MAME renamer - Saved `"$mamePath`" executable path.")
        $PlayniteApi.Dialogs.ShowMessage("MAME executable path `"$mamePath`" saved.", "MAME renamer")
    }

    if (!(Test-Path $mamePath))
    {
        [System.IO.File]::Delete($mameSavedPath)
        $__logger.Info("MAME renamer - Executable not found in `"$mamePath`" and saved path was deleted.")
        $PlayniteApi.Dialogs.ShowMessage("MAME executable was not found in `"$mamePath`". Please run the extension again to configure it to the correct location.", "MAME renamer")
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
    
    $PlayniteApi.Dialogs.ShowMessage("Changed name of $nameChanged games.", "MAME renamer");
    $__logger.Info("Changed the name of $nameChanged games.")
}

function Rename-WithInfo
{
    Rename-SelectedMameGames $true
}

function Rename-NoInfo
{
    Rename-SelectedMameGames $false
}