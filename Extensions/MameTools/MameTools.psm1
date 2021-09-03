function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemRenameRegionInNameDescription")
    $menuItem1.FunctionName = "Rename-WithInfo"
    $menuItem1.MenuSection = "@Mame Tools"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemRenameNoRegionInNameDescription")
    $menuItem2.FunctionName = "Rename-NoInfo"
    $menuItem2.MenuSection = "@Mame Tools"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem3.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemSetSnapAsBackgroundDescription")
    $menuItem3.FunctionName = "Set-SnapshotAsCoverImage"
    $menuItem3.MenuSection = "@Mame Tools"

    $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem4.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemSetSnapAsCoverDescription")
    $menuItem4.FunctionName = "Set-SnapshotAsBackgroundImage"
    $menuItem4.MenuSection = "@Mame Tools"

    $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem4.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemSetSnapAsCoverDescription")
    $menuItem4.FunctionName = "Set-SnapshotAsBackgroundImage"
    $menuItem4.MenuSection = "@Mame Tools"

    return $menuItem1, $menuItem2, $menuItem3, $menuItem4
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
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCMameExecutableSelectMessage"), "MAME Tools")
        $mamePath = $PlayniteApi.Dialogs.SelectFile("MAME executable|mame*.exe")
        if (!$mamePath)
        {
            return
        }
        [System.IO.File]::WriteAllLines($mameSavedPath, $mamePath)
        $__logger.Info("MAME renamer - Saved `"$mamePath`" executable path.")
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCMameExecutablePathSavedMessage") -f $mamePath), "MAME Tools")
    }

    if (!(Test-Path $mamePath))
    {
        [System.IO.File]::Delete($mameSavedPath)
        $__logger.Info("MAME renamer - Executable not found in `"$mamePath`" and saved path was deleted.")
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCMameExecutableNotFoundMessage") -f $mamePath), "MAME Tools")
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

    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    $nameChanged = 0
    foreach ($game in $gameDatabase) {
        if ($null -eq $game.GameActions)
        {
            continue
        }
        elseif ($game.GameActions[0].Type -ne [Playnite.SDK.Models.GameActionType]::Emulator)
        {
            continue
        }
        elseif ($null -eq $game.Roms)
        {
            continue
        }
        $fileName = [System.IO.Path]::GetFileNameWithoutExtension($game.Roms[0].Path)
        try {
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
            $__logger.Info("Changed name of `"$fileName`" to `"$nameInXml`".")
            $nameChanged++
        }
    }
    
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCResultsMessage") -f $nameChanged), "MAME Tools")
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

function Set-MameSnapshotToPlayniteMedia
{
    param (
        $targetMedia
    )
    
    $mamePath = Get-MamePath
    if ($null -eq $mamePath)
    {
        return
    }

    $mameDirectory = [System.IO.Path]::GetDirectoryName($mamePath)
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    $snapshotsImportCount = 0
    $snapshotsMissingCount = 0
    foreach ($game in $gameDatabase) {
        if ($null -eq $game.GameActions)
        {
            continue
        }
        elseif ($game.GameActions[0].Type -ne [Playnite.SDK.Models.GameActionType]::Emulator)
        {
            continue
        }
        elseif ($null -eq $game.Roms)
        {
            continue
        }
        $fileName = [System.IO.Path]::GetFileNameWithoutExtension($game.Roms[0].Path)
        $sourceScreenshotPath = [System.IO.Path]::Combine($mameDirectory, "Snap", $fileName + ".png")
        if (![System.IO.File]::Exists($sourceScreenshotPath))
        {
            try {
                $arguments = @("-listxml", $fileName)
                [xml]$output = Get-ProcessOutput $mamePath $arguments
                if ($output.mame.machine[0].cloneof)
                {
                    $fileName = $output.mame.machine[0].cloneof
                    $sourceScreenshotPath = [System.IO.Path]::Combine($mameDirectory, "Snap", $fileName + ".png")
                    if (![System.IO.File]::Exists($sourceScreenshotPath))
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
            } catch {
                continue
            }
        }

        if ($targetMedia -eq "Background Image")
        {
            if ($game.BackgroundImage)
            {
                $PlayniteApi.Database.RemoveFile($game.BackgroundImage)
                $PlayniteApi.Database.Games.Update($game)
            }
            $copiedImage = $PlayniteApi.Database.AddFile($sourceScreenshotPath, $game.Id)
            $game.BackgroundImage = $copiedImage
            $PlayniteApi.Database.Games.Update($game)
            $snapshotsImportCount++
        }
        elseif ($targetMedia -eq "Cover Image")
        {
            if ($game.CoverImage)
            {
                $PlayniteApi.Database.RemoveFile($game.CoverImage)
                $PlayniteApi.Database.Games.Update($game)
            }
            $copiedImage = $PlayniteApi.Database.AddFile($sourceScreenshotPath, $game.Id)
            $game.CoverImage = $copiedImage
            $PlayniteApi.Database.Games.Update($game)
            $snapshotsImportCount++
        }

    }

    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSnapshotsImportResultsMessage") -f $snapshotsImportCount, $snapshotsMissingCount), "MAME Tools")
}

function Set-SnapshotAsCoverImage
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    Set-MameSnapshotToPlayniteMedia "Background Image"
}

function Set-SnapshotAsBackgroundImage
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    Set-MameSnapshotToPlayniteMedia "Cover Image"
}