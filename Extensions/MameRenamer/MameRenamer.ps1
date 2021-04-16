function GetMainMenuItems()
{
    param($menuArgs)

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Rename selected MAME games"
    $menuItem1.FunctionName = "Rename-WithInfo"
    $menuItem1.MenuSection = "@MAME tools"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = "Rename selected MAME games (Without region information)"
    $menuItem2.FunctionName = "Rename-NoInfo"
    $menuItem2.MenuSection = "@MAME tools"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem3.Description = "Import screenshots as Background images to Playnite from MAME of selected games"
    $menuItem3.FunctionName = "Get-MameSnapshot"
    $menuItem3.MenuSection = "@MAME tools"

    $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem4.Description = "Import screenshots as Cover images to Playnite from MAME of selected games"
    $menuItem4.FunctionName = "Get-MameSnapshotToCover"
    $menuItem4.MenuSection = "@MAME tools"

    return $menuItem1, $menuItem2, $menuItem3, $menuItem4
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
        $mamePath = $PlayniteApi.Dialogs.SelectFile("MAME executable|mame*.exe")
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

function Get-MameSnapshot
{
    $mameSavedPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'mameSavedPath.txt'
    if (Test-Path $mameSavedPath)
    {
        $mamePath = [System.IO.File]::ReadAllLines($mameSavedPath)
    }
    else
    {
        $PlayniteApi.Dialogs.ShowMessage("Select the MAME executable", "MAME renamer");
        $mamePath = $PlayniteApi.Dialogs.SelectFile("MAME executable|mame*.exe")
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

    $mameDirectory = [System.IO.Path]::GetDirectoryName($mamePath)
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.GameImagePath}
    $screenshotAdded = 0
    $screenshotMissing = 0
    foreach ($game in $gameDatabase) {
        $romFileName = [System.IO.Path]::GetFileNameWithoutExtension($game.GameImagePath)
        $sourceScreenshotPath = [System.IO.Path]::Combine($mameDirectory, "Snap", $romFileName + ".png")
        if (!(Test-Path $sourceScreenshotPath))
        {
            $screenshotMissing++
            $__logger.Info("$($game.Name) is not a MAME game or is missing a screenshot.")
            continue
        }

        if ($game.BackgroundImage)
        {
            $PlayniteApi.Database.RemoveFile($game.BackgroundImage)
            $PlayniteApi.Database.Games.Update($game)
        }
        $copiedImage = $PlayniteApi.Database.AddFile($sourceScreenshotPath, $game.Id)
        $game.BackgroundImage = $copiedImage
        $PlayniteApi.Database.Games.Update($game)
        $screenshotAdded++
    }

    $PlayniteApi.Dialogs.ShowMessage("Imported Screenshots of $screenshotAdded games to Playnite.`n$screenshotMissing have a missing screenshot or are not MAME games.", "MAME screenshot importer");
}

function Get-MameSnapshotToCover
{
    $mameSavedPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'mameSavedPath.txt'
    if (Test-Path $mameSavedPath)
    {
        $mamePath = [System.IO.File]::ReadAllLines($mameSavedPath)
    }
    else
    {
        $PlayniteApi.Dialogs.ShowMessage("Select the MAME executable", "MAME renamer");
        $mamePath = $PlayniteApi.Dialogs.SelectFile("MAME executable|mame*.exe")
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

    $mameDirectory = [System.IO.Path]::GetDirectoryName($mamePath)
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.GameImagePath}
    $screenshotAdded = 0
    $screenshotMissing = 0
    foreach ($game in $gameDatabase) {
        $romFileName = [System.IO.Path]::GetFileNameWithoutExtension($game.GameImagePath)
        $sourceScreenshotPath = [System.IO.Path]::Combine($mameDirectory, "Snap", $romFileName + ".png")
        if (!(Test-Path $sourceScreenshotPath))
        {
            $screenshotMissing++
            $__logger.Info("$($game.Name) is not a MAME game or is missing a screenshot.")
            continue
        }

        if ($game.CoverImage)
        {
            $PlayniteApi.Database.RemoveFile($game.CoverImage)
            $PlayniteApi.Database.Games.Update($game)
        }
        $copiedImage = $PlayniteApi.Database.AddFile($sourceScreenshotPath, $game.Id)
        $game.CoverImage = $copiedImage
        $PlayniteApi.Database.Games.Update($game)
        $screenshotAdded++
    }

    $PlayniteApi.Dialogs.ShowMessage("Imported Screenshots of $screenshotAdded games to Playnite.`n$screenshotMissing have a missing screenshot or are not MAME games.", "MAME screenshot importer");
}