function global:GetMainMenuItems()
{
	param($menuArgs)

	$menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
	$menuItem1.Description = "Import dates"
	$menuItem1.FunctionName = "Invoke-SteamDateImporter"
	$menuItem1.MenuSection = "@Steam Date Importer"
	
	return $menuItem1
}

function Invoke-SteamDateImporter
{
	# Create prefix strings to remove
	[System.Collections.Generic.List[string]]$RemoveStrings = @(
		
		# Regions
		" \(Latam\)",
		" \(LATAM\/IN\)",
		" \(LATAM\/RU\/CN\/IN\/TR\)",
		" Latam",
		" \(US\)",
		" \(US\/AU\)",
		" \(NA\)",
		" \(NA\+ROW\)",
		" \(ROW Launch\)",
		" \(ROW\)",
		" \(Rest of World\)",
		" ROW Release",
		" ROW",
		" \(RU\)",
		" RU",
		" \(South America\)",
		" SA",
		" \(WW\)",
		" WW",
		" WW Digital Distribution",
		" \(Key-only WW\)",

		# Release type
		" Collection Retail",
		" \(Retail\)",
		" Retail Key",
		" Retail Rtd",
		" - The Full Package Retail",
		" \(Digital Retail\)",
		" \[DIGITAL RETAIL\]",
		" - [Digital]",
		" [Digital]"
		" Retail",
		" \(preorder\)",
		" \(Pre-Order\)",
		" \(pre-purchase\)",
		" Pre-Purchase",
		" \(Post-Launch\)",
		" Post-Launch",
		" CD key",
		" Retail",
		" Digital"

		# Free
		" - Free Giveaway",
		" - Free",
		" - Free for 24 Hours",
		" - Free For A Limited Time!",
		" \(Free\)",
		" Free",
		" Free Giveaway"

		# Editions
		" Deluxe Edition",
		" Complete Edition",
		" Standard Edition",
		" Voiced Edition",
		"  Digital Edition",
		" Digital Distribution"
		" - Digital Edition of Light",
		" Day One Edition",
		" Enhanced Edition"
		"  Complete Edition",
		" - Legacy Edition",
		" - Starter Pack",
		" - Starter Edition",
		" Special Edition",
		" Standard",
		" Launch",
		" Paper's Cut Edition",
		" - War Chest Edition",
		" - Special Steam Edition",
		": Assassins of Kings Enhanced Edition",
		" - Beta",
		" for Beta Testing"

		# Other
		" \(Rebellion Store\)",
		" PROMO",
		" - Gift",
		" Gift",
		" Steam Store and Retail Key",
		" \(100% off week\)",
		" - Complimentary \(Opt In\)",
		" Gift Copy - Hades Purchase",
		" - Holiday Pack",
		" Bundle (Summer 2012)",
		": REVENGEANCE",
		" Care Package",
		" Complete Season (Episodes 1-5)",
		" Deluxe - Includes OST and an exclusive Artbook",
		" Free On Demand",
		" 4-Pack",
		" 2 Pack",
		" Free Edition - Français + Italiano + 한국어 [Learn French + Italian + Korean]",
		": 20 Year Celebration",
		": Complete Story",
		" Free Access"
	)

	# Create prefix strings to remove that have dates
	[System.Collections.Generic.List[string]]$Months = @(
		"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Aug", "Sep", "Oct", "Nov", "Dec"
	)
	[System.Collections.Generic.List[string]]$Years = @(
		"2016", "2017", "2018", "2019", "2020", "2021"
	)
	foreach ($year in $Years) {
		foreach ($month in $months) {
			$LimitedFree = " Limited Free Promotional Package - " + "$month " + "$year"
			$RemoveStrings.Add($LimitedFree)
		}
	}

	# Use Webview to get licenses page content (Offscreen)
	$LicensesUrl = 'https://store.steampowered.com/account/licenses/?l=english'
	$webView = $PlayniteApi.WebViews.CreateOffscreenView()
	$webView.NavigateAndWait($LicensesUrl)
	$LicensesHtmlContent = $webView.GetPageSource()
	$webView.Close()

	# Use regex to get all licenses
	$regex = '(?:<td\sclass="license_date_col">)(\d\d?\s\w{3},\s\d{4})(?:<\/td>\s+<td>\s+)(?:<div\sclass="free_license_remove_link">\s+<a\shref="javascript:RemoveFreeLicense\(\s\d+,\s''.+\s+<\/div>\s+)?((?:\S|\s(?!\s))*)(?:\s+<\/td>)'
	$LicenseMatches = ([regex]$regex).Matches($LicensesHtmlContent)
	if ($LicenseMatches.count -eq 0)
	{
		# Use Webview to log in
		$webView = $PlayniteApi.WebViews.CreateView(1020, 600)
		$webView.Navigate($LicensesUrl)
		$webView.OpenDialog()
		$webView.Close()

		# Use Webview to get licenses page content (Offscreen)
		$webView = $PlayniteApi.WebViews.CreateOffscreenView()
		$webView.NavigateAndWait($LicensesUrl)
		$LicensesHtmlContent = $webView.GetPageSource()
		$webView.Close()
		$LicenseMatches = ([regex]$regex).Matches($LicensesHtmlContent)
		if ($LicenseMatches.count -eq 0)
		{
			$PlayniteApi.Dialogs.ShowMessage("Not logged in or no licenses found", "Steam Date Importer")
			exit
		}
	}
	
	# Create collections of found licenses
	[System.Collections.Generic.List[object]]$LicensesList = @()
	foreach ($LicenseMatch in $LicenseMatches) {
		
		#Create License name for matching 
		$LicenseNameMatch = $LicenseMatch.Groups[2].value
		foreach ($StringPattern in $RemoveStrings) {
			$LicenseNameMatch = $LicenseNameMatch -replace "($StringPattern)$", ''
		}
		$LicenseNameMatch = $LicenseNameMatch -replace "reg;", '' -replace "amp;", '' -replace "trade;", '' -replace "rsquo;", '' -replace "ndash;", ''
		$LicenseNameMatch = $LicenseNameMatch -replace "ü", 'u' -replace ' and ', '' -replace "Game of the Year", 'GOTY' -replace "(GOTY)$", 'GOTY Edition'
		
		# Create license object and add to collection
		$License = [PSCustomObject]@{
			LicenseName = $LicenseMatch.Groups[2].value
			LicenseNameMatch = $LicenseNameMatch -replace '[^\p{L}\p{Nd}]', ''
			LicenseDate = [datetime]$LicenseMatch.Groups[1].value
		}
		$LicensesList.Add($License)
	}

	# Licenses Export
	$LicenseExportChoice = $PlayniteApi.Dialogs.ShowMessage("Found $($LicenseMatches.count) licenses`nDo you want to export your found Steam licenses?", "Steam Date Importer", 4)
	if ($LicenseExportChoice -eq "Yes")
	{
		$LicenseExportPath = $PlayniteApi.Dialogs.SaveFile("CSV|*.csv|Formated TXT|*.txt")
		if ($LicenseExportPath)
		{
			if ($LicenseExportPath -match ".csv$")
			{
				$LicensesList | Select-Object LicenseName, LicenseDate | ConvertTo-Csv -NoTypeInformation | Out-File $LicenseExportPath -Encoding 'UTF8'
			}
			else
			{
				$LicensesList | Select-Object LicenseName, LicenseDate | Format-Table -AutoSize | Out-File $LicenseExportPath -Encoding 'UTF8'
			}
			$PlayniteApi.Dialogs.ShowMessage("Licenses exported succesfully.", "Steam Date Importer");
		}
	}

	# Set GameDatabase and create modified games collection
	[System.Collections.Generic.List[object]]$GameDatesList = @()
	$GameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.source.name -eq "Steam"}
	$CountNewDate = 0
	$CountMatchLicense = 0
	$CountNoLicense = 0

	foreach ($game in $GameDatabase) {
		
		# Generate matching game name and check for match in licenses collection
		$GameNameMatch = $($game.name) -replace "ü", 'u' -replace ' and ', '' -replace "Game of the Year", 'GOTY' -replace "(GOTY)$", 'GOTY Edition'
		$GameNameMatch = $GameNameMatch -replace '[^\p{L}\p{Nd}]', ''
		$GameDateOld = $game.Added
		$GameLicense = $null
		foreach ($License in $LicensesList) {
			if ($License.LicenseNameMatch -eq $GameNameMatch) 
			{
				[object]$GameLicense = $License
				break
			}
		}
		if ($GameLicense)
		{
			$CountMatchLicense++
			$LicenseFound = "True"
			$LicenseDate = [datetime]$GameLicense.LicenseDate
			if ($game.Added -ne $LicenseDate)
			{
				$game.Added = [datetime]$GameLicense.LicenseDate
				$PlayniteApi.Database.Games.Update($game)
				$GameDateNew = $game.Added
				$DateChanged = "True"
				$CountNewDate++
			}
			else 
			{
				$DateChanged = "False"
				$GameDateNew = $null
			}
		}
		else
		{
			$CountNoLicense++
			$GameDateNew = $null
			$DateChanged = "False"
			$LicenseFound = "False"
			$LicenseDate = $null
		}
		
		$GameDates = [PSCustomObject]@{
			Name = $game.name
			OldDate = $GameDateOld
			NewDate = $GameDateNew
			DateChanged = $DateChanged
			LicenseFound = $LicenseFound
			LicenseDate = $LicenseDate	 
		}
		$GameDatesList.Add($GameDates)
	}

	# Show finish dialogue with results and ask if user wants to export results
	$ExportChoice = $PlayniteApi.Dialogs.ShowMessage("Processed games: $($GameDatabase.count)`n`nGames that matched a license: $CountMatchLicense`nGames that didn't match a license: $CountNoLicense`nGames that had the added date changed: $CountNewDate`n`nDo you want to export results?", "Steam Date Importer", 4)
	if ($ExportChoice -eq "Yes")
	{
		$ExportPath = $PlayniteApi.Dialogs.SaveFile("CSV|*.csv|Formated TXT|*.txt")
		if ($ExportPath)
		{
			if ($ExportPath -match "\.csv$")
			{
				$GameDatesList | Select-Object Name, OldDate, NewDate, DateChanged, LicenseFound, LicenseDate | Sort-Object -Property LicenseFound, Name | ConvertTo-Csv -NoTypeInformation | Out-File $ExportPath -Encoding 'UTF8'
			}
			else
			{
				$GameDatesList | Select-Object Name, OldDate, NewDate, DateChanged, LicenseFound, LicenseDate | Sort-Object -Property LicenseFound, Name | Format-Table -AutoSize | Out-File $ExportPath -Encoding 'UTF8'
			}
			$PlayniteApi.Dialogs.ShowMessage("Results exported successfully.", "Steam Date Importer");
		}
	}
}