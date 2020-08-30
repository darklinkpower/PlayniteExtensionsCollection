function global:NVIDIAGameStreamExport()
{
	# Set paths
	$PlayniteExecutablePath = Join-Path -Path $PlayniteApi.Paths.ApplicationPath -ChildPath "Playnite.DesktopApp.exe"
	$NvidiaGameStreamPath = Join-Path -Path $env:LocalAppData -ChildPath "NVIDIA Corporation\Shield Apps"

	# Set GameDatabase
	$GameDatabase = $PlayniteApi.MainView.SelectedGames
	
	# Set creation counter
	$ShortcutsCreatedCount = 0
	
	foreach ($Game in $GameDatabase) {
		
		# Set game launch URI and Game Name
		$GameLaunchURI = 'playnite://playnite/start/' + "$($game.id)"
		$GameName = $($Game.name).Split([IO.Path]::GetInvalidFileNameChars()) -join ''

		# Check if game has an icon and if it's a *.ico compatible file. Else point to Playnite executable for icon
		if ($($game.icon) -match '.+\.ico$') {
			$IconPath = $PlayniteApi.Database.GetFullFilePath($game.icon)
		}
		else
		{
			$IconPath = $PlayniteExecutablePath
		}
		
		# Create url file
		$UrlPath = Join-Path -Path $NvidiaGameStreamPath -ChildPath $($GameName + '.url')
		New-Item -ItemType File -Path $UrlPath -Force
		"[InternetShortcut]`nIconIndex=0`nIconFile=$IconPath`nURL=$GameLaunchURI" | Out-File -Encoding 'utf8' -FilePath $UrlPath
		
		# Set cover path and create blank file
		$NvidiaGameCoverPath = Join-Path -Path $NvidiaGameStreamPath -ChildPath 'StreamingAssets' | Join-Path -ChildPath $GameName | Join-Path -ChildPath '\box-art.png'
		New-Item -ItemType File -Path $NvidiaGameCoverPath -Force
		
		# Check if game has a cover image
		if ($game.CoverImage)
		{
			if ($game.CoverImage -match '.+\.png$')
			{
				$SourceCover = $PlayniteApi.Database.GetFullFilePath($game.CoverImage)
				Copy-Item $SourceCover $NvidiaGameCoverPath -Force
			}
			else
			{
				# Convert cover image to compatible PNG image format
				try {
					$SourceCover = $PlayniteApi.Database.GetFullFilePath($game.CoverImage)
					Add-Type -AssemblyName system.drawing
					$imageFormat = “System.Drawing.Imaging.ImageFormat” -as [type]
					$image = [drawing.image]::FromFile($SourceCover)
					$image.Save($NvidiaGameCoverPath, $imageFormat::png)
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
				Copy-Item $SourceCover $NvidiaGameCoverPath -Force
			}
		}
		
		# Increase creation count and null $SourceCover
		$ShortcutsCreatedCount++
		$SourceCover = $null
	}

	# Show finish dialogue with shortcut creation count
	$PlayniteApi.Dialogs.ShowMessage("NVIDIA GameStream game shortcuts created: $ShortcutsCreatedCount", "NVIDIA GameStream Export");
}