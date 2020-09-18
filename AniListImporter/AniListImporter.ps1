function global:Import-AniList() 
{
	param (
		[String]$ListType,
		[string]$AniListUsername,
		[string]$ReplaceCompletionStatus
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

	# Define media API query
	$MetadataQuery = '
	query ($id: Int) {
		Media (id: $id) {
			type
			id
			idMal
			siteUrl
			coverImage {
				medium
				large
			}
			description(asHtml: true)
			studios(sort: [NAME]) {
				nodes {
					id
					name
					isAnimationStudio
				}
			}
			staff {
				nodes {
					name {
						full
			  		}
				}
			}
			genres
			tags {
				name
				isGeneralSpoiler
				isMediaSpoiler
			  }
			averageScore
			bannerImage
			title {
				romaji
			}
		}
	}'

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
	
	# Download List
	$ListTypeUpper = $ListType.ToUpper()
	$ListTypeLower = $ListType.ToLower()
	$Variables = "{`"userName`": `"$AniListUsername`", `"type`": `"$ListTypeUpper`"}"  #<--- Define Variables
	$PostParams = @{query=$ListQuery;variables=$Variables} | ConvertTo-Json            #<--- Create query parameters
	try {
		$ListJson = (Invoke-WebRequest -Uri 'https://graphql.AniList.co' -Method POST -Body $PostParams -ContentType 'application/json' | ConvertFrom-Json).data.MediaListCollection.lists
		$__logger.Info("AniList Importer - Downloaded $ListType list of user `"$AniListUsername`"")
	} catch {
		$ErrorMessage = $_.Exception.Message
		$__logger.Info("AniList Importer - Error downloading user list. Error: $ErrorMessage")
		$PlayniteApi.Dialogs.ShowErrorMessage("Error downloading user list. Error: $ErrorMessage", "AniList Importer");
		exit
	}
	
	# Create List collection
	[System.Collections.Generic.List[Object]]$ListGlobal = @()
	foreach ($StatusList in $ListJson)	{
		foreach ($ListEntry in $StatusList.entries) {
			$ListGlobal.Add($ListEntry)
		}
	}
	$EntriesAdded = 0

	foreach ($ListEntry in $ListGlobal) {

		# Check if entry has been imported previously
		if ($AniListEntriesInDatabase -contains $ListEntry.mediaId)
		{
			if ($ReplaceCompletionStatus -ne "Yes")
			{
				continue
			}

			# Import Completion Status if it has changed
			$ExistingEntry = $PlayniteApi.Database.Games | Where-Object {$_.GameId -eq $ListEntry.mediaId}
			$CompletionStatus = $ExistingEntry.CompletionStatus
			switch ($ListEntry.status)
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
			# Download json of Entry
			$Variables = "{`"id`": $($ListEntry.mediaId)}"                                 #<--- Define Variables
			$PostParams = @{query=$MetadataQuery;variables=$Variables} | ConvertTo-Json    #<--- Create query parameters
			try {
				$MediaJson = (Invoke-WebRequest -Uri 'https://graphql.AniList.co' -Method POST -Body $PostParams -ContentType 'application/json' | ConvertFrom-Json).data.Media
				Start-Sleep -Milliseconds 1000
			} catch {
				$ErrorMessage = $_.Exception.Message
				$__logger.Info("AniList Importer - Error downloading media Json, execution stopped. Error: $ErrorMessage")
				$PlayniteApi.Dialogs.ShowErrorMessage("Error downloading media Json, execution will stop. Error: $ErrorMessage", "AniList Importer");
				break
			}
			
			# Initialize new game and check status
			$NewGame = New-Object "Playnite.SDK.Models.Game"
			switch ($ListEntry.status)
			{
				'PLANNING' {$NewGame.CompletionStatus = 'PlanToPlay'}
				'PAUSED' {$NewGame.CompletionStatus = 'OnHold'}
				'CURRENT' {$NewGame.CompletionStatus = 'Playing'}
				'DROPPED' {$NewGame.CompletionStatus = 'Abandoned'}
				'COMPLETED' {$NewGame.CompletionStatus = 'Completed'}
			}

			# Set game properties
			$NewGame.Name = $MediaJson.title.romaji
			$NewGame.GameId = $MediaJson.id
			$NewGame.Description = $MediaJson.description
			$NewGame.SourceId = $Source.Id			
			$NewGame.PlatformId = $Platform.Id
			$NewGame.CommunityScore = [int]$MediaJson.averageScore
			$NewGame.IsInstalled = $true

			# Set Cover and Background Image
			$NewGame.CoverImage = $MediaJson.coverimage.large
			if ($MediaJson.bannerImage)
			{
				$NewGame.BackgroundImage = $MediaJson.bannerImage
			}

			# Set Studio, Producer and Authors
			if ($MediaJson.Type -eq "MANGA")
			{
				foreach ($Author in $MediaJson.staff.nodes) {
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
			elseif ($MediaJson.Type -eq "ANIME")
			{
				foreach ($Company in $MediaJson.studios.nodes) {
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
			foreach ($GenreName in $MediaJson.Genres) {
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
			foreach ($TagName in $MediaJson.Tags) {
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
			$GameAction.Path = $MediaJson.siteUrl
			$NewGame.PlayAction = $GameAction

			# Create MyAnimeList PlayAction
			if ($MediaJson.idMal)
			{
				$GameAction = [Playnite.SDK.Models.GameAction]::New()
				$GameAction.Name = "Open in MyAnimeList"
				$GameAction.Type = "URL"
				$GameAction.Path = "https://myanimelist.net/{0}/{1}/" -f $ListTypeLower, $MediaJson.idMal
				$NewGame.OtherActions = $GameAction
			}
			
			# Add entry to database
			$PlayniteApi.Database.Games.Add($NewGame)
			$__logger.Info("AniList Importer - Added: `"$($NewGame.name)`", Type: `"$ListType`"")
			$EntriesAdded++

			# Add links MAL-Sync API links to entry
			Add-SiteLinks $NewGame 0 $true
		}
	}

	# Show results
	$PlayniteApi.Dialogs.ShowMessage("$ListType List import of user `"$AniListUsername`" finished`nImported $EntriesAdded new entries", "AniList Importer")
	$__logger.Info("AniList Importer - $ListType List import of user `"$AniListUsername`" finished`. Imported $EntriesAdded new entries.")
}

function Import-Anime
{
	# Request Username
	$UserNameInput = $PlayniteApi.Dialogs.SelectString("Enter AniList Username. Profile must be public.", "AniList Importer", "");
	if (!$UserNameInput.SelectedString)
	{
		exit
	}
	$AniListUsername = $UserNameInput.SelectedString

	# Ask if user wants to overwrite completion statuses
	$ReplaceCompletionStatus = $PlayniteApi.Dialogs.ShowMessage("Do you want to overwrite the completion status of already imported entries?", "AniList Importer", 4)
	if ($ReplaceCompletionStatus -ne "Yes")
	{
		$ReplaceCompletionStatus = "No"
	}
	
	# Invoke function
	Import-AniList 'Anime' $AniListUsername $ReplaceCompletionStatus
}

function Import-Manga
{
	# Request Username
	$UserNameInput = $PlayniteApi.Dialogs.SelectString("Enter AniList Username. Profile must be public.", "AniList Importer", "");
	if (!$UserNameInput.SelectedString)
	{
		exit
	}
	$AniListUsername = $UserNameInput.SelectedString

	# Ask if user wants to overwrite completion statuses
	$ReplaceCompletionStatus = $PlayniteApi.Dialogs.ShowMessage("Do you want to overwrite the completion status of already imported entries?", "AniList Importer", 4)
	if ($ReplaceCompletionStatus -ne "Yes")
	{
		$ReplaceCompletionStatus = "No"
	}
	
	# Invoke function
	Import-AniList 'Manga' $AniListUsername $ReplaceCompletionStatus
}

function Import-All
{
	# Request Username
	$UserNameInput = $PlayniteApi.Dialogs.SelectString("Enter AniList Username. Profile must be public.", "AniList Importer", "");
	if (!$UserNameInput.SelectedString)
	{
		exit
	}
	$AniListUsername = $UserNameInput.SelectedString

	# Ask if user wants to overwrite completion statuses
	$ReplaceCompletionStatus = $PlayniteApi.Dialogs.ShowMessage("Do you want to overwrite the completion status of already imported entries?", "AniList Importer", 4)
	if ($ReplaceCompletionStatus -ne "Yes")
	{
		$ReplaceCompletionStatus = "No"
	}
	
	# Invoke function
	Import-AniList 'Anime' $AniListUsername $ReplaceCompletionStatus
	Import-AniList 'Manga' $AniListUsername $ReplaceCompletionStatus
}

function Add-SiteLinks()
{
	param (
		$GameDatabase,
		$SleepTime,
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
			$MalSyncUri = $MalSyncApi -f $ListType, $MalId
			$MalSyncInfo = Invoke-WebRequest $MalSyncUri  | ConvertFrom-Json
			Start-Sleep -Milliseconds $SleepTime
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
			break
		}

		# Add links to entry
		$CountLinksAddedEntry = 0
		$Entry.Links = $null
		foreach ($Site in $MalSyncInfo.Sites.PSObject.Properties) {
			foreach ($Version in $Site.Value.PSObject.Properties.Value) {
				[string]$url = $Version.url
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

	Add-SiteLinks $GameDatabase 1000 $false

	# Show results
	$PlayniteApi.Dialogs.ShowMessage("Added site links to $CountLinkAddedGlobal entries", "AniList Importer");
}

function Add-SiteLinksMissing()
{
	# Set gamedatabase
	$GameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.source.name -eq "AniList"} | Where-Object {$_.Links.count -eq 0}

	Add-SiteLinks $GameDatabase 1000 $false

	# Show results
	$PlayniteApi.Dialogs.ShowMessage("Added site links to $CountLinkAddedGlobal entries", "AniList Importer");
}

function Add-SiteLinksSelected()
{
	# Set gamedatabase
	$GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.source.name -eq "AniList"}

	Add-SiteLinks $GameDatabase 1000 $false

	# Show results
	$PlayniteApi.Dialogs.ShowMessage("Added site links to $CountLinkAddedGlobal entries", "AniList Importer");
}
