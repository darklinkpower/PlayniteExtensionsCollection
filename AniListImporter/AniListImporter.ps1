function global:Import-AniList() 
{
	param (
		[String]$ListType,
		[string]$ReplaceCompletionStatus,
		[string]$AddLinks
	)
	
	# Set Source
	$SourceName = "AniList"
	$Source = $PlayniteApi.Database.Sources.Add($SourceName)

	# Set Platform
	$Platform = $PlayniteApi.Database.Platforms.Add($ListType)

	# Create cache of AniList Entries in Database
	[System.Collections.Generic.List[string]]$AniListEntriesInDatabase = @()
	$AniListAsSource = $PlayniteApi.Database.Games | Where-Object {$_.source.name -eq $SourceName}
	foreach ($Game in $AniListAsSource) {
		$AniListEntriesInDatabase.Add($Game.GameId)
	}
	
	# Get AniList username
	$UsernameConfigPath = Join-Path -Path $PlayniteApi.Paths.ExtensionsDataPath -ChildPath 'AniList Importer\Username.txt'
	if (Test-Path $UsernameConfigPath)
	{
		$AniListUsername = [System.IO.File]::ReadAllLines($UsernameConfigPath)
	}
	else
	{
		$PlayniteApi.Dialogs.ShowMessage("Username has not been configured. A window to configure it will be opened.", "AniList Importer");
		Set-Username
		$AniListUsername = [System.IO.File]::ReadAllLines($UsernameConfigPath)
	}
	
	# Define media API query
	$MetadataQuery = '
	Entry{0}: Media (id: {1}) {{
		type
		id
		idMal
		siteUrl
		coverImage {{
			large
		}}
		description(asHtml: true)
		studios(sort: [NAME]) {{
			nodes {{
				name
				isAnimationStudio
			}}
		}}
		staff {{
			nodes {{
				name {{
					full
				}}
			}}
		}}
		genres
		tags {{
			name
			isGeneralSpoiler
			isMediaSpoiler
		}}
		averageScore
		bannerImage
		title {{
			romaji
		}}
	}}'
	
	# Define Api List Query
	$ListQuery = '
	query ($userName: String, $type: MediaType) {
		MediaListCollection (userName: $userName, type: $type) {
			  lists {
				status
				  entries {
			  id
			  mediaId
			  status
				  }
			  }
		}
	}'
	
	# Download user list from AniList
	$ListTypeUpper = $ListType.ToUpper()
	$ListTypeLower = $ListType.ToLower()
	$Variables = "{`"userName`": `"$AniListUsername`", `"type`": `"$ListTypeUpper`"}"  #<--- Define Variables
	$PostParams = @{query=$ListQuery;variables=$Variables} | ConvertTo-Json            #<--- Create query parameters
	try {
		$UserStatusLists = (Invoke-WebRequest -Uri 'https://graphql.AniList.co' -Method POST -Body $PostParams -ContentType 'application/json' | ConvertFrom-Json).data.MediaListCollection.lists
		$__logger.Info("AniList Importer - Downloaded $ListType list of user `"$AniListUsername`"")
	} catch {
		$ErrorMessage = $_.Exception.Message
		$__logger.Info("AniList Importer - Error downloading user list. Error: $ErrorMessage")
		$PlayniteApi.Dialogs.ShowErrorMessage("Error downloading user list. Error: $ErrorMessage", "AniList Importer");
		exit
	}
	
	# Create collection of entries and MediaIds found in account
	[System.Collections.Generic.List[Object]]$EntriesInAccount = @()
	[System.Collections.Generic.List[object]]$MissingEntries = @()
	foreach ($StatusList in $UserStatusLists)	{
		foreach ($Entry in $StatusList.entries) {
			$EntriesInAccount.Add($Entry)
		}
	}
	$__logger.Info("AniList Importer - Found $($EntriesInAccount.Count) entries of type `"$ListType`" in account")
	
	foreach ($Entry in $EntriesInAccount) {
		# Check if entry has been imported previously
		if ($AniListEntriesInDatabase -contains $Entry.mediaId)
		{
			if ($ReplaceCompletionStatus -ne "Yes")
			{
				continue
			}

			# Import Completion Status if it has changed
			$ExistingEntry = $PlayniteApi.Database.Games | Where-Object {$_.GameId -eq $Entry.mediaId}
			$CompletionStatus = $ExistingEntry.CompletionStatus
			switch ($Entry.status)
			{
				'PLANNING' {
					if ($ExistingEntry.CompletionStatus -ne 'PlanToPlay') 
					{
						$ExistingEntry.CompletionStatus = 'PlanToPlay'
						$PlayniteApi.Database.Games.Update($ExistingEntry)
						$__logger.Info("AniList Importer - Entry: `"$($ExistingEntry.name)`", Changed Completion Status from `"$CompletionStatus`" to `"PlanToPlay`"")
					}
				}
				'PAUSED' {
					if ($ExistingEntry.CompletionStatus -ne 'OnHold') 
					{
						$ExistingEntry.CompletionStatus = 'OnHold'
						$PlayniteApi.Database.Games.Update($ExistingEntry)
						$__logger.Info("AniList Importer - Entry: `"$($ExistingEntry.name)`", Changed Completion Status from `"$CompletionStatus`" to `"OnHold`"")
					}
				}
				'CURRENT' {
					if ($ExistingEntry.CompletionStatus -ne 'Playing') 
					{
						$ExistingEntry.CompletionStatus = 'Playing'
						$PlayniteApi.Database.Games.Update($ExistingEntry)
						$__logger.Info("AniList Importer - Entry: `"$($ExistingEntry.name)`", Changed Completion Status from `"$CompletionStatus`" to `"Playing`"")
					}
				}
				'DROPPED' {
					if ($ExistingEntry.CompletionStatus -ne 'Abandoned') 
					{
						$ExistingEntry.CompletionStatus = 'Abandoned'
						$PlayniteApi.Database.Games.Update($ExistingEntry)
						$__logger.Info("AniList Importer - Entry: `"$($ExistingEntry.name)`", Changed Completion Status from `"$CompletionStatus`" to `"Abandoned`"")
					}
				}
				'COMPLETED' {
					if ($ExistingEntry.CompletionStatus -ne 'Completed') 
					{
						$ExistingEntry.CompletionStatus = 'Completed'
						$PlayniteApi.Database.Games.Update($ExistingEntry)
						$__logger.Info("AniList Importer - Entry: `"$($ExistingEntry.name)`", Changed Completion Status from `"$CompletionStatus`" to `"Completed`"")
					}
				}
			}
		}
		else
		{
			# Save to list of missing entries
			$MissingEntries.Add($Entry)
		}
	}
	$__logger.Info("AniList Importer - Found $($MissingEntries.Count) missing entries of type `"$ListType`" in account")

	# Download Metadata of missing entries
	[System.Collections.Generic.List[String]]$MissingEntriesQuery = @()
	$ItemCount = $MissingEntries.count - 1
	$MetadaDownloadCount = 0
	foreach ($Entry in $MissingEntries) {
		$IterationCount++
		[string]$MediaIdIndex = $MissingEntries.IndexOf($Entry)
		$MediaIdQuery = $MetadataQuery -f $MediaIdIndex, $Entry.mediaId
		$MissingEntriesQuery.Add($MediaIdQuery)
		if (($IterationCount -eq 20) -or ($MediaIdIndex -eq $ItemCount))
		{
			# Download media information from AniList API
			$MetadataRequestQuery = "{$MissingEntriesQuery}"                 #<--- Define Variables
			$PostParams = @{query=$MetadataRequestQuery} | ConvertTo-Json    #<--- Create query parameters
			try {
				$MetadaDownloadCount++
				Start-Sleep -Milliseconds 1000
				$MetadataJson = (Invoke-WebRequest -Uri 'https://graphql.AniList.co' -Method POST -Body $PostParams -ContentType 'application/json' | ConvertFrom-Json).data
				$__logger.Info("AniList Importer - Downloaded Metadata json $MetadaDownloadCount")
			} catch {
				$ErrorMessage = $_.Exception.Message
				$__logger.Info("AniList Importer - Error downloading metadata Json file, execution stopped. Error: $ErrorMessage")
				$PlayniteApi.Dialogs.ShowErrorMessage("Error downloading metadata Json file, execution will stop. Error: $ErrorMessage", "AniList Importer");
				exit
			}
			
 			if ($null -eq $Metadata)
			{
				$Metadata = $MetadataJson
			}
			else
			{
				$Metadata = @($Metadata; $MetadataJson)
			}
			$IterationCount = 0
			[System.Collections.Generic.List[String]]$MissingEntriesQuery = @()
		}
	}

	# Add missing entries
	$EntriesAdded = 0
	foreach ($MissingEntry in $MissingEntries) {
		# Get entry metadata
		[string]$EntryIndex = $MissingEntries.IndexOf($MissingEntry)
		$EntryNumber = "Entry" + $EntryIndex
		$EntryMetadata = $Metadata.$EntryNumber

		# Initialize new game and check status
		$NewGame = New-Object "Playnite.SDK.Models.Game"
		switch ($MissingEntry.status)
		{
			'PLANNING' {$NewGame.CompletionStatus = 'PlanToPlay'}
			'PAUSED' {$NewGame.CompletionStatus = 'OnHold'}
			'CURRENT' {$NewGame.CompletionStatus = 'Playing'}
			'DROPPED' {$NewGame.CompletionStatus = 'Abandoned'}
			'COMPLETED' {$NewGame.CompletionStatus = 'Completed'}
		}

		# Set game properties
		$NewGame.Name = $EntryMetadata.title.romaji
		$NewGame.GameId = $MissingEntry.mediaId
		$NewGame.Description = $EntryMetadata.description
		$NewGame.SourceId = $Source.Id			
		$NewGame.PlatformId = $Platform.Id
		$NewGame.CommunityScore = $EntryMetadata.averageScore
		$NewGame.IsInstalled = $true

		# Set Cover and Background Image
		$NewGame.CoverImage = $EntryMetadata.coverimage.large
		if ($EntryMetadata.bannerImage)
		{
			$NewGame.BackgroundImage = $EntryMetadata.bannerImage
		}

		# Set Studio, Producer and Authors
		if ($EntryMetadata.Type -eq "MANGA")
		{
			foreach ($Author in $EntryMetadata.staff.nodes) {
				# Create author in database
				$AuthorName = $Author.name.full
				$Author = $PlayniteApi.Database.Companies.Add($AuthorName)

				# Add author to entry
				if ($NewGame.DeveloperIds)
				{
					$NewGame.DeveloperIds.Add($Author.Id)
				}
				else
				{
					# Fix in case game property is null
					$NewGame.DeveloperIds = $Author.Id
				}
			}
		}
		elseif ($EntryMetadata.Type -eq "ANIME")
		{
			foreach ($Company in $EntryMetadata.studios.nodes) {
				# Creat company in database
				$CompanyName = $Company.name
				$Company = $PlayniteApi.Database.Companies.Add($CompanyName)
				if ($Company.isAnimationStudio -eq $false)
				{
					# Add publisher to entry
					if ($NewGame.PublisherIds) 
					{
						$NewGame.PublisherIds.Add($Company.Id)
					}
					else
					{
						# Fix in case game property is null
						$NewGame.PublisherIds = $Company.Id
					}
				}
				else
				{
					# Add developer to entry
					if ($NewGame.DeveloperIds) 
					{
						$NewGame.DeveloperIds.Add($Company.Id)
					}
					else
					{
						# Fix in case game property is null
						$NewGame.DeveloperIds = $Company.Id
					}
				}
			}
		}

		# Set Genres
		foreach ($GenreName in $EntryMetadata.Genres) {
			# Create genre in database
			$Genre = $PlayniteApi.Database.Genres.Add($GenreName)

			# Add genre to entry
			if ($NewGame.GenreIds)
			{
				$NewGame.GenreIds.Add($Genre.Id)
			}
			else
			{
				# Fix in case game property is null
				$NewGame.GenreIds = $Genre.Id
			}
		}

		# Set tags
		foreach ($TagName in $EntryMetadata.Tags) {
			# Skip tag if it's a spoiler
			if (($TagName.isGeneralSpoiler -eq $true) -or ($TagName.isMediaSpoiler -eq $true))
			{
				continue
			}

			# Create tag in database
			$Tag = $PlayniteApi.Database.Tags.Add($TagName.name)

			# Add tag to entry
			if ($NewGame.TagIds)
			{
				$NewGame.TagIds.Add($Tag.Id)
			}
			else
			{
				# Fix in case game property is null
				$NewGame.TagIds = $Tag.Id
			}
		}

		# Create AniList PlayAction
		$GameAction = [Playnite.SDK.Models.GameAction]::New()
		$GameAction.Type = "URL"
		$GameAction.Path = $EntryMetadata.siteUrl
		$NewGame.PlayAction = $GameAction

		# Create MyAnimeList PlayAction
		if ($EntryMetadata.idMal)
		{
			$GameAction = [Playnite.SDK.Models.GameAction]::New()
			$GameAction.Name = "Open in MyAnimeList"
			$GameAction.Type = "URL"
			$GameAction.Path = "https://myanimelist.net/{0}/{1}/" -f $ListTypeLower, $EntryMetadata.idMal
			$NewGame.OtherActions = $GameAction
		}
		
		# Add entry to database
		$PlayniteApi.Database.Games.Add($NewGame)
		$__logger.Info("AniList Importer - Added: `"$($NewGame.name)`", Type: `"$ListType`"")
		$EntriesAdded++

		# Add links MAL-Sync API links to entry
		if ($AddLinks -eq "Yes")
		{
			Add-SiteLinks $NewGame $true
		}
	}

	# Show results
	$PlayniteApi.Dialogs.ShowMessage("$ListType List import of user `"$AniListUsername`" finished`nImported $EntriesAdded new entries", "AniList Importer")
	$__logger.Info("AniList Importer - $ListType List import of user `"$AniListUsername`" finished`. Imported $EntriesAdded new entries.")
}

function Import-Anime
{
	# Ask if user wants to overwrite completion statuses
	$ReplaceCompletionStatus = $PlayniteApi.Dialogs.ShowMessage("Do you want to overwrite the completion status of already imported entries?", "AniList Importer", 4)
	if ($ReplaceCompletionStatus -ne "Yes")
	{
		$ReplaceCompletionStatus = "No"
	}

	# Ask if user wants to add MAL-Sync Links
	$AddLinks = $PlayniteApi.Dialogs.ShowMessage("Do you want to add streaming links during import process?`n`nBe aware that adding the links will make the import process much longer`nLinks can be added afterwards too with the extension functions", "AniList Importer", 4)
	if ($AddLinks -ne "Yes")
	{
		$AddLinks = "No"
	}

	# Invoke function
	Import-AniList 'Anime' $ReplaceCompletionStatus $AddLinks
}

function Import-Manga
{
	# Ask if user wants to overwrite completion statuses
	$ReplaceCompletionStatus = $PlayniteApi.Dialogs.ShowMessage("Do you want to overwrite the completion status of already imported entries?", "AniList Importer", 4)
	if ($ReplaceCompletionStatus -ne "Yes")
	{
		$ReplaceCompletionStatus = "No"
	}
	
	# Ask if user wants to add MAL-Sync Links
	$AddLinks = $PlayniteApi.Dialogs.ShowMessage("Do you want to add reading links during import process?`n`nBe aware that adding the links will make the import process much longer`nLinks can be added afterwards too with the extension functions", "AniList Importer", 4)
	if ($AddLinks -ne "Yes")
	{
		$AddLinks = "No"
	}

	# Invoke function
	Import-AniList 'Manga' $ReplaceCompletionStatus $AddLinks
}

function Import-All
{
	# Ask if user wants to overwrite completion statuses
	$ReplaceCompletionStatus = $PlayniteApi.Dialogs.ShowMessage("Do you want to overwrite the completion status of already imported entries?", "AniList Importer", 4)
	if ($ReplaceCompletionStatus -ne "Yes")
	{
		$ReplaceCompletionStatus = "No"
	}
	
	# Ask if user wants to add MAL-Sync Links
	$AddLinks = $PlayniteApi.Dialogs.ShowMessage("Do you want to add streaming and reading links during import process?`n`nBe aware that adding the links will make the import process much longer`nLinks can be added afterwards too with the extension functions", "AniList Importer", 4)
	if ($AddLinks -ne "Yes")
	{
		$AddLinks = "No"
	}

	# Invoke function
	Import-AniList 'Anime' $ReplaceCompletionStatus $AddLinks
	Import-AniList 'Manga' $ReplaceCompletionStatus $AddLinks
}

function Add-SiteLinks()
{
	param (
		$GameDatabase,
		$IgnoreErrors
	)
	
	# MAL-Sync API
	$MalSyncApi = 'https://api.malsync.moe/mal/{0}/{1}'

	# Counters
	$global:CountLinkAddedGlobal = 0

	foreach ($Entry in $GameDatabase) {
		# Check for MalLink
		$MalId = $null
		$ListType = ($Entry.Platform.name).ToLower()
		foreach ($PlayAction in $Entry.OtherActions) {
			switch -regex ($PlayAction.Path) 
			{
				"https://myanimelist.net/$ListType/(\d+)/" {
					$MalId = $matches[1]
				}
			}
			if ($MalId)
			{
				break
			}
		}
		if (!$MalId)
		{
			continue
		}

		# Download MAL-Sync entry information
		try {
			Start-Sleep -Milliseconds 1000
			$MalSyncUri = $MalSyncApi -f $ListType, $MalId
			$MalSyncInfo = Invoke-WebRequest $MalSyncUri  | ConvertFrom-Json
		}
		catch {
			if ($IgnoreErrors -eq $true)
			{
				continue
			}
			$ErrorMessage = $_.Exception.Message
			$__logger.Info("AniList Importer - Title name: `"$($Entry.name)`", Type: `"$ListType`", MAL Id: `"$MalId`". Error downloading entry information from MAL-Sync API. Error: `"$ErrorMessage`"")
			if (!$Continue)
			{
				$PlayniteApi.Dialogs.ShowErrorMessage("Title name: `"$($Entry.name)`", Type: `"$ListType`", MAL Id: `"$MalId`"`nError downloading entry information from MAL-Sync API. Error: `"$ErrorMessage`"", "AniList Importer");
				$Continue = $PlayniteApi.Dialogs.ShowMessage("Continue extension execution ignoring all errors?`nIf the error was `"(400)`" it's because the entry has no information in the MAL-Sync API and can be safely ignored", "AniList Importer", 4);
			}
			if ($Continue -eq "Yes")
			{
				continue
			}
			else
			{
				break			
			}
		}

		# Add links to entry
		$Entry.Links = $null
		foreach ($Site in $MalSyncInfo.Sites.PSObject.Properties) {
			foreach ($Version in $Site.Value.PSObject.Properties.Value) {
				if ($Entry.links.url -notcontains $Version.url)
				{
					$LinkName = "$($Site.Name) - $($Version.title)"
					$Link = [Playnite.SDK.Models.Link]::New($LinkName, $Version.url)
					if ($Entry.Links)
					{
						$Entry.Links.Add($Link)
					}
					else
					{
						# Fix in case game has null property
						$Entry.Links = $Link
					}
				}
			}
		}

		# Update entry in database
		$PlayniteApi.Database.Games.Update($Entry)
		$global:CountLinkAddedGlobal++
		$__logger.Info("AniList Importer - Added $CountLinkAddedEntry links to `"$($Entry.name)`", Type: `"$ListType`", MAL Id: `"$MalId`"")
	}
}

function Add-SiteLinksAll()
{
	# Set gamedatabase
	$GameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.source.name -eq "AniList"}
	
	# Ask for confirmation
	$Confirm = $PlayniteApi.Dialogs.ShowMessage("Warning: This function can take a long time to complete depending on the number of entries in the account.`n`nAre you sure you want to reset the links in all entries?", "AniList Importer", 4)
	if ($Confirm -ne "Yes")
	{
		exit
	}

	Add-SiteLinks $GameDatabase $false

	# Show results
	$PlayniteApi.Dialogs.ShowMessage("Added site links to $CountLinkAddedGlobal entries", "AniList Importer");
}

function Add-SiteLinksMissing()
{
	# Set gamedatabase
	$GameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.source.name -eq "AniList"} | Where-Object {$_.Links.count -eq 0}

	Add-SiteLinks $GameDatabase $false

	# Show results
	$PlayniteApi.Dialogs.ShowMessage("Added site links to $CountLinkAddedGlobal entries", "AniList Importer");
}

function Add-SiteLinksSelected()
{
	# Set gamedatabase
	$GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.source.name -eq "AniList"}

	Add-SiteLinks $GameDatabase $false

	# Show results
	$PlayniteApi.Dialogs.ShowMessage("Added site links to $CountLinkAddedGlobal entries", "AniList Importer");
}

function Set-Username()
{
	# Set paths
	$ExtensionPath = Join-Path -Path $PlayniteApi.Paths.ExtensionsDataPath -ChildPath 'AniList Importer'
	$UsernameConfigPath = Join-Path -Path $ExtensionPath -ChildPath 'Username.txt'
	if (!(Test-Path $ExtensionPath))
	{
		New-Item -ItemType Directory -Path $ExtensionPath -Force
	}
	
	# Request Username
	$UserNameInput = $PlayniteApi.Dialogs.SelectString("Enter AniList Username. Profile must be public:", "AniList Importer", "");
	if ($UserNameInput.Result -eq $false)
	{
		exit
	}
	$AniListUsername = $UserNameInput.SelectedString
	$AniListUsername | Out-File -Encoding 'UTF8' -FilePath $UsernameConfigPath
	$PlayniteApi.Dialogs.ShowMessage("Username `"$AniListUsername`" has been configured.", "AniList Importer");
}