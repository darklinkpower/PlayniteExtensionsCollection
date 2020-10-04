function global:GetMainMenuItems()
{
	param($menuArgs)

	$menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
	$menuItem1.Description = "Process Images in Cache"
	$menuItem1.FunctionName = "Invoke-ImageCacheSizeSaver"
	$menuItem1.MenuSection = "@Image Cache Size Saver"

	return $menuItem1
}

function Invoke-ImageCacheSizeSaver()
{
	# Set images cache path
	if ($PlayniteApi.Paths.IsPortable -eq $true)
	{
		$PathCacheDirectory = Join-Path -Path $PlayniteApi.Paths.ApplicationPath -ChildPath "cache\images\*"
		
	}
	else
	{
		$PathCacheDirectory = Join-Path -Path $env:appdata -ChildPath "Playnite\cache\images\*"
	}
	
	# Set other paths
	$ImageTempPath = Join-Path -Path $env:temp -ChildPath 'ImageCacheSizeSaver.tmp'
	$MagickConfigPath = Join-Path -Path $PlayniteApi.Paths.ExtensionsDataPath -ChildPath 'ImageCacheSizeSaver\ConfigMagicPath.ini'
	$PreviouslyProcessedPath = Join-Path -Path $PlayniteApi.Paths.ExtensionsDataPath -ChildPath 'ImageCacheSizeSaver\ImageCacheSizeSaverList.txt'
	
	# Set Magick Executable Path
	if (Test-Path $MagickConfigPath)
	{
		$MagickExecutablePath = [System.IO.File]::ReadAllLines($MagickConfigPath)
	}
	else
	{
		$PlayniteApi.Dialogs.ShowMessage("Select ImageMagick executable", "Image Cache Size Saver")
		$MagickExecutablePath = $PlayniteApi.Dialogs.SelectFile("magick|magick.exe")
		if (!$MagickExecutablePath)
		{
			exit
		}
		New-Item -ItemType File -Path $MagickConfigPath -Force
		[System.IO.File]::WriteAllLines($MagickConfigPath, $MagickExecutablePath)
		$PlayniteApi.Dialogs.ShowMessage("Magick executable path saved", "Image Cache Size Saver")
	}

	if (!(Test-Path $MagickExecutablePath))
	{
		[System.IO.File]::Delete($MagickConfigPath)
		$PlayniteApi.Dialogs.ShowMessage("Magick executable not found at configured location. Please run the extension again to configure it to the correct location.", "Image Cache Size Saver")
		exit
	}

	# Set arrays for processed games and image extensions
	if (Test-Path $PreviouslyProcessedPath)
	{
		[System.Collections.Generic.List[string]]$PreviouslyProcessedList = @([System.IO.File]::ReadAllLines($PreviouslyProcessedPath))
	}
	else
	{
		New-Item -ItemType File -Path $PreviouslyProcessedPath -Force
		[System.Collections.Generic.List[string]]$PreviouslyProcessedList = @()
	}
	$ImageExtensions= @(
		"*.jpg",
		"*.png",
		"*.gif"
	)

	# Set Counters
	$ProcessedError = 0
	$ProcessedLessSize = 0

	# Set images to be processed and get current cache size
	$ImagesAll = Get-ChildItem -path $PathCacheDirectory -Include $ImageExtensions
	$ImagesToProcess = (Get-ChildItem -path $ImagesAll -Exclude $PreviouslyProcessedList).FullName
	[string]$ImagesSizeBefore = "{0:N2}" -f (($ImagesAll | Measure-Object -Sum Length).Sum / 1MB)
	
	foreach ($ImageSourcePath in $ImagesToProcess) {
		try {
			# Process Image with ImageMagick. Try to delete temp image for safety.
			[System.IO.File]::Delete($ImageTempPath)
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
			$ImageFileName = [System.IO.Path]::GetFileName($ImageSourcePath)
			$PreviouslyProcessedList.Add($ImageFileName)
		} catch {
			$ErrorMessage = $_.Exception.Message
			$__logger.Error("Image Cache Size Saver - `"$ImageSourcePath`" image couldn't be processed - Error: $ErrorMessage")
			$ProcessedError++
		}
	}
	
	# Write new process list, calculate Image Cache Size after processing and show results
	[System.IO.File]::WriteAllLines($PreviouslyProcessedPath, $PreviouslyProcessedList)
	[string]$ImagesSizeAfter = "{0:N2}" -f ((Get-ChildItem -path $PathCacheDirectory -Include $ImageExtensions | Measure-Object -Sum Length).Sum / 1MB)
	$PlayniteApi.Dialogs.ShowMessage("Image processing finished. Results:`n`nImages Processed: $($ImagesToProcess.count)`n`nImages that had size reduced: $ProcessedLessSize`nErrors: $ProcessedError`n`nImage Cache Size Before: $ImagesSizeBefore MB`nImage Cache Size After: $ImagesSizeAfter MB", "Image Cache Size Saver")
}