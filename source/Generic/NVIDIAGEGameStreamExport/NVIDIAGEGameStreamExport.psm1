function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCNVIDIA_GE_GameStream_Export_MenuItemExportSelectedGamesDescription")
    $menuItem1.FunctionName = "NVIDIAGameStreamExport"
    $menuItem1.MenuSection = "@NVIDIA GE GameStream Export"
    
    return $menuItem1
}

function NVIDIAGameStreamExport
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    # Load assemblies
    Add-Type -AssemblyName System.Drawing
    $imageFormat = "System.Drawing.Imaging.ImageFormat" -as [type]

    # Set paths
    $playniteExecutablePath = Join-Path -Path $PlayniteApi.Paths.ApplicationPath -ChildPath "Playnite.DesktopApp.exe"
    $nvidiaShorcutsPath = Join-Path -Path $env:LocalAppData -ChildPath "NVIDIA Corporation\Shield Apps"
    $playniteShorcutsPath = Join-Path -Path $env:LocalAppData -ChildPath "NVIDIA Corporation\Playnite Shortcuts"

    $streamingAssetsPath = Join-Path -Path $nvidiaShorcutsPath -ChildPath 'StreamingAssets'
    if (!(Test-Path $streamingAssetsPath -PathType Container))
    {
        New-Item -ItemType Container -Path $streamingAssetsPath -Force
    }

    # Set creation counter
    $shortcutsCreatedCount = 0
    
    foreach ($game in $PlayniteApi.MainView.SelectedGames) {
        # Check if game has an icon and if it's a *.ico compatible file. Else point to Playnite executable for icon
        if ([System.IO.Path]::GetExtension($game.icon) -eq ".ico") 
        {
            $iconPath = $PlayniteApi.Database.GetFullFilePath($game.icon)
        }
        else
        {
            $iconPath = $playniteExecutablePath
        }
        
        # Create url file
        $gameLaunchURI = 'playnite://playnite/start/' + "$($game.id)"
        $gameName = $($game.name).Split([IO.Path]::GetInvalidFileNameChars()) -join ''
        $urlPath = Join-Path -Path $playniteShorcutsPath -ChildPath $($($gameName -replace "[^\x00-\x7A]","-") + '.url')
        New-Item -ItemType File -Path $urlPath -Force
        "[InternetShortcut]`nIconIndex=0`nIconFile=$iconPath`nURL=$gameLaunchURI" | Out-File -Encoding 'utf8' -FilePath $urlPath

        # Create Nvidia game shortcut file in temp folder // Move after to correct path, done as a fix in case path has incompatible characters with WshShell
        $lnkPath = Join-Path -Path $nvidiaShorcutsPath -ChildPath $($gameName + '.lnk')
        $lnkTempPath = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath "LnkTmp.lnk"
        New-Item -ItemType File -Path $lnkTempPath -Force
        $wshShell = New-Object -ComObject WScript.Shell
        $shortcut = $wshShell.CreateShortcut($lnkTempPath)
        $shortcut.IconLocation = $iconPath
        $shortcut.TargetPath = $urlPath
        $shortcut.Save()
        Move-Item $lnkTempPath $lnkPath -Force

        # Set cover path and create blank file
        $nvidiaGameCoverPath = [System.IO.Path]::Combine($streamingAssetsPath, $gameName, "box-art.png")
        New-Item -ItemType File -Path $nvidiaGameCoverPath -Force
        
        $coverSet = $false
        if ($null -ne $game.CoverImage)
        {
            $sourceCover = $PlayniteApi.Database.GetFullFilePath($game.CoverImage)
            if (($game.CoverImage -notmatch "^http") -and (Test-Path $sourceCover -PathType Leaf))
            {
                if ([System.IO.Path]::GetExtension($game.CoverImage) -eq ".png")
                {
                    Copy-Item $sourceCover $nvidiaGameCoverPath -Force
                    $coverSet = $true
                }
                else
                {
                    # Convert cover image to compatible PNG image format
                    try {
                        $image = [System.Drawing.Image]::FromFile($PlayniteApi.Database.GetFullFilePath($game.CoverImage))
                        $image.Save($nvidiaGameCoverPath, $imageFormat::png)
                        $image.Dispose()
                        $coverSet = $true
                    } catch {
                        $image.Dispose()
                        $errorMessage = $_.Exception.Message
                        $__logger.Info("Error converting cover image of `"$($game.name)`". Error: $errorMessage")
                    }
                }
            }
        }

        if ($coverSet -eq $false)
        {
            # Copy Playnite blank cover to cover path if game cover was not copied or converted to png
            $sourceCover = Join-Path $PlayniteApi.Paths.ApplicationPath -ChildPath '\Themes\Desktop\Default\Images\custom_cover_background.png'
            if (Test-Path $sourceCover -PathType Leaf)
            {
                Copy-Item $sourceCover $nvidiaGameCoverPath -Force
            }
        }

        $shortcutsCreatedCount++
    }

    # Show finish dialogue with shortcut creation count
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCNVIDIA_GE_GameStream_Export_ResultsMessage") -f $shortcutsCreatedCount), "NVIDIA GameStream Export")
}