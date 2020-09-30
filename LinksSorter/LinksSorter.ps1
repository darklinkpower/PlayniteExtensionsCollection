function Format-Links()
{
	param (
		$GameDatabase
	)

	$SortedGames = 0
	foreach ($Game in $GameDatabase) {
		$SortedLinks = $Game.Links | Sort-Object -Property @{Expression = "Url"; Descending = $false}
		$SortedUrlOrder = $SortedLinks | Select-Object -Property url | Out-String
		$OriginalUrlOrder = $Game.Links | Select-Object -Property url | Out-String
		if ($OriginalUrlOrder -ne $SortedUrlOrder)
		{
			$Game.Links = $SortedLinks
			$PlayniteApi.Database.Games.Update($game)
			$SortedGames++
		}
	}
	
	# Show finish dialogue with shortcut creation count
	$PlayniteApi.Dialogs.ShowMessage("Sorted links of $SortedGames games", "Links Sorter");
}

function Format-SelectedGames()
{
	# Set GameDatabase
	$GameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.Links}
	
	Format-Links $GameDatabase
}

function Format-AllGames()
{
	# Set GameDatabase
	$GameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.Links}
	
	Format-Links $GameDatabase
}