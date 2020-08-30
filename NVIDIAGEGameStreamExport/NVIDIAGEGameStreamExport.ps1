function global:NVIDIAGameStreamExport()
{
	# Set PlayniteExecutablePath
	$PlayniteExecutablePath = Join-Path -Path $PlayniteApi.Paths.ApplicationPath -ChildPath "Playnite.DesktopApp.exe"
	
	# Set GameDatabase
	$GameDatabase = $PlayniteApi.MainView.SelectedGames
	
	# Set paths
	$NvidiaPath = Join-Path -Path $env:LocalAppData -ChildPath "NVIDIA Corporation\Shield Apps"
	$UrlsDirectoryPath =  Join-Path -Path $PlayniteApi.Paths.ExtensionsDataPath -ChildPath "NVIDIAGameStreamExport"

	# Set creation count
	$ShortcutsCreatedCount = 0
	
	foreach ($Game in $GameDatabase) {
		
		#Set game launch URI and Game Name
		$GameLaunchURI = 'playnite://playnite/start/' + "$($game.id)"
		$GameName = $($Game.name).Split([IO.Path]::GetInvalidFileNameChars()) -join '' -replace "[^\x00-\x7A]",""
			
		# Check if game has an icon and if it's a *.ico compatible file. Else point to Playnite executable for icon
		if ($($game.icon) -match '.+\.ico$') 
		{
			$IconPath = $PlayniteApi.Database.GetFullFilePath($game.icon)
		}
		else
		{
			$IconPath = $PlayniteExecutablePath
		}
		
		# Create url file
		$UrlPath = Join-Path -Path $UrlsDirectoryPath -ChildPath $($GameName + '.url')
		New-Item -ItemType File -Path $UrlPath -Force
		"[InternetShortcut]`nIconIndex=0`nIconFile=$IconPath`nURL=$GameLaunchURI" | Out-File -Encoding 'utf8' -FilePath $UrlPath

		# Create Nvidia game shortcut file
		$NvidiaShortcutPath = Join-Path -Path $NvidiaPath -ChildPath $($GameName + '.lnk')
		New-Item -ItemType File -Path $NvidiaShortcutPath -Force
		$shell = New-Object -ComObject WScript.Shell
		$shortcut = $shell.CreateShortcut($NvidiaShortcutPath)
		$shortcut.IconLocation = $IconPath
		$shortcut.TargetPath = $UrlPath
		$shortcut.Arguments = $GameLaunchURI
		$shortcut.WorkingDirectory = $NvidiaPath
		$shortcut.Save()
		
		# Set cover path and create blank file
		$NvidiaCover = Join-Path -Path $NvidiaPath -ChildPath 'StreamingAssets' | Join-Path -ChildPath $GameName | Join-Path -ChildPath '\box-art.png'
		New-Item -ItemType File -Path $NvidiaCover -Force
		
		# Check if game has a cover image
		if ($game.CoverImage)
		{
			if ($game.CoverImage -match '.+\.png$')
			{
				$SourceCover = $PlayniteApi.Database.GetFullFilePath($game.CoverImage)
				Copy-Item $SourceCover $NvidiaCover -Force
			}
			else
			{
				# Convert cover image to compatible PNG image format
				try {
					$SourceCover = $PlayniteApi.Database.GetFullFilePath($game.CoverImage)
					Add-Type -AssemblyName system.drawing
					$imageFormat = “System.Drawing.Imaging.ImageFormat” -as [type]
					$image = [drawing.image]::FromFile($SourceCover)
					$image.Save($NvidiaCover, $imageFormat::png)
				} catch {
					$ErrorMessage = $_.Exception.Message
					$__logger.Info("NVIDIA GameStream Export - Error converting cover image of `"$($game.name)`". Error: $ErrorMessage")
					$SourceCover = $null
				}
			}
		}

		if (!$SourceCover)
		{
			# Copy Playnite blank cover to cover path if game cover was not copied or converted to png
			$SourceCover = Join-Path $PlayniteApi.Paths.ApplicationPath -ChildPath '\Themes\Desktop\Default\Images\custom_cover_background.png'
			if (Test-Path $SourceCover)
			{
				Copy-Item $SourceCover $NvidiaCover -Force
			}
		}
		
		#Increase creation count and null $SourceCover
		$ShortcutsCreatedCount++
		$SourceCover = $null
	}

	# Show finish dialogue with shortcut creation count
	$PlayniteApi.Dialogs.ShowMessage("NVIDIA GameStream game shortcuts created: $ShortcutsCreatedCount", "NVIDIA GameStream Export");
}