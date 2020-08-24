function global:NVIDIAGEGameStreamExport()
{
	# Set PlayniteExecutablePath
	$PlayniteExecutablePath = Join-Path -Path $PlayniteApi.Paths.ApplicationPath -ChildPath "Playnite.DesktopApp.exe"
	
	# Set GameDatabase
	$GameDatabase = $PlayniteApi.MainView.SelectedGames
	
	# Set NVIDIA GameStream directory
	$NVIDIAPath = Join-Path $env:LocalAppData -ChildPath "NVIDIA Corporation\Shield Apps"
	
	# Set creation count
	$count = 0
	
	foreach ($Game in $GameDatabase) {
		
		#Set game launch URI
		$URI = 'playnite://playnite/start/' + "$($game.id)"
		
		# Set shortcut path based on Windows Explorer compatible game name
		$GameName = $($Game.name).Split([IO.Path]::GetInvalidFileNameChars()) -join ''
		$NVIDIAShortcutPath = Join-Path -Path $NVIDIAPath -ChildPath $("$($GameName)" + '.lnk')
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
		
		# Create game .lnk file
		$shell = New-Object -ComObject WScript.Shell
		$shortcut = $shell.CreateShortcut("$NVIDIAShortcutPath")
		$shortcut.IconLocation = $IconPath
		$shortcut.TargetPath = Join-Path $env:SystemRoot -ChildPath 'System32\cmd.exe'
		$shortcut.WorkingDirectory = Join-Path $env:SystemRoot -ChildPath 'System32'
		$shortcut.Arguments = "/min /q /c start $URI"
		$shortcut.Save()
		
		# Set cover path and create blank file
		$NVIDIACover = Join-Path $NVIDIAPath -ChildPath 'StreamingAssets' | Join-Path -ChildPath $GameName | Join-Path -ChildPath '\box-art.png'
		New-Item -ItemType File -Path $NVIDIACover -Force
		
		# Check if game has a cover image
		if ($($game.CoverImage))
		{
			if ($($game.CoverImage) -match '.+\.png$')
			{
				$SourceCover = $PlayniteApi.Database.GetFullFilePath($game.CoverImage)
				Copy-Item $SourceCover $NVIDIACover -Force
			}
			else
			{
				# Convert cover image to compatible PNG
				try {
					$SourceCover = $PlayniteApi.Database.GetFullFilePath($game.CoverImage)
					Add-Type -AssemblyName system.drawing
					$imageFormat = “System.Drawing.Imaging.ImageFormat” -as [type]
					$image = [drawing.image]::FromFile($SourceCover)
					$image.Save($NVIDIACover, $imageFormat::png)
				} catch {
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
		$count++
		$SourceCover = $null
	}
	# Show finish dialogue with shortcut creation count
	$PlayniteApi.Dialogs.ShowMessage("NVIDIA Geforce Experience GameStream shortcut created for $count selected games.");
}