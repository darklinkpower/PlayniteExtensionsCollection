function global:NVIDIAGameStreamExport()
{
	# Set PlayniteExecutablePath
	$PlayniteExecutablePath = Join-Path -Path $PlayniteApi.Paths.ApplicationPath -ChildPath "Playnite.DesktopApp.exe"
	
	# Set GameDatabase
	$GameDatabase = $PlayniteApi.MainView.SelectedGames
	
	# Set NVIDIA GameStream directory
	$NVIDIAPath = Join-Path $env:LocalAppData -ChildPath "NVIDIA Corporation\Shield Apps"
	
	# Set creation count
	$ShortcutsCreatedCount = 0
	
	foreach ($Game in $GameDatabase) {
		
		#Set game launch URI
		$GameLaunchURI = 'playnite://playnite/start/' + "$($game.id)"
		
		# Set shortcut path based on Windows Explorer compatible game name
		$GameName = $($Game.name).Split([IO.Path]::GetInvalidFileNameChars()) -join ''
		$NVIDIAShortcutPath = Join-Path -Path $NVIDIAPath -ChildPath $($GameName + '.lnk')
		New-Item -ItemType File -Path $NVIDIAShortcutPath -Force
		
		# Check if game has an icon and if it's a *.ico compatible file. Else point to Playnite executable for icon
		if ($($game.icon) -match '.+\.ico$')
		{
			$IconPath = $PlayniteApi.Database.GetFullFilePath($game.icon)
		}
		else
		{
			$IconPath = $PlayniteExecutablePath
		}
		
		# Create game shortcut file
		$shell = New-Object -ComObject WScript.Shell
		$shortcut = $shell.CreateShortcut($NVIDIAShortcutPath)
		$shortcut.IconLocation = $IconPath
		$shortcut.TargetPath = Join-Path -Path $env:SystemRoot -ChildPath "explorer.exe"
		$shortcut.Arguments = $GameLaunchURI
		$shortcut.WorkingDirectory = $env:SystemRoot
		$shortcut.WindowStyle = 7
		$shortcut.Save()
		
		# Set cover path and create blank file
		$NVIDIACover = Join-Path $NVIDIAPath -ChildPath 'StreamingAssets' | Join-Path -ChildPath $GameName | Join-Path -ChildPath '\box-art.png'
		New-Item -ItemType File -Path $NVIDIACover -Force
		
		# Check if game has a cover image
		if ($game.CoverImage)
		{
			if ($game.CoverImage -match '.+\.png$')
			{
				$SourceCover = $PlayniteApi.Database.GetFullFilePath($game.CoverImage)
				Copy-Item $SourceCover $NVIDIACover -Force
			}
			else
			{
				# Convert cover image to compatible PNG image format
				try {
					$SourceCover = $PlayniteApi.Database.GetFullFilePath($game.CoverImage)
					Add-Type -AssemblyName system.drawing
					$imageFormat = “System.Drawing.Imaging.ImageFormat” -as [type]
					$image = [drawing.image]::FromFile($SourceCover)
					$image.Save($NVIDIACover, $imageFormat::png)
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
				Copy-Item $SourceCover $NVIDIACover -Force
			}
		}
		
		#Increase creation count and null $SourceCover
		$ShortcutsCreatedCount++
		$SourceCover = $null
	}

	# Show finish dialogue with shortcut creation count
	$PlayniteApi.Dialogs.ShowMessage("NVIDIA GameStream game shortcuts created: $ShortcutsCreatedCount", "NVIDIA GameStream Export");
}