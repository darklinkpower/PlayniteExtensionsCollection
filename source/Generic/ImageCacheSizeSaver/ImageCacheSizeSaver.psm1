function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCImage_Cache_Size_Saver_MenuItemProcessImagesDescription")
    $menuItem1.FunctionName = "Invoke-ImageCacheSizeSaver"
    $menuItem1.MenuSection = "@Image Cache Size Saver"

    return $menuItem1
}

function Invoke-ImageCacheSizeSaver
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    # Set images cache path
    if ($PlayniteApi.Paths.IsPortable -eq $true)
    {
        $__logger.Info("Image Cache Size Saver - Playnite is Portable.")
        $PathCacheDirectory = Join-Path -Path $PlayniteApi.Paths.ApplicationPath -ChildPath "cache\images\*"
    }
    else
    {
        $__logger.Info("Image Cache Size Saver - Playnite is Installed.")
        $PathCacheDirectory = Join-Path -Path $env:APPDATA -ChildPath "Playnite\cache\images\*"
    }

    # Try to get magick.exe path via registry
    $Key = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine, [Microsoft.Win32.RegistryView]::Registry64)
    $RegSubKey =  $Key.OpenSubKey("Software\ImageMagick\Current")
    if ($RegSubKey)
    {
        $RegInstallDir = $RegSubKey.GetValue("BinPath")
        if ($RegInstallDir)
        {
            $MagickExecutable = Join-Path -Path $RegInstallDir -ChildPath 'magick.exe'
            if (Test-Path $MagickExecutable)
            {
                $MagickExecutablePath = $MagickExecutable
                $__logger.Info("Image Cache Size Saver - Found executable Path via registry in `"$MagickExecutablePath`".")
            }
        }
    }

    if ($null -eq $MagickExecutablePath)
    {
        # Set Magick Executable Path via user Input
        $MagickConfigPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'ConfigMagicPath.ini'
        if (Test-Path $MagickConfigPath)
        {
            $MagickExecutablePath = [System.IO.File]::ReadAllLines($MagickConfigPath)
        }
        else
        {
            $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCImage_Cache_Size_Saver_MagickExecutableSelectMessage"), "Image Cache Size Saver")
            $MagickExecutablePath = $PlayniteApi.Dialogs.SelectFile("magick|magick.exe")
            if (!$MagickExecutablePath)
            {
                exit
            }
            [System.IO.File]::WriteAllLines($MagickConfigPath, $MagickExecutablePath)
            $__logger.Info("Image Cache Size Saver - Saved executable Path via user input in `"$MagickExecutablePath`".")
            $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCImage_Cache_Size_Saver_MagickExecutablePathSavedMessage"), "Image Cache Size Saver")
        }

        if (!(Test-Path $MagickExecutablePath))
        {
            [System.IO.File]::Delete($MagickConfigPath)
            $__logger.Info("Image Cache Size Saver - Executable not found in user configured path `"$MagickExecutablePath`".")
            $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCImage_Cache_Size_Saver_MagickExecutableNotFoundMessage") -f $MagickExecutablePath), "Image Cache Size Saver")
            exit
        }
    }

    # Set arrays for processed games and image extensions
    $previouslyProcessedPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'ImageCacheSizeSaverList.txt'
    $processedImagesHashtable = @{}
    if (Test-Path $previouslyProcessedPath)
    {
        $__logger.Info("Image Cache Size Saver - Found existing processed list")
        @([System.IO.File]::ReadAllLines($previouslyProcessedPath)) | ForEach-Object {
            $processedImagesHashtable.add($_, "")
        }
    }
    
    # Set Counters
    $processedError = 0
    $processedLessSize = 0
    $processedImagesCount= 0

    # Set images to be processed and get current cache size
    $imagesObjectList = Get-ChildItem -path $PathCacheDirectory
    [string]$ImagesSizeBefore = "{0:N2}" -f (($imagesObjectList | Measure-Object -Sum Length).Sum / 1MB)
    $ImageTempPath = Join-Path -Path $env:temp -ChildPath 'ImageCacheSizeSaver.tmp'
    
    foreach ($imageObject in $imagesObjectList)
    {
        if ($null -ne $processedImagesHashtable[$imageObject.Name])
        {
            continue
        }
        
        $imageSourcePath = $imageObject.FullName
        try {
            # Process Image with ImageMagick. Try to delete temp image for safety.
            $processedImagesCount++
            & "$MagickExecutablePath" "$ImageSourcePath[0]" $ImageTempPath

            # Overwrite original image if it's bigger than processed image
            if ( ((Get-Item $ImageTempPath).length) -lt ((Get-Item $ImageSourcePath).length) )
            {
                [System.IO.File]::Delete($ImageSourcePath)
                [System.IO.File]::Move($ImageTempPath, $ImageSourcePath)
                $ProcessedLessSize++
            }
            else
            {
                [System.IO.File]::Delete($ImageTempPath)
            }

            # Add to processed list
            $processedImagesHashtable.add($imageObject.Name, "")
        } catch {
            $processedImagesHashtable.add($imageObject.Name, "")
            $ErrorMessage = $_.Exception.Message
            $__logger.Error("Image Cache Size Saver - `"$ImageSourcePath`" image couldn't be processed - Error: $ErrorMessage")
            $ProcessedError++
            [System.IO.File]::Delete($ImageTempPath)
        }
    }
    
    # Write new process list, calculate Image Cache Size after processing and show results
    if ($processedImagesCount -gt 0)
    {
        $processedImagesList = $processedImagesHashtable.GetEnumerator() | ForEach-Object { "$($_.Key)" }
        Set-Clipboard $processedImagesList
        [System.IO.File]::WriteAllLines($PreviouslyProcessedPath, $processedImagesList)
    }
    
    [string]$ImagesSizeAfter = "{0:N2}" -f ((Get-ChildItem -path $PathCacheDirectory | Measure-Object -Sum Length).Sum / 1MB)
    $__logger.Info("Image Cache Size Saver - Image processing finished. Images Processed: $processedImagesCount. Images that had size reduced: $ProcessedLessSize. Errors: $ProcessedError. Image Cache Size Before: $ImagesSizeBefore MB. Image Cache Size After: $ImagesSizeAfter MB")
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCImage_Cache_Size_Saver_ResultsMessage") -f $processedImagesCount, $ProcessedLessSize, $ProcessedError, $ImagesSizeBefore, $ImagesSizeAfter), "Image Cache Size Saver")
}