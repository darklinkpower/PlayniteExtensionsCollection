function GetMainMenuItems()
{
    param($menuArgs)

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Rename selected MAME games"
    $menuItem1.FunctionName = "Rename-WithInfo"
    $menuItem1.MenuSection = "@MAME Tools"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = "Rename selected MAME games (Without region information)"
    $menuItem2.FunctionName = "Rename-NoInfo"
    $menuItem2.MenuSection = "@MAME Tools"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem3.Description = "Import screenshots as Background images to Playnite from MAME of selected games"
    $menuItem3.FunctionName = "Get-MameSnapshot"
    $menuItem3.MenuSection = "@MAME Tools"

    $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem4.Description = "Import screenshots as Cover images to Playnite from MAME of selected games"
    $menuItem4.FunctionName = "Get-MameSnapshotToCover"
    $menuItem4.MenuSection = "@MAME Tools"

    $menuItem5 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem5.Description = "Remove selected entries that are MAME BIOS"
    $menuItem5.FunctionName = "Remove-MameBiosEntries"
    $menuItem5.MenuSection = "@MAME Tools"

    return $menuItem1, $menuItem2, $menuItem3, $menuItem4, $menuItem5
}

function Get-ProcessOutput
{
    param (
        $executablePath,
        $arguments
    )

    try {
        $processStartInfo = New-object System.Diagnostics.ProcessStartInfo
        $processStartInfo.CreateNoWindow = $true
        $processStartInfo.UseShellExecute = $false
        $processStartInfo.RedirectStandardOutput = $true
        $processStartInfo.RedirectStandardError = $true
        $processStartInfo.FileName = $executablePath
        $processStartInfo.Arguments = $arguments
        $process = New-Object System.Diagnostics.Process
        $process.StartInfo = $processStartInfo
        $process.Start() | Out-Null
        $output = $process.StandardOutput.ReadToEnd()
        $process.WaitForExit()
        $process.Dispose()
        return $output
    } catch {
        $process.Dispose()
        return
    }
}

function Get-MamePath
{
    $mameSavedPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'mameSavedPath.txt'
    if (Test-Path $mameSavedPath)
    {
        $mamePath = [System.IO.File]::ReadAllLines($mameSavedPath)
    }
    else
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCMameExecutableSelectMessage"), "MAME renamer")
        $mamePath = $PlayniteApi.Dialogs.SelectFile("MAME executable|mame*.exe")
        if (!$mamePath)
        {
            return
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
        return
    }
    return $mamePath
}

function Rename-SelectedMameGames
{
    param (
        $keepRegionInfo
    )

    $mamePath = Get-MamePath
    if ($null -eq $mamePath)
    {
        return
    }

    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.GameImagePath}
    $nameChanged = 0
    foreach ($game in $gameDatabase) {
        try {
            $fileName = [System.IO.Path]::GetFileNameWithoutExtension($game.GameImagePath)
            $arguments = @("-listxml", $fileName)
            [xml]$output = Get-ProcessOutput $mamePath $arguments
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
    
    $PlayniteApi.Dialogs.ShowMessage("Changed name of $nameChanged games.", "MAME Tools");
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
    $mamePath = Get-MamePath
    if ($null -eq $mamePath)
    {
        return
    }

    $mameDirectory = [System.IO.Path]::GetDirectoryName($mamePath)
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.GameImagePath}
    $screenshotAdded = 0
    $screenshotMissing = 0
    foreach ($game in $gameDatabase) {
        $fileName = [System.IO.Path]::GetFileNameWithoutExtension($game.GameImagePath)
        $sourceScreenshotPath = [System.IO.Path]::Combine($mameDirectory, "Snap", $fileName + ".png")
        if (!(Test-Path $sourceScreenshotPath))
        {
            $arguments = @("-listxml", $fileName)
            [xml]$output = Get-ProcessOutput $mamePath $arguments
            if ($output.mame.machine[0].cloneof)
            {
                $fileName = $output.mame.machine[0].cloneof
                $sourceScreenshotPath = [System.IO.Path]::Combine($mameDirectory, "Snap", $fileName + ".png")
                if (!(Test-Path $sourceScreenshotPath))
                {
                    $screenshotMissing++
                    $__logger.Info("$($game.Name) is missing a screenshot.")
                    continue
                }
            }
            else
            {
                $screenshotMissing++
                $__logger.Info("$($game.Name) is not a a clone or is missing a screenshot.")
                continue
            }
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
    $mamePath = Get-MamePath
    if ($null -eq $mamePath)
    {
        return
    }

    $mameDirectory = [System.IO.Path]::GetDirectoryName($mamePath)
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.GameImagePath}
    $screenshotAdded = 0
    $screenshotMissing = 0
    foreach ($game in $gameDatabase) {
        $fileName = [System.IO.Path]::GetFileNameWithoutExtension($game.GameImagePath)
        $sourceScreenshotPath = [System.IO.Path]::Combine($mameDirectory, "Snap", $fileName + ".png")
        if (!(Test-Path $sourceScreenshotPath))
        {
            $arguments = @("-listxml", $fileName)
            [xml]$output = Get-ProcessOutput $mamePath $arguments
            if ($output.mame.machine[0].cloneof)
            {
                $fileName = $output.mame.machine[0].cloneof
                $sourceScreenshotPath = [System.IO.Path]::Combine($mameDirectory, "Snap", $fileName + ".png")
                if (!(Test-Path $sourceScreenshotPath))
                {
                    $screenshotMissing++
                    $__logger.Info("$($game.Name) is missing a screenshot.")
                    continue
                }
            }
            else
            {
                $screenshotMissing++
                $__logger.Info("$($game.Name) is not a a clone or is missing a screenshot.")
                continue
            }
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

function Remove-MameBiosEntries
{
    $mamePath = Get-MamePath
    if ($null -eq $mamePath)
    {
        return
    }

    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.GameImagePath}
    $entriesRemoved = 0
    foreach ($game in $gameDatabase) {
        $fileName = [System.IO.Path]::GetFileNameWithoutExtension($game.GameImagePath)
        $arguments = @("-listxml", $fileName)
        [xml]$output = Get-ProcessOutput $mamePath $arguments
        if ($output.mame.machine[0].isbios -eq "yes")
        {
            $PlayniteApi.Database.Games.Remove($game.Id)
            $entriesRemoved++
        }
    }

    $PlayniteApi.Dialogs.ShowMessage("Removed $entriesRemoved entries from Playnite that were BIOS.", "MAME Tools");
}