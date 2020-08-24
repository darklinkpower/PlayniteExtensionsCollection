function GameMediaTools()
{
	# Set Log Path
	# $LogPath = Join-Path $PlayniteApi.Paths.ApplicationPath -ChildPath "PlayniteExtensionTests.log"
	
	# Set Error Log Path and start log
	$ErrorPath = Join-Path $PlayniteApi.Paths.ApplicationPath -ChildPath "GameMediaToolsErrors.log"
	"-------------------------- $(Get-Date -Format "yyyy/MM/dd HH:mm:ss") | INFO: GameMediaTools 1.1 Error Log --------------------------"  | Out-File -Encoding 'UTF8' -FilePath $ErrorPath
	
	# Set Counters
	$CountNoMedia = 0
	$CountNoMediaBefore = 0
	$CountTagAdded = 0
	$CountTagRemoved = 0
	$CountIncompatibleFile = 0
	
	# Set GameDatabase via input window
	$InputGameDatabase = $PlayniteApi.Dialogs.SelectString(
		"Enter a number to select the games to process:
		`n`"1`" - for games selected in Playnite UI
		`n`"2`" - for all games in Playnite database"
	, "Select game database", "");
	if ("$($InputGameDatabase.result)" -eq "True")
	{
		switch ("$($InputGameDatabase.SelectedString)")
		{
			1 { $GameDatabase = $PlayniteApi.MainView.SelectedGames }
			2 { $GameDatabase = $PlayniteApi.Database.Games }
			default {
				$PlayniteApi.Dialogs.ShowMessage("Invalid input");
				exit
			}
		}
	}
	else
	{
		exit
	}
	
	# Set Media Type via input window
	$InputMediaType = $PlayniteApi.Dialogs.SelectString(
		"Enter a number to select the media type to process:
		`n`"1`" - for Cover images
		`n`"2`" - for Backgrounds images
		`n`"3`" - for Icons"
	, "Select media type", "");

	if ("$($InputMediaType.result)" -eq "True")
	{
		switch ("$($InputMediaType.SelectedString)")
		{
			1 { $MediaType = "Cover" }
			2 { $MediaType = "Background" }
			3 { $MediaType = "Icon" }
			default {
				$PlayniteApi.Dialogs.ShowMessage("Invalid input");
				exit
			}
		}
	}
	else
	{
		exit
	}
	
	# Set aspect ratio or resolution via input window
	$InputAspectResolution = $PlayniteApi.Dialogs.SelectString(
		"Enter to select a tool option:
		`n`"0`" - to only detect games that are missing  the selected media
		`n`"{width}:{height}`" (i.e. 16:9) - to detect games not matching entered aspect ratio
		`n`"{width}x{height}`" (i.e. 1920x1080) - to detect games not matching entered resolution
		`n`"+{number}`" (i.e. +1024) - to detect games larger than that size in kb
		`n`"Ext: {extension}`" (i.e. Ext: jpg) - to detect games with given file extension"
	, "Select tool", "");
	
	if ("$($InputAspectResolution.result)" -eq "True")
	{
		# Check if input was aspect ratio
		if ( "$($InputAspectResolution.SelectedString)" -match '^\d+:\d+$')
		{
			switch -regex ("$($InputAspectResolution.SelectedString)")
			{
				'^\d+' {
					[int]$width = $Matches[0]
				}
				'\d+$' {
					[int]$height = $Matches[0]
				}
			}
			[double]$AspectRatio = $Height / $width
			$ToMatch = "AR " + "$Width" + ':' + "$height"
			$ToMatchReport = "Image aspect ratio: " + "$Width" + ':' + "$height"
			$Tool = "Aspect ratio match"
			$Mode = "Exclusive"
		}
		# Check if input was resolution
		elseif ( "$($InputAspectResolution.SelectedString)" -match '^\d+x\d+$')
		{
			switch -regex ("$($InputAspectResolution.SelectedString)")
			{
				'^\d+' {
					[int]$width = $Matches[0]
				}
				'\d+$' {
					[int]$height = $Matches[0]
				}
			}
			$ToMatch = "Res. " + "$Width" + 'x' + "$height"
			$ToMatchReport = "Image resolution: " + "$Width" + 'x' + "$height"
			$Tool = "Resolution match"
			$Mode = "Exclusive"
		}
		
		# Check if input was 'kb' size
		elseif ( "$($InputAspectResolution.SelectedString)" -match '^[+]\d+$')
		{
			switch -regex ("$($InputAspectResolution.SelectedString)")
			{
				'^[+](\d+)' {
					[int]$MoreTKB = $Matches[0]
				}
			}
			$ToMatch = "Size" + ">" + "$($MoreTKB)" + "kb"
			$ToMatchReport = "Image size" + ">" + "$($MoreTKB)" + "kb"
			$Tool = "Size Bigger Than"
			$Mode = "Inclusive"
		}

		# Check if input was file extension
		elseif ( "$($InputAspectResolution.SelectedString)" -match '^ext:\s?\S+$')
		{
			switch -regex ("$($InputAspectResolution.SelectedString)")
			{
				'^ext:\s?(\S+)$' {
					[string]$MatchExt = $Matches[1]
				}
			}
			$ToMatch = "ImgExt " + "$($MatchExt)"
			$ToMatchReport = "Image extension: " + "$($MatchExt)"
			$Tool = "Image Extension"
			$Mode = "Inclusive"
		}
		
		# Check no Tool has been set and value '0' was entered to only check for missing media
		elseif ("$($InputAspectResolution.SelectedString)" -match '^0$')
		{
			$Tool = "Missing media"
			$Mode = "Missing Media"
		}
		
		else
		{
			# Exit execution if invalid input
			$PlayniteApi.Dialogs.ShowMessage("Invalid input");
			exit
		}
	}
	else
	{
		exit
	}
	
	# Create No media tag
	$tagNoMediaName = "No Media: " + "$($MediaType)"
	$tagNoMedia = $PlayniteApi.Database.tags.Add($tagNoMediaName)
	[guid[]]$tagNoMediaIds = $tagNoMedia.Id
	
	# Create tag name
	if ($Mode -eq "Exclusive")
	{
		$tagMatchName = "No Match: " + "$($MediaType) " + "$($ToMatch)"

	}
	elseif($Mode -eq "Inclusive")
	{
		$tagMatchName = "Match: " + "$($MediaType) " + "$($ToMatch)"
	}
	
	# Create tag
	if ($tagMatchName)
	{
		$tagMatch = $PlayniteApi.Database.tags.Add($tagMatchName)
		[guid[]]$tagMatchIds = $tagMatch.Id
	}
	
	foreach ($Game in $GameDatabase) {
		# Remove variables from previous loop
		$MatchStatus = $null
		$imageFile = $null
		
		# Verify selected media type, if game has it and get full file path
		if ( ($MediaType -eq "Cover") -and ($game.CoverImage) )
		{
			$imageFile = $PlayniteApi.Database.GetFullFilePath($game.CoverImage)
		}
		elseif ( ($MediaType -eq "Background") -and ($game.BackgroundImage) )
		{
			$imageFile = $PlayniteApi.Database.GetFullFilePath($game.BackgroundImage)
		}
		elseif ( ($MediaType -eq "Icon") -and ($game.Icon) )
		{
			$imageFile = $PlayniteApi.Database.GetFullFilePath($game.Icon)
		}

		# Verify if imagefile path was obtained
		if ($imageFile)
		{

			# Check if game already has No Media tag and remove if true
			if ($game.tags.name -eq "$tagNoMediaName")
			{
				$game.tagIds.Remove("$tagNoMediaIds")
				$PlayniteApi.Database.Games.Update($game)
				$CountNoMediaBefore++
			}
			
			
			if ($Tool -eq "Missing media")
			{
				# Skip current loop execution
				continue
			}
			
			# Check if Tool is set to 'Aspect ratio match' or 'Resolution match' to obtain the necessary image information
			if ( ($Tool -eq "Aspect ratio match") -or ($Tool -eq "Resolution match") )
			{
				# Get image height, width and aspect ratio
				try {
					add-type -AssemblyName System.Drawing
					$image = New-Object System.Drawing.Bitmap $imageFile
					[int]$imageHeight = $image.Height
					[int]$imageWidth = $image.Width
					[double]$ImageAspectRatio = $imageHeight / $imageWidth
					$image.Dispose()
				} catch {
					$CountIncompatibleFile++
					"$($game.name): Error processing image `"$imageFile`""  | Out-File -Encoding 'UTF8' -FilePath $ErrorPath -Append
					continue
				}
			}
			# Check if Tool is set to 'Size Bigger Than'
			if ($Tool -eq "Size Bigger Than")
			{
				# Get file size and compare with input
				[int]$imagesize = (Get-Item $imageFile).length/1KB
				if ($imagesize -gt $MoreTKB)
				{
					$MatchStatus = "AddTag"
				}
				else
				{
					$MatchStatus = "RemoveTag"
				}
			}
			# Check if Tool is set to 'Image Extension'
			elseif ($Tool -eq "Image Extension")
			{
				# Get Image extension
				$ImageExtension = [IO.Path]::GetExtension($imageFile) -replace '\.', ''
				if ($ImageExtension -eq $MatchExt)
				{
					$MatchStatus = "AddTag"
				}
				else
				{
					$MatchStatus = "RemoveTag"
				}
			}
			
			# Check if Tool is set to 'AspectRatioMatch'
			elseif ($Tool -eq "Aspect ratio match")
			{
				# Check if image aspect ratio matches set aspect ratio
				if ($ImageAspectRatio -eq $AspectRatio)
				{
					$MatchStatus = "RemoveTag"
				}
				else
				{
					$MatchStatus = "AddTag"
				}
			}
			# Check if Tool is set to 'ResolutionMatch'
			elseif ($Tool -eq "Resolution match")
			{
				# Check if image resolution matches set resolution
				if ( ($height -eq $imageHeight) -and ($width -eq $imageWidth) )
				{
					$MatchStatus = "RemoveTag"
				}
				else
				{
					$MatchStatus = "AddTag"
				}
			}
			
			# Check match status to add or remove tag
			if ($MatchStatus -eq "RemoveTag")
			{
				# Check if game has no match tag and remove if true
				if ($game.tags.name -eq "$tagMatchName")
				{
					$game.tagIds.Remove("$tagMatchIds")
					
					# Update game in database and increase removed tag count
					$PlayniteApi.Database.Games.Update($game)
					$CountTagRemoved++
				}
			}
			elseif ($MatchStatus -eq "AddTag")
			{
				# Check if game already has the tag
				if ($game.tags.name -ne "$tagMatchName")
				{
					# Add tag Id to game
					if ($game.tagIds) 
					{
						$game.tagIds += $tagMatchIds
					} 
					else 
					{
						# Fix in case game has null tagIds
						$game.tagIds = $tagMatchIds
					}
					
					# Update game in database and increase added no match tag count
					$PlayniteApi.Database.Games.Update($game)
					$CountTagAdded++
				}
			}
		}
		else
		{
			# Check if game already has No Media tag
			if ($game.tags.name -ne "$tagNoMediaName")
			{
				# Add tag Id to game
				if ($game.tagIds)
				{
					$game.tagIds += $tagNoMediaIds
				} 
				else 
				{
					# Fix in case game has null tagIds
					$game.tagIds = $tagNoMediaIds
				}
				
				# Update game in database and increase no media count
				$PlayniteApi.Database.Games.Update($game)
				$CountNoMedia++
			}
		}
	}
	
	# Generate error report
	$ErrorReport = "No errors found"
	if ( $CountIncompatibleFile -gt 0)
	{
		$ErrorReport = "$CountIncompatibleFile games couldn't be processed. See log at $ErrorPath for details"
	}
	
	# Show dialogue window with results
	if ($Mode -eq "Missing Media")
	{
		$PlayniteApi.Dialogs.ShowMessage(
			"`t`t`tMedia processing finished`n
			$($GameDatabase.Count) games were processed
			Selected tool: `"$Tool`"`n
			---------------------------------------------------------------------------
			Games with missing Media`n`t---------------------------------------------------------------------------
			`n$CountNoMedia games didn't have the selected type of media ($MediaType) and had the `"$tagNoMediaName`" tag added
			`n$CountNoMediaBefore games that didn't have $MediaType media before had the `"$tagNoMediaName`" tag removed"
		);
	}
	elseif ($Mode -eq "Exclusive")
	{
		$PlayniteApi.Dialogs.ShowMessage(
			"`t`t`tMedia processing finished`n
			$($GameDatabase.Count) games were processed
			Selected tool: `"$Tool`"
			Value to match: `"$ToMatchReport`"`n
			---------------------------------------------------------------------------
			Games with missing Media`n`t---------------------------------------------------------------------------
			`n$CountNoMedia games didn't have the selected type of media ($MediaType) and had the `"$tagNoMediaName`" tag added 
			`n$CountNoMediaBefore games that didn't have $MediaType media before had the `"$tagNoMediaName`" tag removed
			---------------------------------------------------------------------------
			Match with configured settings`n`t---------------------------------------------------------------------------
			`n$CountTagAdded games didn't match the set properties and had the `"$tagMatchName`" tag added
			`n$CountTagRemoved games that didn't have configured properties media before had the `"$tagMatchName`" tag removed
			`n$ErrorReport"
		);
	}
	elseif($Mode -eq "Inclusive")
	{
		$PlayniteApi.Dialogs.ShowMessage(
			"`t`t`tMedia processing finished`n
			$($GameDatabase.Count) games were processed
			Selected tool: `"$Tool`"
			Value to match: `"$ToMatchReport`"`n
			---------------------------------------------------------------------------
			Games with missing Media`n`t---------------------------------------------------------------------------
			`n$CountNoMedia games didn't have the selected type of media ($MediaType) and had the `"$tagNoMediaName`" tag added 
			`n$CountNoMediaBefore games that didn't have $MediaType media before had the `"$tagNoMediaName`" tag removed
			---------------------------------------------------------------------------
			Match with configured settings`n`t---------------------------------------------------------------------------
			`n$CountTagAdded games matched the set properties and had the `"$tagMatchName`" tag added
			`n$CountTagRemoved games that didn't have configured properties media before had the `"$tagMatchName`" tag removed
			`n$ErrorReport"
		);
	}
	
	# Get number of games using used match tag in game collection
	if ($tagMatchIds)
	{
		[string]$MatchGuid = $tagMatchIds
		[int]$GameDbMatchTag = $($PlayniteApi.Database.Games | Where-Object {$_.tags.id -eq $MatchGuid}).count
	}
	
	# Get number of games using used no media tag in game collection
	[string]$MediaGuid = $tagNoMediaIds
	[int]$GameDbMatchNoMediaTag = $($PlayniteApi.Database.Games | Where-Object {$_.tags.id -eq $MediaGuid}).count
	
	# Remove created/used tags from tag collection if there is not any game in the game database using them
	if ($GameDbMatchTag -eq 0)
	{
		$PlayniteApi.Database.Tags.Remove("$MatchGuid")
	}
	if ($GameDbMatchNoMediaTag -eq 0)
	{
		$PlayniteApi.Database.Tags.Remove("$MediaGuid")
	}
}

function OpenMetadataFolder()
{
	# Set GameDatabase
	$GameDatabase = $PlayniteApi.MainView.SelectedGames
	
	foreach ($game in $GameDatabase) {
		# Set metadata folder directory
		$Directory = Join-Path $PlayniteApi.Database.DatabasePath -ChildPath "Files" | Join-Path -ChildPath $($game.Id)
		
		# Verify if metadata folder exists and open
		if (Test-Path $Directory)
		{
			Invoke-Item $Directory
		}
	}
}