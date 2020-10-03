function global:GetMainMenuItems()
{
    param($menuArgs)

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "All games in database"
	$menuItem1.FunctionName = "DescriptionRemoveAboutGameAll"
	$menuItem1.MenuSection = "@Description remove before `"About Game`""
	
    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = "Only selected games"
	$menuItem2.FunctionName = "DescriptionRemoveAboutGameSelected"
	$menuItem2.MenuSection = "@Description remove before `"About Game`""
	
	return $menuItem1, $menuItem2
}

function DescriptionRemoveAboutGameAll()
{
	#Set GameDatabase
	$GameDatabase = $PlayniteApi.Database.Games | Where-Object { ($_.description) -and ($_.platform.name -eq "PC") }

	# Set counters
	$ChangedCount = 0

	# Regex
	$regex = '(?:[\s\S]+)<h1>About the Game<\/h1>([\s\S]+)'

	foreach ($game in $GameDatabase) {
		$RegexMatch = ([regex]$regex).Matches($($game.description))
		if ($RegexMatch.count -eq 1)
		{
			$game.description = '<h1>About the Game</h1>' + $RegexMatch.groups[1].value
			$PlayniteApi.Database.Games.Update($game)
			$ChangedCount++
		}
	}
	# Show finish dialogue with results
	$PlayniteApi.Dialogs.ShowMessage("Changed $ChangedCount games description ", "Description Remove `"About Game`"");
}

function DescriptionRemoveAboutGameSelected()
{
	#Set GameDatabase
	$GameDatabase = $PlayniteApi.Mainview.Selectedgames | Where-Object { ($_.description) -and ($_.platform.name -eq "PC") }

	# Set counters
	$ChangedCount = 0

	# Regex
	$regex = '(?:[\s\S]+)<h1>About the Game<\/h1>([\s\S]+)'

	foreach ($game in $GameDatabase) {
		$RegexMatch = ([regex]$regex).Matches($($game.description))
		if ($RegexMatch.count -eq 1)
		{
			$game.description = '<h1>About the Game</h1>' + $RegexMatch.groups[1].value
			$PlayniteApi.Database.Games.Update($game)
			$ChangedCount++
		}
	}
	# Show finish dialogue with results
	$PlayniteApi.Dialogs.ShowMessage("Changed $ChangedCount games description ", "Description Remove `"About Game`"");
}